using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
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
			public SerializableType	sType;
		}

		[Serializable]
		public enum TextureAllocMode
		{
			SameAsOutput	= 0,
			_32				= 32,
			_64				= 64,
			_128			= 128,
			_256			= 256,
			_512			= 512,
			_1024			= 1024,
			_2048			= 2048,
			_4096			= 4096,
			_8192			= 8192,
			// Custom			= -1,
		}

		[Serializable]
		public class ResourceDescriptor
		{
			public string			propertyName;
			public bool				autoAlloc;
			public int				bufferStride = 4;
			public int				bufferSize = 1;

			public TextureAllocMode textureAllocMode = TextureAllocMode.SameAsOutput;
			public int				textureCustomWidth = 512;
			public int				textureCustomHeight = 512;
			public int				textureCustomDepth = 1;
			public SerializableType	sType;

			[NonSerialized]
			public RenderTexture	allocatedTexture;
			[NonSerialized]
			public ComputeBuffer	allocatedBuffer;
		}

		[Input(name = "In"), SerializeField]
		public List< ComputeParameter >	computeInputs = new List<ComputeParameter>();
		[Output(name = "Out"), SerializeField]
		public List< ComputeParameter >	computeOutputs = new List<ComputeParameter>();

		[HideInInspector]
		public ComputeShader			computeShader;

		[SerializeField]
		internal List<ResourceDescriptor> managedResources = new List<ResourceDescriptor>();

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
		/// <summary>This function also controls the output memory allocation</summary>
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

			foreach (var res in managedResources)
				AllocateResource(res);
		}

		void AllocateResource(ResourceDescriptor desc)
		{
			if (!autoDetectOutputs)
				return;

			var t = desc.sType.type;
			if (t == typeof(ComputeBuffer))
			{
				desc.allocatedBuffer = AllocComputeBuffer(desc.bufferSize, desc.bufferStride);
			}
			else if (typeof(Texture).IsAssignableFrom(t))
			{
				int expectedWidth = desc.textureAllocMode == TextureAllocMode.SameAsOutput ? rtSettings.GetWidth(graph) : (int)desc.textureAllocMode;
				int expectedHeight = desc.textureAllocMode == TextureAllocMode.SameAsOutput ? rtSettings.GetHeight(graph) : (int)desc.textureAllocMode;
				int expectedDepth = desc.textureAllocMode == TextureAllocMode.SameAsOutput ? rtSettings.GetDepth(graph) : (int)desc.textureAllocMode;

				RenderTextureDescriptor descriptor = new RenderTextureDescriptor
				{
					// TODO: custom size
					width = expectedWidth,
					height = expectedHeight,
					volumeDepth = expectedDepth,
					autoGenerateMips = false,
					useMipMap = false,
					graphicsFormat = GraphicsFormat.R16G16B16A16_SFloat,
					enableRandomWrite = true,
					depthBufferBits = 0,
					dimension = TextureUtils.GetDimensionFromType(t),
					msaaSamples = 1,
				};
				desc.allocatedTexture = new RenderTexture(descriptor)
				{
					name = "AutoAllocated - " + name,
				};
				desc.allocatedTexture.Create();
			}
		}

		void FreeResource(ResourceDescriptor desc)
		{
			if (!desc.autoAlloc)
				return;

			if (desc.allocatedTexture != null)
				CoreUtils.Destroy(desc.allocatedTexture);
			else if (desc.allocatedBuffer != null)
				desc.allocatedBuffer.Release();
			desc.allocatedTexture = null;
			desc.allocatedBuffer = null;
		}

		public void AddManagedResource(ResourceDescriptor desc)
		{
			AllocateResource(desc);
			managedResources.Add(desc);
		}

		public void RemoveManagedResource(ResourceDescriptor desc)
		{
			FreeResource(desc);
			managedResources.Remove(desc);
		}

		public void UpdateManagedResource(ResourceDescriptor desc)
		{
			if (desc.autoAlloc && desc.allocatedTexture == null && desc.allocatedBuffer == null)
				AllocateResource(desc);
			if (!desc.autoAlloc && (desc.allocatedTexture != null || desc.allocatedBuffer != null))
				FreeResource(desc);
			
			// TODO: check allocated size vs settings and do patch stuff
			if (desc.allocatedTexture != null)
			{
				var t = desc.allocatedTexture;
				int expectedWidth = desc.textureAllocMode == TextureAllocMode.SameAsOutput ? rtSettings.GetWidth(graph) : (int)desc.textureAllocMode;
				int expectedHeight = desc.textureAllocMode == TextureAllocMode.SameAsOutput ? rtSettings.GetHeight(graph) : (int)desc.textureAllocMode;
				int expectedDepth = desc.textureAllocMode == TextureAllocMode.SameAsOutput ? rtSettings.GetDepth(graph) : (int)desc.textureAllocMode;
				if (t.width != expectedWidth || t.height != expectedHeight || t.volumeDepth != expectedDepth)
				{
					t.Release();
					t.width = expectedWidth;
					t.height = expectedHeight;
					t.volumeDepth = expectedDepth;
					t.Create();
				}
			}
			if (desc.allocatedBuffer != null)
			{
				var b = desc.allocatedBuffer;

				if (b.stride != desc.bufferStride || b.count != desc.bufferSize)
				{
					b.Release();
					desc.allocatedBuffer = AllocComputeBuffer(desc.bufferSize, desc.bufferStride);
				}
			}
		}

		ComputeBuffer AllocComputeBuffer(int count, int stride)
		{
			count = Mathf.Max(1, count);
			stride = Mathf.Max(4, stride);
			return new ComputeBuffer(count, stride) { name = "AutoAlloc - " + name };
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
						displayType = p.sType.type,
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
					if (p.sType == null)
						continue;

					yield return new PortData
					{
						displayName = p.displayName,
						displayType = p.sType.type,
						identifier = p.propertyName,
						acceptMultipleEdges = true,
					};
				}
			}
		}

		List<(string propertyName, object value)> assignedInputs = new List<(string, object)>();

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
				assignedInputs.Add((edge.inputPortIdentifier, edge.passThroughBuffer));
			}
		}

		[CustomPortOutput(nameof(computeOutputs), typeof(object))]
		void ComputeShaderOutputs(List< SerializableEdge > edges)
		{
			foreach (var edge in edges)
			{
				// Output managed resources:
				foreach (var res in managedResources)
					if (res.propertyName == edge.outputPortIdentifier)
						edge.passThroughBuffer = (object)res.allocatedTexture ?? res.allocatedBuffer;

				// Output inputs in case they are RW:
				foreach (var input in assignedInputs)
					if (input.propertyName == edge.outputPortIdentifier)
						edge.passThroughBuffer = input.value;
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

			if (previewKernelIndex == -1)
				computeShader.SetTexture(kernelIndex, previewTexturePropertyName, nodePreview);
			
			// Bind managed resources:
			foreach (var res in managedResources)
			{
				if (!res.autoAlloc)
					continue;

				if (res.allocatedTexture != null)
					computeShader.SetTexture(kernelIndex, res.propertyName, res.allocatedTexture);

				if (res.allocatedBuffer != null)
					computeShader.SetBuffer(kernelIndex, res.propertyName, res.allocatedBuffer);
			}

			cmd.DispatchCompute(computeShader, kernelIndex,

				Mathf.Max(1, width / threadSizes.x),
				Mathf.Max(1, height / threadSizes.y),
				Mathf.Max(1, depth / threadSizes.z)
			);
		}

		protected void DispatchComputePreview(CommandBuffer cmd)
		{
			if (hasPreview && previewTexture != null)
			{
				int index = previewKernelIndex != -1 ? kernelIndex : previewKernelIndex;
				if (index > 0)
				{
					computeShader.SetTexture(index, previewTexturePropertyName, nodePreview);
					DispatchCompute(cmd, index, previewTexture.width, previewTexture.height, 1);
				}
			}
		}

		protected override void Disable()
		{
			base.Disable();

			foreach (var res in managedResources)
			{
				FreeResource(res);
			}
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