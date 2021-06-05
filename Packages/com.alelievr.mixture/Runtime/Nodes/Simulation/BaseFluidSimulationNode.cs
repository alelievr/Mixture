using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using GraphProcessor;

namespace Mixture
{
	[System.Serializable]
	public abstract class BaseFluidSimulationNode : ComputeShaderNode 
	{
		public enum BorderMode
		{
			Borders,
			NoBorders,
			Tile,
		}

		public float simulationSpeed;

		[ShowInInspector]
		public BorderMode borderMode;

		CustomRenderTexture fluidBuffer;

		public override string name => "2D Fluid";

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
		static bool IsCompatibleWithRealtimeGraph(BaseGraph graph)
			=> (graph as MixtureGraph).type == MixtureGraphType.Realtime;

		int advectKernel;
		int advectVelocityKernel;
		int applyBuoyancyKernel;
		int computeVorticityKernel;
		int computeConfinementKernel;
		int computeDivergenceKernel;
		int computePressureKernel;
		int computeProjectionKernel;
		int setBoundsKernel;
		int injectDensityKernel;

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
			applyBuoyancyKernel = computeShader.FindKernel("ApplyBuoyancy");
			computeVorticityKernel = computeShader.FindKernel("ComputeVorticity");
			computeConfinementKernel = computeShader.FindKernel("ComputeConfinement");
			computeDivergenceKernel = computeShader.FindKernel("ComputeDivergence");
			computePressureKernel = computeShader.FindKernel("ComputePressure");
			computeProjectionKernel = computeShader.FindKernel("ComputeProjection");
			setBoundsKernel = computeShader.FindKernel("SetBounds");
			injectDensityKernel = computeShader.FindKernel("InjectDensity");
        }

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			if (!base.ProcessNode(cmd))
				return false;

			size = new Vector3(settings.GetResolvedWidth(graph), settings.GetResolvedHeight(graph), settings.GetResolvedDepth(graph));

            // Set size and epsilon
            cmd.SetComputeVectorParam(computeShader, "_Size", size);

