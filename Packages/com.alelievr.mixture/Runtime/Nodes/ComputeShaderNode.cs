using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;
using System;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Compute Shader")]
	public class ComputeShaderNode : MixtureNode
	{
		[Serializable]
		public struct ComputeParameter
		{
			public string	name;
			public string	specificType; // In case the property is templated
			public Type		type;
		}

		[Input(name = "In")]
		public List< ComputeParameter >	computeInputs;

		[Output(name = "Out")]
		public List< ComputeParameter >	computeOutputs;


		public ComputeShader			computeShader;

		public override string			name => computeShader != null ? computeShader.name : "Compute Shader";

        protected virtual IEnumerable<string> filteredOutProperties => Enumerable.Empty<string>();

		// We arbitrary take the first compute output that is a texture.
		// TODO: settings in the node for the name of the preview texture
		// public override Texture previewTexture => computeOutputs.FirstOrDefault(o => o is Texture) as Texture;

		[SerializeField]
		internal int			kernelIndex;

		[SerializeField]
		internal int			previewKernelIndex = -1;

		[SerializeField]
		internal List< string >	kernelNames;

		protected override void Enable()
		{
		}

		// Functions with Attributes must be either protected or public otherwise they can't be accessed by the reflection code
		[CustomPortBehavior(nameof(computeInputs))]
		public IEnumerable< PortData > ListComputeInputProperties(List< SerializableEdge > edges)
		{
			if (computeInputs != null)
			{
				foreach (var p in computeInputs)
				{
					yield return new PortData
					{
						displayName = p.name,
						displayType = p.type,
						identifier = p.name + p.type,
						acceptMultipleEdges = false,
					};
				}
			}
		}

		[CustomPortBehavior(nameof(computeInputs))]
		public IEnumerable< PortData > ListComputeOutputProperties(List< SerializableEdge > edges)
		{
			if (computeOutputs != null)
			{
				foreach (var p in computeOutputs)
				{
					yield return new PortData
					{
						displayName = p.name,
						displayType = p.type,
						identifier = p.name + p.type,
						acceptMultipleEdges = true,
					};
				}
			}
		}

		[CustomPortInput(nameof(computeInputs), typeof(object))]
		void GetMaterialInputs(List< SerializableEdge > edges)
		{
			foreach (var kernelIndex in GetKernelIndices())
			{
			}
			// AssignMaterialPropertiesFromEdges(edges, material);
			// TODO: assign compute inputs
		}

		protected virtual IEnumerable<int> GetKernelIndices()
		{
			yield return kernelIndex;
		}

		[CustomPortBehavior(nameof(computeOutputs))]
		protected IEnumerable< PortData > ChangeOutputPortType(List< SerializableEdge > edges)
		{
			yield return new PortData{
				displayName = "output",
				displayType = TextureUtils.GetTypeFromDimension(rtSettings.GetTextureDimension(graph)),
				identifier = "output",
				acceptMultipleEdges = true,
			};
		}

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			if (computeShader == null)
				return false;

#if UNITY_EDITOR // IsShaderCompiled is editor only
			if (!IsComputeShaderCompiled(computeShader))
			{
				Debug.LogError($"Can't process {name}, shader has errors.");
				LogComputeShaderErrors(computeShader);
				return false;
			}
#endif

			foreach (var kernelIndex in GetKernelIndices())
			{
				DispatchComputePixels(cmd, computeShader, kernelIndex, rtSettings.GetWidth(graph), rtSettings.GetHeight(graph), rtSettings.GetDepth(graph));
			}

			if (previewTexture != null && previewKernelIndex != -1)
				DispatchComputePixels(cmd, computeShader, previewKernelIndex, previewTexture.width, previewTexture.height, 1);

			return true;
		}

		Dictionary<int, (int x, int y, int z)> kernelGroupSizes = new Dictionary<int, (int a, int, int)>();

		protected void DispatchComputePixels(CommandBuffer cmd, ComputeShader compute, int kernelIndex, int width, int height, int depth)
		{
			if (!kernelGroupSizes.ContainsKey(kernelIndex))
			{
				compute.GetKernelThreadGroupSizes(kernelIndex, out uint x, out uint y, out uint z);
				kernelGroupSizes[kernelIndex] = ((int)x, (int)y, (int)z);
			}

			var threadSizes = kernelGroupSizes[kernelIndex];

			if (width % threadSizes.x != 0 || height % threadSizes.y != 0 || depth % threadSizes.z != 0)
				Debug.LogError("DispatchComputePixels size must be a multiple of the kernel group thread size defined in the compute shader");

			cmd.DispatchCompute(compute, kernelIndex, width / threadSizes.x, height / threadSizes.y, depth / threadSizes.z);
		}

#if UNITY_EDITOR
		static bool IsComputeShaderCompiled(ComputeShader computeShader)
		{
			foreach (var message in UnityEditor.ShaderUtil.GetComputeShaderMessages(computeShader))
				if (message.severity == UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error)
					return false;
			return true;
		}

		void LogComputeShaderErrors(ComputeShader computeShader)
		{
			foreach (var m in UnityEditor.ShaderUtil.GetComputeShaderMessages(computeShader).Where(m => m.severity == UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error))
			{
				string file = String.IsNullOrEmpty(m.file) ? computeShader.name : m.file;
				Debug.LogError($"{file}:{m.line} {m.message} {m.messageDetails}");
			}
		}
#endif
    }
}