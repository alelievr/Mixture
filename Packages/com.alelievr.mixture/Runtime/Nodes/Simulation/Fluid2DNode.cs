#if MIXTURE_EXPERIMENTAL
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
		[Input("Density")]
		public Texture inputDensity;

		[Input("Velocity")]
		public Texture inputVelocity;

		[Input]
		public Texture inputObstacles;
		
		[Output("Output Density")]
		public Texture outputDensity;
		[Output("Output Velocity")]
		public Texture outputVelocity;
		[Output("Output Pressure")]
		public Texture outputPressure;

		public int m_iterations = 10;
		public float m_vorticityStrength = 1.0f;
		public float m_densityAmount = 1.0f;
		public float m_densityDissipation = 0.99f;
		public float m_densityBuoyancy = 1.0f;
		public float m_densityWeight = 0.0125f;
		public float m_temperatureAmount = 10.0f;
		public float m_temperatureDissipation = 0.995f;
		public float m_velocityDissipation = 0.95f;
		float m_ambientTemperature = 0.0f;

		public override string name => "2D Fluid";

		protected override string computeShaderResourcePath => "Mixture/Fluid2D";

		public override bool showDefaultInspector => true;
		public override Texture previewTexture => outputDensity;

		protected override MixtureSettings defaultSettings
		{
			get
			{
				var settings = base.defaultSettings;
				settings.dimension = OutputDimension.Texture2D;
				return settings;
			}
		}

		public override List<OutputDimension> supportedDimensions => new List<OutputDimension>() {
			OutputDimension.Texture2D,
		};

		// For now only available in realtime mixtures, we'll see later for static with a spritesheet mode maybe
		[IsCompatibleWithGraph]
		static bool IsCompatibleWithRealtimeGraph(BaseGraph graph)
			=> (graph as MixtureGraph).type == MixtureGraphType.Realtime;

		Vector3 m_size;

		[SerializeField, HideInInspector]
		RenderTexture[] m_density, m_velocity, m_pressure, m_temperature;
		[SerializeField, HideInInspector]
		RenderTexture m_temp3f, m_obstacles;

        protected override void Enable()
        {
			base.Enable();

			settings.doubleBuffered = true;
			settings.outputChannels = OutputChannel.RGBA;

			m_density = new RenderTexture[2];
			m_density[READ] = AllocateRenderTexture("densityR", GraphicsFormat.R16_SFloat);
			m_density[WRITE] = AllocateRenderTexture("densityW", GraphicsFormat.R16_SFloat);
			
			m_temperature = new RenderTexture[2];
			m_temperature[READ] = AllocateRenderTexture("temperatureR", GraphicsFormat.R16_SFloat);
			m_temperature[WRITE] = AllocateRenderTexture("temperatureW", GraphicsFormat.R16_SFloat);
			
			m_velocity = new RenderTexture[2];
			m_velocity[READ] = AllocateRenderTexture("velocityR", GraphicsFormat.R16G16_SFloat);
			m_velocity[WRITE] = AllocateRenderTexture("velocityW", GraphicsFormat.R16G16_SFloat);
			
			m_pressure = new RenderTexture[2];
			m_pressure[READ] = AllocateRenderTexture("pressureR", GraphicsFormat.R16_SFloat);
			m_pressure[WRITE] = AllocateRenderTexture("pressureW", GraphicsFormat.R16_SFloat);
			
			m_obstacles = AllocateRenderTexture("Obstacles", GraphicsFormat.R8_SNorm);

			m_temp3f = AllocateRenderTexture("Temp", GraphicsFormat.R16G16_SFloat);
        }

		public override void RealtimeReset()
		{
			// Reset all temp textures

			ClearRenderTexture(m_density[READ]);
			ClearRenderTexture(m_velocity[READ]);
			ClearRenderTexture(m_temperature[READ]);
			ClearRenderTexture(m_pressure[READ]);
			ClearRenderTexture(m_obstacles);
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
			
			// TODO: code to update RTs when a setting change (size, not format)

			outputDensity = m_density[READ];
			outputVelocity = m_velocity[READ];
			outputPressure = m_pressure[READ];

			m_size = new Vector3(settings.GetResolvedWidth(graph), settings.GetResolvedHeight(graph), settings.GetResolvedDepth(graph));

			// UpdateTempRenderTexture(ref fluidBuffer);

			ComputeObstacles(cmd, m_obstacles);

			InjectObstacles(cmd, inputObstacles, m_obstacles);

			InjectVelocity(cmd, inputVelocity, m_velocity);

			//First off advect any buffers that contain physical quantities like density or temperature by the 
			//velocity field. Advection is what moves values around.
			ApplyAdvection(cmd, m_temperatureDissipation, 0.0f, m_temperature, m_velocity[READ], m_obstacles);
			ApplyAdvection(cmd, m_densityDissipation, 0.0f, m_density, m_velocity[READ], m_obstacles);

			//The velocity field also advects its self. 
			ApplyAdvectionVelocity(cmd, m_velocityDissipation, m_velocity, m_obstacles);
			
			//Apply the effect the sinking colder smoke has on the velocity field
			ApplyBuoyancy(cmd, m_density[READ], m_temperature[READ], m_velocity, m_densityBuoyancy, m_ambientTemperature, m_densityWeight);
			
			//Adds a certain amount of density (the visible smoke) and temperate
			InjectDensity(cmd, m_densityAmount, inputDensity, m_density);
			InjectDensity(cmd, m_densityAmount, inputDensity, m_temperature);
			
			//The fuild sim math tends to remove the swirling movement of fluids.
			//This step will try and add it back in
			ComputeVorticityAndConfinement(cmd, m_temp3f, m_velocity, m_vorticityStrength);
			
			//Compute the divergence of the velocity field. In fluid simulation the
			//fluid is modelled as being incompressible meaning that the volume of the fluid
			//does not change over time. The divergence is the amount the field has deviated from being divergence free
			ComputeDivergence(cmd, m_velocity[READ], m_obstacles, m_temp3f);
			
			//This computes the pressure need return the fluid to a divergence free condition
			ComputePressure(cmd, m_temp3f, m_obstacles, m_pressure, m_iterations);
			
			//Subtract the pressure field from the velocity field enforcing the divergence free conditions
			ComputeProjection(cmd, m_obstacles, m_pressure[READ], m_velocity);

			return true;
		}
	}
}
#endif