using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using GraphProcessor;

namespace Mixture
{
	[System.Serializable]
	public abstract class BaseFluidSimulationNode : ComputeShaderNode, IRealtimeReset
	{
		public enum BorderMode
		{
			Borders = 0,
			NoBorders = 1,
			Tile = 2,
		}

		CustomRenderTexture fluidBuffer;

		public override bool showDefaultInspector => true;

		protected override MixtureSettings defaultSettings
		{
			get
			{
				var settings = base.defaultSettings;
				settings.editFlags &= ~(EditFlags.Format);

				return settings;
			}
		}

		public override List<OutputDimension> supportedDimensions => new List<OutputDimension>() {
			OutputDimension.Texture2D,
			OutputDimension.Texture3D,
		};

		// For now only available in realtime mixtures, we'll see later for static with a spritesheet mode maybe
		[IsCompatibleWithGraph]
		static bool IsCompatibleWithRealtimeGraph(BaseGraph graph) => MixtureUtils.IsRealtimeGraph(graph);

		int advectKernel;
		int advectVelocityKernel;
		int applyBuoyancyKernel;
		int computeVorticityKernel;
		int computeConfinementKernel;
		int computeDivergenceKernel;
		int computePressureKernel;
		int computeProjectionKernel;
		int SetObstaclesKernel;
		int injectDensityKernel;
		int injectVelocityKernel;
		int injectObstaclesKernel;
		int extinguishmentImpulseKernel;

		List<RenderTexture> trackedRenderTextures = new List<RenderTexture>();

		protected Vector3 size;

        protected override void Enable()
        {
			base.Enable();

            // TODO: use this buffer!
			settings.doubleBuffered = true;
			settings.outputChannels = OutputChannel.RGBA;
			UpdateTempRenderTexture(ref fluidBuffer);

			advectKernel = computeShader.FindKernel("Advect");
			advectVelocityKernel = computeShader.FindKernel("AdvectVelocity");
			computeVorticityKernel = computeShader.FindKernel("ComputeVorticity");
			computeConfinementKernel = computeShader.FindKernel("ComputeConfinement");
			computeDivergenceKernel = computeShader.FindKernel("ComputeDivergence");
			computePressureKernel = computeShader.FindKernel("ComputePressure");
			computeProjectionKernel = computeShader.FindKernel("ComputeProjection");
			SetObstaclesKernel = computeShader.FindKernel("SetObstacles");
			injectDensityKernel = computeShader.FindKernel("InjectDensity");
			injectVelocityKernel = computeShader.FindKernel("InjectVelocity");
			injectObstaclesKernel = computeShader.FindKernel("InjectObstacles");

			// non mandatory kernels
			if (computeShader.HasKernel("ApplyBuoyancy"))
				applyBuoyancyKernel = computeShader.FindKernel("ApplyBuoyancy");
			if (computeShader.HasKernel("ExtinguishmentImpulse"))
				extinguishmentImpulseKernel = computeShader.FindKernel("ExtinguishmentImpulse");
        }

		public virtual void RealtimeReset() {}

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			if (!base.ProcessNode(cmd))
				return false;

			size = new Vector3(settings.GetResolvedWidth(graph), settings.GetResolvedHeight(graph), settings.GetResolvedDepth(graph));

            // Set size and epsilon
            cmd.SetComputeVectorParam(computeShader, "_Size", size);
			cmd.SetComputeFloatParam(computeShader, "_DeltaTime", GetDeltaTime());

			// Resize the render targets when changing the resolution
			foreach (var rt in trackedRenderTextures)
			{
				if (rt.width != settings.GetResolvedWidth(graph)
					|| rt.height != settings.GetResolvedHeight(graph)
					|| (rt.volumeDepth != settings.GetResolvedDepth(graph) && rt.dimension == TextureDimension.Tex3D))
				{
					rt.Release();
					rt.width = settings.GetResolvedWidth(graph);
					rt.height = settings.GetResolvedHeight(graph);
					if (rt.dimension == TextureDimension.Tex3D)
						rt.volumeDepth = settings.GetResolvedDepth(graph);
					rt.Create();
					ClearRenderTexture(rt);
				}
			}

			return true;
		}