			return true;
		}

		protected virtual float GetDeltaTime() => 0.1f; // TODO: expose setting

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

			for (int i = 0; i < settings.GetResolvedDepth(graph); i++)
				Graphics.Blit(Texture2D.blackTexture, rt, 0, i);

			return rt;
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

		protected void ComputeObstacles(CommandBuffer cmd, RenderTexture obstacles)
		{
			cmd.BeginSample("ComputeObstacles");
			cmd.SetComputeVectorParam(computeShader, "_Size", size);
			cmd.SetComputeTextureParam(computeShader, setBoundsKernel, "_Obstacles", obstacles);
			DispatchCompute(cmd, setBoundsKernel, (int)size.x, (int)size.y, (int)size.z);
			cmd.EndSample("ComputeObstacles");
		}
	
		protected void ApplyAdvection(CommandBuffer cmd, float dissipation, float decay, RenderTexture[] bufferToAdvect, RenderTexture velocity, RenderTexture obstacles)
		{
			cmd.BeginSample("ApplyAdvection");

			cmd.SetComputeVectorParam(computeShader, "_Size", size);
			cmd.SetComputeFloatParam(computeShader, "_DeltaTime", GetDeltaTime());
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

		protected void ApplyAdvectionVelocity(CommandBuffer cmd, float velocityDissipation, RenderTexture[] velocity, RenderTexture obstacles)
		{
			cmd.BeginSample("ApplyAdvectionVelocity");
			cmd.SetComputeVectorParam(computeShader, "_Size", size);
			cmd.SetComputeFloatParam(computeShader, "_DeltaTime", GetDeltaTime());
			cmd.SetComputeFloatParam(computeShader, "_Dissipate", velocityDissipation);
			cmd.SetComputeFloatParam(computeShader, "_Forward", 1.0f);
			cmd.SetComputeFloatParam(computeShader, "_Decay", 0.0f);
			
			cmd.SetComputeTextureParam(computeShader, advectVelocityKernel, "_Read3f", velocity[READ]);
			cmd.SetComputeTextureParam(computeShader, advectVelocityKernel, "_Write3f", velocity[WRITE]);
			cmd.SetComputeTextureParam(computeShader, advectVelocityKernel, "_VelocityR", velocity[READ]);
			cmd.SetComputeTextureParam(computeShader, advectVelocityKernel, "_Obstacles", obstacles);
			
			DispatchCompute(cmd, advectVelocityKernel, (int)size.x, (int)size.y, (int)size.z);
			cmd.EndSample("ApplyAdvectionVelocity");

			Swap(velocity);
		}

		protected void ComputeVorticityAndConfinement(CommandBuffer cmd, RenderTexture vorticity, RenderTexture[] velocity, float vorticityStrength)
		{
			cmd.BeginSample("ComputeVorticity");
			cmd.SetComputeVectorParam(computeShader, "_Size", size);
			
			cmd.SetComputeTextureParam(computeShader, computeVorticityKernel, "_Write", vorticity);
			cmd.SetComputeTextureParam(computeShader, computeVorticityKernel, "_VelocityR", velocity[READ]);
			
			DispatchCompute(cmd, computeVorticityKernel, (int)size.x, (int)size.y, (int)size.z);
			cmd.EndSample("ComputeVorticity");
			
			cmd.BeginSample("ComputeConfinement");
			cmd.SetComputeVectorParam(computeShader, "_Size", size);
			cmd.SetComputeFloatParam(computeShader, "_DeltaTime", GetDeltaTime());
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
			cmd.SetComputeVectorParam(computeShader, "_Size", size);
			
			cmd.SetComputeTextureParam(computeShader, computeDivergenceKernel, "_Write1f", divergence);
			cmd.SetComputeTextureParam(computeShader, computeDivergenceKernel, "_VelocityR", velocity);
			cmd.SetComputeTextureParam(computeShader, computeDivergenceKernel, "_ObstaclesR", obstacles);
			
			DispatchCompute(cmd, computeDivergenceKernel, (int)size.x, (int)size.y, (int)size.z);
			cmd.EndSample("ComputeDivergence");
		}
		
		protected void ComputePressure(CommandBuffer cmd, RenderTexture divergence, RenderTexture obstacles, RenderTexture[] pressure, int iterationCount)
		{
			cmd.BeginSample("ComputePressure");
			cmd.SetComputeVectorParam(computeShader, "_Size", size);
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
			cmd.SetComputeVectorParam(computeShader, "_Size", size);
			cmd.SetComputeTextureParam(computeShader, computeProjectionKernel, "_ObstaclesR", obstacles);
			
			cmd.SetComputeTextureParam(computeShader, computeProjectionKernel, "_Pressure", pressure);
			cmd.SetComputeTextureParam(computeShader, computeProjectionKernel, "_VelocityR", velocity[READ]);
			cmd.SetComputeTextureParam(computeShader, computeProjectionKernel, "_Write", velocity[WRITE]);
			
			DispatchCompute(cmd, computeProjectionKernel, (int)size.x, (int)size.y, (int)size.z);
			cmd.EndSample("ComputeProjection");
			
			Swap(velocity);
		}

		protected void ApplyBuoyancy(CommandBuffer cmd, RenderTexture density, RenderTexture temperature, RenderTexture[] velocity, float densityBuoyancy, float ambientTemperature, float densityWeight)
		{
			cmd.BeginSample("ApplyBuoyancy");
			cmd.SetComputeVectorParam(computeShader, "_Size", size);
			cmd.SetComputeVectorParam(computeShader, "_Up", new Vector4(0,1,0,0));
			cmd.SetComputeFloatParam(computeShader, "_Buoyancy", densityBuoyancy);
			cmd.SetComputeFloatParam(computeShader, "_AmbientTemperature", ambientTemperature);
			cmd.SetComputeFloatParam(computeShader, "_Weight", densityWeight);
			cmd.SetComputeFloatParam(computeShader, "_DeltaTime", GetDeltaTime());
			
			cmd.SetComputeTextureParam(computeShader, applyBuoyancyKernel, "_Write", velocity[WRITE]);
			cmd.SetComputeTextureParam(computeShader, applyBuoyancyKernel, "_VelocityR", velocity[READ]);
			cmd.SetComputeTextureParam(computeShader, applyBuoyancyKernel, "_Density", density);
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
			cmd.SetComputeVectorParam(computeShader, "_Size", size);
			cmd.SetComputeFloatParam(computeShader, "_Amount", amount);
			cmd.SetComputeFloatParam(computeShader, "_DeltaTime", GetDeltaTime());

			cmd.SetComputeTextureParam(computeShader, injectDensityKernel, "_InputDensity", inputDensity);
			cmd.SetComputeTextureParam(computeShader, injectDensityKernel, "_Density", density[READ]);
			cmd.SetComputeTextureParam(computeShader, injectDensityKernel, "_Write", density[WRITE]);
			
			DispatchCompute(cmd, injectDensityKernel, (int)size.x, (int)size.y, (int)size.z);
			cmd.EndSample("InjectDensity");
			
			Swap(density);
		}
	}
}