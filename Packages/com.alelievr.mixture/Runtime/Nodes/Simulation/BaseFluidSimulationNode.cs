using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using GraphProcessor;

namespace Mixture
{
	[System.Serializable]
	public class BaseFluidSimulationNode : ComputeShaderNode 
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
		public override Texture previewTexture => output;

		protected override MixtureSettings defaultRTSettings {
			get
			{
				var settings = base.defaultRTSettings;
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
		int gaussImpulseKernel;
		int computeVorticityKernel;
		int computeConfinementKernel;
		int computeDivergenceKernel;
		int computePressureKernel;
		int computeProjectionKernel;
		int setBoundsKernel;

		Vector3 m_size;

        protected override void Enable()
        {
			base.Enable();

            // TODO: use this buffer!
			rtSettings.doubleBuffered = true;
			rtSettings.outputChannels = OutputChannel.RGBA;
			UpdateTempRenderTexture(ref fluidBuffer);
        }

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			if (!base.ProcessNode(cmd))
				return false;

			m_size = new Vector3(rtSettings.GetWidth(graph), rtSettings.GetHeight(graph), rtSettings.GetDepth(graph));

            // Set size and epsilon
            cmd.SetComputeVectorParam(computeShader, "_Size", m_size);

			return true;
		}

		const int READ = 0;
		const int WRITE = 1;
		const int PHI_N_HAT = 0;
		const int PHI_N_1_HAT = 1;

		void Swap(RenderTexture[] buffer)
		{
			RenderTexture tmp = buffer[READ];
			buffer[READ] = buffer[WRITE];
			buffer[WRITE] = tmp;
		}

		void ComputeObstacles(CommandBuffer cmd, RenderTexture obstacles)
		{
			cmd.SetComputeVectorParam(computeShader, "_Size", m_size);
			cmd.SetComputeTextureParam(computeShader, setBoundsKernel, "_Obstacles", obstacles);
			DispatchCompute(cmd, setBoundsKernel, (int)m_size.x, (int)m_size.y, (int)m_size.z);
		}
	
		void ApplyAdvection(CommandBuffer cmd, float dt, float dissipation, float decay, RenderTexture[] buffer)
		{
			cmd.BeginSample("ApplyAdvection");

			cmd.SetComputeVectorParam(computeShader, "_Size", m_size);
			cmd.SetComputeFloatParam(computeShader, "_DeltaTime", dt);
			cmd.SetComputeFloatParam(computeShader, "_Dissipate", dissipation);
			cmd.SetComputeFloatParam(computeShader, "_Decay", decay);
			
			cmd.SetComputeTextureParam(computeShader, advectKernel, "_Read1f", buffer[READ]);
			cmd.SetComputeTextureParam(computeShader, advectKernel, "_Write1f", buffer[WRITE]);
			cmd.SetComputeTextureParam(computeShader, advectKernel, "_VelocityR", m_velocity[READ]);
			cmd.SetComputeTextureParam(computeShader, advectKernel, "_Obstacles", m_obstacles);
			
			DispatchCompute(cmd, advectKernel, (int)m_size.x, (int)m_size.y, (int)m_size.z);
			
			cmd.EndSample("ApplyAdvection");
			Swap(buffer);
		}

		void ApplyAdvectionVelocity(CommandBuffer cmd, float dt)
		{
			cmd.BeginSample("ApplyAdvectionVelocity");
			cmd.SetComputeVectorParam(computeShader, "_Size", m_size);
			cmd.SetComputeFloatParam(computeShader, "_DeltaTime", dt);
			cmd.SetComputeFloatParam(computeShader, "_Dissipate", m_velocityDissipation);
			cmd.SetComputeFloatParam(computeShader, "_Forward", 1.0f);
			cmd.SetComputeFloatParam(computeShader, "_Decay", 0.0f);
			
			cmd.SetComputeTextureParam(computeShader, advectVelocityKernel, "_Read3f", m_velocity[READ]);
			cmd.SetComputeTextureParam(computeShader, advectVelocityKernel, "_Write3f", m_velocity[WRITE]);
			cmd.SetComputeTextureParam(computeShader, advectVelocityKernel, "_VelocityR", m_velocity[READ]);
			cmd.SetComputeTextureParam(computeShader, advectVelocityKernel, "_Obstacles", m_obstacles);
			
			DispatchCompute(cmd, advectVelocityKernel, (int)m_size.x, (int)m_size.y, (int)m_size.z);
			cmd.EndSample("ApplyAdvectionVelocity");

			Swap(m_velocity);
		}

		void ComputeVorticityConfinement(CommandBuffer cmd, float dt)
		{
			cmd.BeginSample("ComputeVorticity");
			cmd.SetComputeVectorParam(computeShader, "_Size", m_size);
			
			cmd.SetComputeTextureParam(computeShader, computeVorticityKernel, "_Write", m_temp3f);
			cmd.SetComputeTextureParam(computeShader, computeVorticityKernel, "_VelocityR", m_velocity[READ]);
			
			DispatchCompute(cmd, computeVorticityKernel, (int)m_size.x, (int)m_size.y, (int)m_size.z);
			cmd.EndSample("ComputeVorticity");
			
			cmd.BeginSample("ComputeConfinement");
			cmd.SetComputeVectorParam(computeShader, "_Size", m_size);
			cmd.SetComputeFloatParam(computeShader, "_DeltaTime", dt);
			cmd.SetComputeFloatParam(computeShader, "_Epsilon", m_vorticityStrength);
			
			cmd.SetComputeTextureParam(computeShader, computeConfinementKernel, "_Write", m_velocity[WRITE]);
			cmd.SetComputeTextureParam(computeShader, computeConfinementKernel, "_Read", m_velocity[READ]);
			cmd.SetComputeTextureParam(computeShader, computeConfinementKernel, "_Vorticity", m_temp3f);
			
			DispatchCompute(cmd, computeConfinementKernel, (int)m_size.x, (int)m_size.y, (int)m_size.z);
			cmd.EndSample("ComputeConfinement");
			
			Swap(m_velocity);
		}
		
		void ComputeDivergence(CommandBuffer cmd)
		{
			cmd.BeginSample("ComputeDivergence");
			cmd.SetComputeVectorParam(computeShader, "_Size", m_size);
			
			cmd.SetComputeTextureParam(computeShader, computeDivergenceKernel, "_Write1f", m_temp3f);
			cmd.SetComputeTextureParam(computeShader, computeDivergenceKernel, "_VelocityR", m_velocity[READ]);
			cmd.SetComputeTextureParam(computeShader, computeDivergenceKernel, "_ObstaclesR", m_obstacles);
			
			DispatchCompute(cmd, computeDivergenceKernel, (int)m_size.x, (int)m_size.y, (int)m_size.z);
			cmd.EndSample("ComputeDivergence");
		}
		
		void ComputePressure(CommandBuffer cmd)
		{
			cmd.BeginSample("ComputePressure");
			cmd.SetComputeVectorParam(computeShader, "_Size", m_size);
			cmd.SetComputeTextureParam(computeShader, computePressureKernel, "_Divergence", m_temp3f);
			cmd.SetComputeTextureParam(computeShader, computePressureKernel, "_ObstaclesR", m_obstacles);
			
			for(int i = 0; i < m_iterations; i++)
			{
				cmd.SetComputeTextureParam(computeShader, computePressureKernel, "_Write1f", m_pressure[WRITE]);
				cmd.SetComputeTextureParam(computeShader, computePressureKernel, "_Pressure", m_pressure[READ]);
				
				DispatchCompute(cmd, computePressureKernel, (int)m_size.x, (int)m_size.y, (int)m_size.z);
				
				Swap(m_pressure);
			}
			cmd.EndSample("ComputePressure");
		}
		
		void ComputeProjection(CommandBuffer cmd)
		{
			cmd.BeginSample("ComputeProjection");
			cmd.SetComputeVectorParam(computeShader, "_Size", m_size);
			cmd.SetComputeTextureParam(computeShader, computeProjectionKernel, "_ObstaclesR", m_obstacles);
			
			cmd.SetComputeTextureParam(computeShader, computeProjectionKernel, "_Pressure", m_pressure[READ]);
			cmd.SetComputeTextureParam(computeShader, computeProjectionKernel, "_VelocityR", m_velocity[READ]);
			cmd.SetComputeTextureParam(computeShader, computeProjectionKernel, "_Write", m_velocity[WRITE]);
			
			DispatchCompute(cmd, computeProjectionKernel, (int)m_size.x, (int)m_size.y, (int)m_size.z);
			cmd.EndSample("ComputeProjection");
			
			Swap(m_velocity);
		}

		void ApplyBuoyancy(CommandBuffer cmd, float dt)
		{
			cmd.BeginSample("ApplyBuoyancy");
			cmd.SetComputeVectorParam(computeShader, "_Size", m_size);
			cmd.SetComputeVectorParam(computeShader, "_Up", new Vector4(0,1,0,0));
			cmd.SetComputeFloatParam(computeShader, "_Buoyancy", m_densityBuoyancy);
			cmd.SetComputeFloatParam(computeShader, "_AmbientTemperature", m_ambientTemperature);
			cmd.SetComputeFloatParam(computeShader, "_Weight", m_densityWeight);
			cmd.SetComputeFloatParam(computeShader, "_DeltaTime", dt);
			
			cmd.SetComputeTextureParam(computeShader, applyBuoyancyKernel, "_Write", m_velocity[WRITE]);
			cmd.SetComputeTextureParam(computeShader, applyBuoyancyKernel, "_VelocityR", m_velocity[READ]);
			cmd.SetComputeTextureParam(computeShader, applyBuoyancyKernel, "_Density", m_density[READ]);
			cmd.SetComputeTextureParam(computeShader, applyBuoyancyKernel, "_Temperature", m_temperature[READ]);

			DispatchCompute(cmd, applyBuoyancyKernel, (int)m_size.x, (int)m_size.y, (int)m_size.z);
			cmd.EndSample("ApplyBuoyancy");
			
			Swap(m_velocity);
		}

		void ApplyImpulse(CommandBuffer cmd, float dt, float amount, RenderTexture[] buffer)
		{
			cmd.BeginSample("ApplyImpulse");
			cmd.SetComputeVectorParam(computeShader, "_Size", m_size);
			cmd.SetComputeFloatParam(computeShader, "_Radius", m_inputRadius);
			cmd.SetComputeFloatParam(computeShader, "_Amount", amount);
			cmd.SetComputeFloatParam(computeShader, "_DeltaTime", dt);
			cmd.SetComputeVectorParam(computeShader, "_Pos", m_inputPos);
			
			cmd.SetComputeTextureParam(computeShader, gaussImpulseKernel, "_Read", buffer[READ]);
			cmd.SetComputeTextureParam(computeShader, gaussImpulseKernel, "_Write", buffer[WRITE]);
			
			DispatchCompute(cmd, gaussImpulseKernel, (int)m_size.x, (int)m_size.y, (int)m_size.z);
			cmd.EndSample("ApplyImpulse");
			
			Swap(buffer);
		}
	}
}