		// delta time in ms
		protected virtual float GetDeltaTime() => graph.settings.GetUpdatePeriodInMilliseconds();

		protected RenderTexture AllocateRenderTexture(string name, GraphicsFormat format)
		{
			var rt = new RenderTexture(settings.GetResolvedWidth(graph), settings.GetResolvedHeight(graph), 0, format)
			{
				name = name,
				enableRandomWrite = true,
				volumeDepth = settings.GetResolvedDepth(graph),
				dimension = settings.GetResolvedTextureDimension(graph),
				hideFlags = HideFlags.HideAndDontSave,
			};

			trackedRenderTextures.Add(rt);

			ClearRenderTexture(rt);

			return rt;
		}

		protected void ClearRenderTexture(RenderTexture rt)
		{
			for (int i = 0; i < settings.GetResolvedDepth(graph); i++)
				Graphics.Blit(Texture2D.blackTexture, rt, 0, i);
		}

		protected const int READ = 0;
		protected const int WRITE = 1;
		protected const int PHI_N_HAT = 0;
		protected const int PHI_N_1_HAT = 1;

		protected void Swap(RenderTexture[] buffer)
		{
			RenderTexture tmp = buffer[READ];
			buffer[READ] = buffer[WRITE];
			buffer[WRITE] = tmp;
		}

		protected void ComputeObstacles(CommandBuffer cmd, RenderTexture obstacles, BorderMode borderMode)
		{
			cmd.BeginSample("ComputeObstacles");
			cmd.SetComputeTextureParam(computeShader, SetObstaclesKernel, "_Obstacles", obstacles);
			cmd.SetComputeFloatParam(computeShader, "_BorderMode", (int)borderMode);
			DispatchCompute(cmd, SetObstaclesKernel, (int)size.x, (int)size.y, (int)size.z);
			cmd.EndSample("ComputeObstacles");
		}
	
		protected void ApplyAdvection(CommandBuffer cmd, float dissipation, float decay, RenderTexture[] bufferToAdvect, RenderTexture velocity, RenderTexture obstacles)
		{
			cmd.BeginSample("ApplyAdvection");

			cmd.SetComputeFloatParam(computeShader, "_Dissipate", dissipation);
			cmd.SetComputeFloatParam(computeShader, "_Decay", decay);
			
			cmd.SetComputeTextureParam(computeShader, advectKernel, "_Read1f", bufferToAdvect[READ]);
			cmd.SetComputeTextureParam(computeShader, advectKernel, "_Write1f", bufferToAdvect[WRITE]);
			cmd.SetComputeTextureParam(computeShader, advectKernel, "_VelocityR", velocity);
			cmd.SetComputeTextureParam(computeShader, advectKernel, "_Obstacles", obstacles);
			
			DispatchCompute(cmd, advectKernel, (int)size.x, (int)size.y, (int)size.z);
			
			cmd.EndSample("ApplyAdvection");

			// This would be done after
			Swap(bufferToAdvect);
		}

		protected void ApplyAdvectionVelocity(CommandBuffer cmd, float velocityDissipation, RenderTexture[] velocity, RenderTexture obstacles, RenderTexture density, float densityWeight)
		{
			cmd.BeginSample("ApplyAdvectionVelocity");
			cmd.SetComputeFloatParam(computeShader, "_Dissipate", velocityDissipation);
			cmd.SetComputeFloatParam(computeShader, "_Forward", 1.0f);
			cmd.SetComputeFloatParam(computeShader, "_Decay", 0.0f);
			cmd.SetComputeFloatParam(computeShader, "_Density", 0.0f);
			cmd.SetComputeFloatParam(computeShader, "_Weight", densityWeight);

			cmd.SetComputeTextureParam(computeShader, advectVelocityKernel, "_Read3f", velocity[READ]);
			cmd.SetComputeTextureParam(computeShader, advectVelocityKernel, "_Write3f", velocity[WRITE]);
			cmd.SetComputeTextureParam(computeShader, advectVelocityKernel, "_VelocityR", velocity[READ]);
			cmd.SetComputeTextureParam(computeShader, advectVelocityKernel, "_Obstacles", obstacles);
			cmd.SetComputeTextureParam(computeShader, advectVelocityKernel, "_Density", density);
			
			DispatchCompute(cmd, advectVelocityKernel, (int)size.x, (int)size.y, (int)size.z);
			cmd.EndSample("ApplyAdvectionVelocity");

			Swap(velocity);
		}

