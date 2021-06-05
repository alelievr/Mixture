using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using GraphProcessor;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Simulation/2D Fluid")]
	public class Fluid2DNode : BaseFluidSimulationNode 
	{
		[Input]
		public Texture density;

		[Input]
		public Texture velocity;
		
		[Output]
		public Texture output;

		public float viscosity;

		// TODO: expose these settings as presets (water, smoke, ect...)
		public int m_iterations = 10;
		public float m_vorticityStrength = 1.0f;
		public float m_densityAmount = 1.0f;
		public float m_densityDissipation = 0.999f;
		public float m_densityBuoyancy = 1.0f;
		public float m_densityWeight = 0.0125f;
		public float m_temperatureAmount = 10.0f;
		public float m_temperatureDissipation = 0.995f;
		public float m_velocityDissipation = 0.995f;
		public float m_inputRadius = 0.04f;
		float m_ambientTemperature = 0.0f;
		public Vector4 m_inputPos = new Vector4(0.5f,0.1f,0.5f,0.0f);
		// [Output]
		// public Texture vectorField;
		// [Output]
		// public Texture outputDensity;

		CustomRenderTexture fluidBuffer;

		public override string name => "2D Fluid";

		protected override string computeShaderResourcePath => "Mixture/Fluid2D";

		public override bool showDefaultInspector => true;
		public override Texture previewTexture => output;

		// protected override MixtureSettings defaultRTSettings {
		// 	get
		// 	{
		// 		var settings = Get2DOnlyRTSettings(base.defaultRTSettings);
		// 		settings.editFlags &= ~(EditFlags.Format);
		// 		return settings;
		// 	}
		// }

		// public override List<OutputDimension> supportedDimensions => new List<OutputDimension>() {
		// 	OutputDimension.Texture2D,
		// };

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

		RenderTexture[] m_density, m_velocity, m_pressure, m_temperature;
		RenderTexture m_temp3f, m_obstacles;

		RenderTexture AllocateRenderTexture(string name, GraphicsFormat format)
		{
			var rt = new RenderTexture(settings.GetResolvedWidth(graph), settings.GetResolvedHeight(graph), 0, format)
			{
				name = name,
				enableRandomWrite = true,
				volumeDepth = settings.GetResolvedDepth(graph),
				dimension = TextureDimension.Tex3D,
				hideFlags = HideFlags.HideAndDontSave,
			};

			for (int i = 0; i < settings.GetResolvedDepth(graph); i++)
				Graphics.Blit(Texture2D.blackTexture, rt, 0, i);

			return rt;
		}

        protected override void Enable()
        {
			base.Enable();

			settings.doubleBuffered = true;
			settings.outputChannels = OutputChannel.RGBA;
			UpdateTempRenderTexture(ref fluidBuffer);

			m_density = new RenderTexture[2];
			m_density[READ] = AllocateRenderTexture("densityR", GraphicsFormat.R16_SFloat);
			m_density[WRITE] = AllocateRenderTexture("densityW", GraphicsFormat.R16_SFloat);
			
			m_temperature = new RenderTexture[2];
			m_temperature[READ] = AllocateRenderTexture("temperatureR", GraphicsFormat.R16_SFloat);
			m_temperature[WRITE] = AllocateRenderTexture("temperatureW", GraphicsFormat.R16_SFloat);
			
			m_velocity = new RenderTexture[2];
			m_velocity[READ] = AllocateRenderTexture("velocityR", GraphicsFormat.R16G16B16A16_SFloat);
			m_velocity[WRITE] = AllocateRenderTexture("velocityW", GraphicsFormat.R16G16B16A16_SFloat);
			
			m_pressure = new RenderTexture[2];
			m_pressure[READ] = AllocateRenderTexture("pressureR", GraphicsFormat.R16_SFloat);
			m_pressure[WRITE] = AllocateRenderTexture("pressureW", GraphicsFormat.R16_SFloat);
			
			m_obstacles = AllocateRenderTexture("Obstacles", GraphicsFormat.R8_SNorm);

			m_temp3f = AllocateRenderTexture("Temp", GraphicsFormat.R16G16B16A16_SFloat);

			// Find all kernels:
			advectKernel = computeShader.FindKernel("Advect");
			advectVelocityKernel = computeShader.FindKernel("AdvectVelocity");
			applyBuoyancyKernel = computeShader.FindKernel("ApplyBuoyancy");
			gaussImpulseKernel = computeShader.FindKernel("GaussImpulse");
			computeVorticityKernel = computeShader.FindKernel("ComputeVorticity");
			computeConfinementKernel = computeShader.FindKernel("ComputeConfinement");
			computeDivergenceKernel = computeShader.FindKernel("ComputeDivergence");
			computePressureKernel = computeShader.FindKernel("ComputePressure");
			computeProjectionKernel = computeShader.FindKernel("ComputeProjection");
			setBoundsKernel = computeShader.FindKernel("SetBounds");
        }

        protected override void Disable()
        {
			base.Disable();

			m_density[READ].Release();
			m_density[WRITE].Release();
			
			m_temperature[READ].Release();
			m_temperature[WRITE].Release();
			
			m_velocity[READ].Release();
			m_velocity[WRITE].Release();
			
			m_pressure[READ].Release();
			m_pressure[WRITE].Release();
			
			m_obstacles.Release();
			
			m_temp3f.Release();
        }

		// Source: GPU Gems 3 ch 38: Fast Fluid Dynamics Simulation on the GPU
		// and https://github.com/Scrawk/GPU-GEMS-3D-Fluid-Simulation
		protected override bool ProcessNode(CommandBuffer cmd)
		{
			if (!base.ProcessNode(cmd))
				return false;

			output = m_density[READ];

			m_size = new Vector3(settings.GetResolvedWidth(graph), settings.GetResolvedHeight(graph), settings.GetResolvedDepth(graph));

			UpdateTempRenderTexture(ref fluidBuffer);

			ComputeObstacles(cmd);

			// TODO: exposed parameter (time step)
			float dt = 0.1f;
			
			//First off advect any buffers that contain physical quantities like density or temperature by the 
			//velocity field. Advection is what moves values around.
			ApplyAdvection(cmd, dt, m_temperatureDissipation, 0.0f, m_temperature);
			ApplyAdvection(cmd, dt, m_densityDissipation, 0.0f, m_density);

			//The velocity field also advects its self. 
			ApplyAdvectionVelocity(cmd, dt);
			
			//Apply the effect the sinking colder smoke has on the velocity field
			ApplyBuoyancy(cmd, dt);
			
			//Adds a certain amount of density (the visible smoke) and temperate
			ApplyImpulse(cmd, dt, m_densityAmount, m_density);
			ApplyImpulse(cmd, dt,  m_temperatureAmount, m_temperature);
			
			//The fuild sim math tends to remove the swirling movement of fluids.
			//This step will try and add it back in
			ComputeVorticityConfinement(cmd, dt);
			
			//Compute the divergence of the velocity field. In fluid simulation the
			//fluid is modelled as being incompressible meaning that the volume of the fluid
			//does not change over time. The divergence is the amount the field has deviated from being divergence free
			ComputeDivergence(cmd);
			
			//This computes the pressure need return the fluid to a divergence free condition
			ComputePressure(cmd);
			
			//Subtract the pressure field from the velocity field enforcing the divergence free conditions
			ComputeProjection(cmd);

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

		// TODO: replace all Set* by command buffer version
		void ComputeObstacles(CommandBuffer cmd)
		{
			cmd.SetComputeVectorParam(computeShader, "_Size", m_size);
			cmd.SetComputeTextureParam(computeShader, setBoundsKernel, "_Obstacles", m_obstacles);
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