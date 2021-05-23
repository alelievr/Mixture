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
	[System.Serializable]
	public abstract class ComputeShaderNode : MixtureNode
	{
		[HideInInspector]
		public ComputeShader			computeShader;

		static readonly int previewResolutionId = Shader.PropertyToID("_PreviewResolution");

		// We arbitrary take the first compute output that is a texture.
		// TODO: settings in the node for the name of the preview texture
		public override Texture previewTexture => tempRenderTexture;

		// We don't use the 'Custom' part but we need a CRT for utility functions
		protected CustomRenderTexture tempRenderTexture;

		internal string previewComputeProperty = "_Preview";

		public override string			name => computeShader != null ? computeShader.name : "Compute Shader";

		public virtual string previewTexturePropertyName => previewComputeProperty;
		protected abstract string computeShaderResourcePath { get; }

		public virtual bool showOpenButton => false;

		protected virtual bool tempRenderTextureHasMipmaps => false;
		protected virtual bool tempRenderTextureHasDepthBuffer => false;

		protected override void Enable()
		{
            base.Enable();
			if (!String.IsNullOrEmpty(computeShaderResourcePath) && computeShader == null)
				computeShader = LoadComputeShader(computeShaderResourcePath);

			UpdateTempRenderTexture(ref tempRenderTexture, hasMips: tempRenderTextureHasMipmaps, depthBuffer: tempRenderTextureHasDepthBuffer);

			beforeProcessSetup += UpdateTempRT;
			afterProcessCleanup += UpdateTempRT;
		}

		// By overriding this function, we mark this node as dependent of the graph, so it will be update
		// so it will be updated when the graph dimension changes (the ports will be correct when we open the create from edge menu)
		[IsCompatibleWithGraph]
		protected static bool IsCompatibleWithGraph(BaseGraph graph) => true;

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

		public override bool canProcess => ComputeIsValid();

		void UpdateTempRT()
		{
			// Update the temp RT so users that overrides processNode don't have to do it
			UpdateTempRenderTexture(ref tempRenderTexture, hasMips: tempRenderTextureHasMipmaps, depthBuffer: tempRenderTextureHasDepthBuffer);
		}

		public bool ComputeIsValid()
		{
			ClearMessages();

			if (computeShader == null)
			{
				LoadComputeShader(computeShaderResourcePath);
				if (computeShader == null)
				{
					AddMessage($"Compute Shader Can't be found", NodeMessageType.Error);
					return false;
				}
			}
			
#if UNITY_EDITOR // IsShaderCompiled is editor only
			if (!IsComputeShaderCompiled(computeShader))
			{
				foreach (var m in UnityEditor.ShaderUtil.GetComputeShaderMessages(computeShader).Where(m => m.severity == UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error))
				{
					string file = String.IsNullOrEmpty(m.file) ? computeShader.name : m.file;
					AddMessage($"{file}:{m.line} {m.message} {m.messageDetails}", NodeMessageType.Error);
				}
				return false;
			}
#endif

			return true;
		}

		protected void DispatchCompute(CommandBuffer cmd, int kernelIndex, int width, int height = 1, int depth = 1)
			=> DispatchCompute(cmd, computeShader, kernelIndex, width, height, depth);

		protected void DispatchCompute(CommandBuffer cmd, ComputeShader compute, int kernelIndex, int width, int height = 1, int depth = 1)
		{
			computeShader.GetKernelThreadGroupSizes(kernelIndex, out uint x, out uint y, out uint z);

			if (width % x != 0 || height % y != 0 || depth % z != 0)
				Debug.LogError("DispatchCompute size must be a multiple of the kernel group thread size defined in the computeShader shader");

			// Bind the preview texture as well in case users write to it
			cmd.SetComputeTextureParam(compute, kernelIndex, previewTexturePropertyName, previewTexture);
			cmd.SetComputeVectorParam(compute, previewResolutionId, new Vector4(previewTexture.width, previewTexture.height, 1.0f / previewTexture.width, 1.0f / previewTexture.height));

			cmd.DispatchCompute(computeShader, kernelIndex,
				Mathf.Max(1, width / (int)x),
				Mathf.Max(1, height / (int)y),
				Mathf.Max(1, depth / (int)z)
			);
		}

		protected void DispatchComputePreview(CommandBuffer cmd, int previewKernel)
			=> DispatchComputePreview(cmd, computeShader, previewKernel);

		protected void DispatchComputePreview(CommandBuffer cmd, ComputeShader compute, int previewKernel)
		{
			if (hasPreview && previewTexture != null)
			{
				if (previewKernel != -1)
				{
					cmd.SetComputeTextureParam(compute, previewKernel, previewTexturePropertyName, previewTexture);
					cmd.SetComputeVectorParam(compute, previewResolutionId, new Vector4(previewTexture.width, previewTexture.height, 1.0f / previewTexture.width, 1.0f / previewTexture.height));
					DispatchCompute(cmd, compute, previewKernel, previewTexture.width, previewTexture.height, 1);
				}
				else
				{
					Debug.LogError($"Invalid previewKernel in {compute}");
				}
			}
		}

		protected void ClearBuffer(CommandBuffer cmd, ComputeBuffer buffer, int sizeInByte = -1, int offset = 0)
			=> MixtureUtils.ClearBuffer(cmd, buffer, sizeInByte, offset);

		protected override void Disable()
		{
			base.Disable();

        	CoreUtils.Destroy(tempRenderTexture);
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
		}
#endif
    }
}