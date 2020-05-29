using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;
using System;
using System.IO;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Compute Shader")]
	public class ComputeShaderNode : MixtureNode
	{
		[Serializable]
		public struct ComputeParameter
		{
			public string	displayName;
			public string	propertyName;
			public string	specificType; // In case the property is templated
			public Type		type;
		}

		[Input(name = "In")]
		public List< ComputeParameter >	computeInputs = new List<ComputeParameter>();
		[Output(name = "Out")]
		public List< ComputeParameter >	computeOutputs = new List<ComputeParameter>();

		[HideInInspector]
		public ComputeShader			computeShader;

        protected virtual IEnumerable<string> filteredOutProperties => Enumerable.Empty<string>();

		// We arbitrary take the first compute output that is a texture.
		// TODO: settings in the node for the name of the preview texture
		public override Texture previewTexture => nodePreview;

		// We don't use the 'Custom' part but we need a CRT for utility functions
		protected CustomRenderTexture nodePreview;

		internal string previewComputeProperty = "_Preview";
		[SerializeField]
		internal int			kernelIndex;
		[SerializeField]
		internal int			previewKernelIndex = -1;
		[SerializeField]
		internal List< string >	kernelNames = new List<string>();

		public override string			name => computeShader != null ? computeShader.name : "Compute Shader";

		public virtual string previewTexturePropertyName => previewComputeProperty;
		protected virtual string computeShaderResourcePath => null;
		protected virtual bool autoDetectInputs => true;
		protected virtual bool autoDetectOutputs => true;

		protected virtual string previewKernel => null;

		protected override void Enable()
		{
// We avoid to re-find the compute shader in build (if it was found with AssetDatabase, it will break the reference)
#if UNITY_EDITOR
			if (!String.IsNullOrEmpty(computeShaderResourcePath))
				computeShader = LoadComputeShader(computeShaderResourcePath);
#endif

			UpdateTempRenderTexture(ref nodePreview);
		}

		protected ComputeShader LoadComputeShader(string name)
		{
			var compute = Resources.Load<ComputeShader>(name);

#if UNITY_EDITOR
			var path = Path.GetDirectoryName(graph.mainAssetPath);

			if (compute == null)
				compute = UnityEditor.AssetDatabase.LoadAssetAtPath<ComputeShader>(path + "/" + name);
			if (compute == null)
				compute = UnityEditor.AssetDatabase.LoadAssetAtPath<ComputeShader>(path + "/" + graph.name + "/" + name);
#endif

			if (compute == null)
				Debug.LogError($"Couldn't find compute shader {name}, please place it in a Resources folder or in the mixture folder");

			return compute;
		}

		// Functions with Attributes must be either protected or public otherwise they can't be accessed by the reflection code
		[CustomPortBehavior(nameof(computeInputs))]
		public IEnumerable< PortData > ListComputeInputProperties(List< SerializableEdge > edges)
		{
			if (!autoDetectInputs)
				yield break;

			if (computeInputs != null)
			{
				foreach (var p in computeInputs)
				{
					yield return new PortData
					{
						displayName = p.displayName,
						displayType = p.type,
						identifier = p.propertyName,
						acceptMultipleEdges = false,
					};
				}
			}
		}

		[CustomPortBehavior(nameof(computeOutputs))]
		public IEnumerable< PortData > ListComputeOutputProperties(List< SerializableEdge > edges)
		{
			if (!autoDetectOutputs)
				yield break;

			if (computeOutputs != null)
			{
				foreach (var p in computeOutputs)
				{
					yield return new PortData
					{
						displayName = p.displayName,
						displayType = p.type,
						identifier = p.propertyName,
						acceptMultipleEdges = true,
					};
				}
			}
		}

		[CustomPortInput(nameof(computeInputs), typeof(object))]
		void GetMaterialInputs(List< SerializableEdge > edges)
		{
			foreach (var edge in edges)
			{
				switch (edge.passThroughBuffer)
				{
					case float f: computeShader.SetFloat(edge.inputPortIdentifier, f); break;
					case bool b: computeShader.SetBool(edge.inputPortIdentifier, b); break;
					case Vector2 v: computeShader.SetVector(edge.inputPortIdentifier, v); break;
					case Vector3 v: computeShader.SetVector(edge.inputPortIdentifier, v); break;
					case Vector4 v: computeShader.SetVector(edge.inputPortIdentifier, v); break;
					case Matrix4x4 m: computeShader.SetMatrix(edge.inputPortIdentifier, m); break;
					case Texture t: computeShader.SetTexture(kernelIndex, edge.inputPortIdentifier, t); break;
					case ComputeBuffer b: computeShader.SetBuffer(kernelIndex, edge.inputPortIdentifier, b); break;
					default: throw new Exception($"Can't assign {edge.passThroughBuffer} to a compute shader!");
				}
			}
		}

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			if (computeShader == null)
				return false;

			if (!ComputeIsValid())
				return false;

			UpdateTempRenderTexture(ref nodePreview);

			DispatchCompute(cmd, kernelIndex, rtSettings.GetWidth(graph), rtSettings.GetHeight(graph), rtSettings.GetDepth(graph));

			DispatchComputePreview(cmd);

			return true;
		}

		protected bool ComputeIsValid()
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

			return true;
		}

		Dictionary<int, (int x, int y, int z)> kernelGroupSizes = new Dictionary<int, (int a, int, int)>();

		protected void DispatchCompute(CommandBuffer cmd, int kernelIndex, int width, int height = 1, int depth = 1)
			=> DispatchCompute(cmd, computeShader, kernelIndex, width, height, depth);

		protected void DispatchCompute(CommandBuffer cmd, ComputeShader compute, int kernelIndex, int width, int height = 1, int depth = 1)
		{
			if (!kernelGroupSizes.ContainsKey(kernelIndex))
			{
				computeShader.GetKernelThreadGroupSizes(kernelIndex, out uint x, out uint y, out uint z);
				kernelGroupSizes[kernelIndex] = ((int)x, (int)y, (int)z);
			}

			var threadSizes = kernelGroupSizes[kernelIndex];

			if (width % threadSizes.x != 0 || height % threadSizes.y != 0 || depth % threadSizes.z != 0)
				Debug.LogError("DispatchCompute size must be a multiple of the kernel group thread size defined in the computeShader shader");

			cmd.DispatchCompute(computeShader, kernelIndex,
				Mathf.Max(1, width / threadSizes.x),
				Mathf.Max(1, height / threadSizes.y),
				Mathf.Max(1, depth / threadSizes.z)
			);
		}

		protected void DispatchComputePreview(CommandBuffer cmd)
		{
			// TODO
			if (previewTexture != null && previewKernelIndex != -1)
				DispatchCompute(cmd, previewKernelIndex, previewTexture.width, previewTexture.height, 1);
			var k = computeShader.FindKernel(previewKernel);
			// TODO: handle custom preview name
			computeShader.SetTexture(k, previewTexturePropertyName, nodePreview);
			DispatchCompute(cmd, k, nodePreview.width, nodePreview.height);
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