		protected void ComputeVorticityAndConfinement(CommandBuffer cmd, RenderTexture vorticity, RenderTexture[] velocity, float vorticityStrength)
		{
			cmd.BeginSample("ComputeVorticity");
			
			cmd.SetComputeTextureParam(computeShader, computeVorticityKernel, "_Write", vorticity);
			cmd.SetComputeTextureParam(computeShader, computeVorticityKernel, "_VelocityR", velocity[READ]);
			
			DispatchCompute(cmd, computeVorticityKernel, (int)size.x, (int)size.y, (int)size.z);
			cmd.EndSample("ComputeVorticity");
			
			cmd.BeginSample("ComputeConfinement");
			cmd.SetComputeFloatParam(computeShader, "_Epsilon", vorticityStrength);
			
			cmd.SetComputeTextureParam(computeShader, computeConfinementKernel, "_Write", velocity[WRITE]);
			cmd.SetComputeTextureParam(computeShader, computeConfinementKernel, "_Read", velocity[READ]);
			cmd.SetComputeTextureParam(computeShader, computeConfinementKernel, "_Vorticity", vorticity);
			
			DispatchCompute(cmd, computeConfinementKernel, (int)size.x, (int)size.y, (int)size.z);
			cmd.EndSample("ComputeConfinement");
			
			Swap(velocity);
		}
		
		protected void ComputeDivergence(CommandBuffer cmd, RenderTexture velocity, RenderTexture obstacles, RenderTexture divergence)
		{
			cmd.BeginSample("ComputeDivergence");
			
			cmd.SetComputeTextureParam(computeShader, computeDivergenceKernel, "_Write1f", divergence);
			cmd.SetComputeTextureParam(computeShader, computeDivergenceKernel, "_VelocityR", velocity);
			cmd.SetComputeTextureParam(computeShader, computeDivergenceKernel, "_ObstaclesR", obstacles);
			
			DispatchCompute(cmd, computeDivergenceKernel, (int)size.x, (int)size.y, (int)size.z);
			cmd.EndSample("ComputeDivergence");
		}
		
		protected void ComputePressure(CommandBuffer cmd, RenderTexture divergence, RenderTexture obstacles, RenderTexture[] pressure, int iterationCount)
		{
			cmd.BeginSample("ComputePressure");
			cmd.SetComputeTextureParam(computeShader, computePressureKernel, "_Divergence", divergence);
			cmd.SetComputeTextureParam(computeShader, computePressureKernel, "_ObstaclesR", obstacles);

			for(int i = 0; i < iterationCount; i++)
			{
				cmd.SetComputeTextureParam(computeShader, computePressureKernel, "_Write1f", pressure[WRITE]);
				cmd.SetComputeTextureParam(computeShader, computePressureKernel, "_Pressure", pressure[READ]);
				
				DispatchCompute(cmd, computePressureKernel, (int)size.x, (int)size.y, (int)size.z);
				
				Swap(pressure);
			}
			cmd.EndSample("ComputePressure");
		}
		
		protected void ComputeProjection(CommandBuffer cmd, RenderTexture obstacles, RenderTexture pressure, RenderTexture[] velocity)
		{
			cmd.BeginSample("ComputeProjection");
			cmd.SetComputeTextureParam(computeShader, computeProjectionKernel, "_ObstaclesR", obstacles);
			
			cmd.SetComputeTextureParam(computeShader, computeProjectionKernel, "_Pressure", pressure);
			cmd.SetComputeTextureParam(computeShader, computeProjectionKernel, "_VelocityR", velocity[READ]);
			cmd.SetComputeTextureParam(computeShader, computeProjectionKernel, "_Write", velocity[WRITE]);
			
			DispatchCompute(cmd, computeProjectionKernel, (int)size.x, (int)size.y, (int)size.z);
			cmd.EndSample("ComputeProjection");
			
			Swap(velocity);
		}

