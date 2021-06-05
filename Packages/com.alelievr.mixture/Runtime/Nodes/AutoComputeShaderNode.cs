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
	[Documentation(@"
Compute Shader Node behaves like the Shader Node but with a Compute Shader.
Note that this node tries to generate input / output based on the declared properties in the compute shader, see the compute shader template for more information.
")]

	[System.Serializable, NodeMenuItem("Utils/Compute Shader")]
	public class AutoComputeShaderNode : ComputeShaderNode, ICreateNodeFrom<ComputeShader>
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

		[SerializeField]
		internal List<ResourceDescriptor> managedResources = new List<ResourceDescriptor>();

		[SerializeField]
		internal int			kernelIndex;
		[SerializeField]
		internal int			previewKernelIndex = -1;
		[SerializeField]
		internal List< string >	kernelNames = new List<string>();
		[SerializeField]
		internal string			resourcePath;

		public override string	name => computeShader != null ? computeShader.name : "Compute Shader";
		public override bool    showOpenButton => true;
        public override bool	isRenamable => true;

		public override string previewTexturePropertyName => previewComputeProperty;
		protected override string computeShaderResourcePath => resourcePath;

        // TODO: setting in the UI for this
		protected virtual string previewKernel => "Preview";

		public bool InitializeNodeFromObject(ComputeShader value)
		{
			computeShader = value;
			UpdateComputeShader();

			return true;
		}

		protected override void Enable()
		{
            base.Enable();
			if (!String.IsNullOrEmpty(computeShaderResourcePath) && computeShader == null)
				computeShader = LoadComputeShader(computeShaderResourcePath);

			UpdateTempRenderTexture(ref tempRenderTexture);

			foreach (var res in managedResources)
				AllocateResource(res);

			UpdateComputeShader();
		}

		internal void UpdateComputeShader()
		{
			if (computeShader == null)
				return;

			if (!String.IsNullOrEmpty(previewKernel) && computeShader.HasKernel(previewKernel))
				previewKernelIndex = computeShader.FindKernel(previewKernel);
		}

		void AllocateResource(ResourceDescriptor desc)
		{
			var t = desc.sType.type;
			if (t == typeof(ComputeBuffer))
			{
				desc.allocatedBuffer = AllocComputeBuffer(desc.bufferSize, desc.bufferStride);
			}
			else if (typeof(Texture).IsAssignableFrom(t))
			{
				int expectedWidth = desc.textureAllocMode == TextureAllocMode.SameAsOutput ? settings.GetResolvedWidth(graph) : (int)desc.textureAllocMode;
				int expectedHeight = desc.textureAllocMode == TextureAllocMode.SameAsOutput ? settings.GetResolvedHeight(graph) : (int)desc.textureAllocMode;
				int expectedDepth = desc.textureAllocMode == TextureAllocMode.SameAsOutput ? settings.GetResolvedDepth(graph) : (int)desc.textureAllocMode;

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
					hideFlags = HideFlags.HideAndDontSave,
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
				int expectedWidth = desc.textureAllocMode == TextureAllocMode.SameAsOutput ? settings.GetResolvedWidth(graph) : (int)desc.textureAllocMode;
				int expectedHeight = desc.textureAllocMode == TextureAllocMode.SameAsOutput ? settings.GetResolvedHeight(graph) : (int)desc.textureAllocMode;
				int expectedDepth = desc.textureAllocMode == TextureAllocMode.SameAsOutput ? settings.GetResolvedDepth(graph) : (int)desc.textureAllocMode;
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
		void AssignComputeInputs(List< SerializableEdge > edges)
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

			UpdateTempRenderTexture(ref tempRenderTexture);

            BindManagedResources(kernelIndex);

			cmd.SetComputeVectorParam(computeShader, "_Time", new Vector4(Time.realtimeSinceStartup, Mathf.Sin(Time.realtimeSinceStartup), Mathf.Cos(Time.realtimeSinceStartup), Time.deltaTime));
			DispatchCompute(cmd, kernelIndex, settings.GetResolvedWidth(graph), settings.GetResolvedHeight(graph), settings.GetResolvedDepth(graph));

            if (!String.IsNullOrEmpty(previewKernel))
            {
                int k = computeShader.FindKernel(previewKernel); 
                BindManagedResources(k);
                DispatchComputePreview(cmd, k);
            }

			return true;
		}

        void BindManagedResources(int kernel)
        {
			// Bind managed resources:
			foreach (var res in managedResources)
			{
				if (!res.autoAlloc)
					continue;

				if (res.allocatedTexture != null)
					computeShader.SetTexture(kernel, res.propertyName, res.allocatedTexture);

				if (res.allocatedBuffer != null)
					computeShader.SetBuffer(kernel, res.propertyName, res.allocatedBuffer);
			}
        }

		protected override void Disable()
		{
			base.Disable();

			tempRenderTexture?.Release();

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