		protected void ApplyBuoyancy(CommandBuffer cmd, RenderTexture temperature, RenderTexture[] velocity, float densityBuoyancy, float ambientTemperature)
		{
			cmd.BeginSample("ApplyBuoyancy");
			cmd.SetComputeVectorParam(computeShader, "_Up", new Vector4(0,1,0,0));
			cmd.SetComputeFloatParam(computeShader, "_Buoyancy", densityBuoyancy);
			cmd.SetComputeFloatParam(computeShader, "_AmbientTemperature", ambientTemperature);
			
			cmd.SetComputeTextureParam(computeShader, applyBuoyancyKernel, "_Write", velocity[WRITE]);
			cmd.SetComputeTextureParam(computeShader, applyBuoyancyKernel, "_VelocityR", velocity[READ]);
			cmd.SetComputeTextureParam(computeShader, applyBuoyancyKernel, "_Temperature", temperature);

			DispatchCompute(cmd, applyBuoyancyKernel, (int)size.x, (int)size.y, (int)size.z);
			cmd.EndSample("ApplyBuoyancy");
			
			Swap(velocity);
		}

		protected void InjectDensity(CommandBuffer cmd, float amount, Texture inputDensity, RenderTexture[] density)
		{
			if (inputDensity == null)
				return;

			cmd.BeginSample("InjectDensity");

			cmd.SetComputeFloatParam(computeShader, "_Amount", amount);
			cmd.SetComputeTextureParam(computeShader, injectDensityKernel, "_InputDensity", inputDensity);
			cmd.SetComputeTextureParam(computeShader, injectDensityKernel, "_Density", density[READ]);
			cmd.SetComputeTextureParam(computeShader, injectDensityKernel, "_Write", density[WRITE]);
			
			DispatchCompute(cmd, injectDensityKernel, (int)size.x, (int)size.y, (int)size.z);
			cmd.EndSample("InjectDensity");
			
			Swap(density);
		}

		protected void InjectVelocity(CommandBuffer cmd, Texture inputVelocity, RenderTexture[] velocity)
		{
			if (inputVelocity == null)
				return;

			cmd.BeginSample("InjectVelocity");

			cmd.SetComputeTextureParam(computeShader, injectVelocityKernel, "_InputVelocity", inputVelocity);
			cmd.SetComputeTextureParam(computeShader, injectVelocityKernel, "_VelocityR", velocity[READ]);
			cmd.SetComputeTextureParam(computeShader, injectVelocityKernel, "_Write", velocity[WRITE]);
			
			DispatchCompute(cmd, injectVelocityKernel, (int)size.x, (int)size.y, (int)size.z);
			cmd.EndSample("InjectVelocity");

			Swap(velocity);
		}

		protected void InjectObstacles(CommandBuffer cmd, Texture inputObstacles, RenderTexture obstacles)
		{
			if (inputObstacles == null)
				return;

			cmd.BeginSample("InjectObstacles");

			cmd.SetComputeTextureParam(computeShader, injectObstaclesKernel, "_InputObstacles", inputObstacles);
			cmd.SetComputeTextureParam(computeShader, injectObstaclesKernel, "_Write", obstacles);
			
			DispatchCompute(cmd, injectObstaclesKernel, (int)size.x, (int)size.y, (int)size.z);
			cmd.EndSample("InjectObstacles");
		}

		protected void ApplyExtinguishmentImpulse(CommandBuffer cmd, float densityAmount, RenderTexture reaction, RenderTexture[] density, float reactionExtinguishment)
		{
			cmd.BeginSample("ApplyExtinguishmentImpulse");

			cmd.SetComputeFloatParam(computeShader, "_Amount", densityAmount);
			cmd.SetComputeFloatParam(computeShader, "_Extinguishment", reactionExtinguishment);
			
			cmd.SetComputeTextureParam(computeShader, extinguishmentImpulseKernel, "_Read", density[READ]);
			cmd.SetComputeTextureParam(computeShader, extinguishmentImpulseKernel, "_Write", density[WRITE]);
			cmd.SetComputeTextureParam(computeShader, extinguishmentImpulseKernel, "_Reaction", reaction);

			DispatchCompute(cmd, extinguishmentImpulseKernel, (int)size.x, (int)size.y, (int)size.z);

			cmd.EndSample("ApplyExtinguishmentImpulse");
			
			Swap(density);
		}

	}
}