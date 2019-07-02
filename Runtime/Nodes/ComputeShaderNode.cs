using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using GraphProcessor;
using System.Linq;
using UnityEditor;
using System;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Compute Shader")]
	public class ComputeShaderNode : MixtureNode
	{
		[Input(name = "In"), SerializeField]
		public List< object >	materialInputs;

		[Output(name = "Out"), SerializeField]
		public RenderTexture	output = null;

		public ComputeShader	computeShader;
		public override string	name => "Shader";

        public string           kernelName = "CSMain";

		public static string	DefaultShaderName = "ComputeShaderNodeDefault";

		protected override void Enable()
		{
			if (computeShader == null)
			{
				computeShader = Resources.Load<ComputeShader>(DefaultShaderName);
			}
		}

		[CustomPortBehavior(nameof(materialInputs))]
		IEnumerable< PortData > ListMaterialProperties(List< SerializableEdge > edges)
		{
            // TODO: a list of properties to bind to the compute shader
            yield break;
		}

		[CustomPortInput(nameof(materialInputs), typeof(object))]
		public void GetMaterialInputs(List< SerializableEdge > edges)
		{
            // TODO: assign the compute shader inputs values
		}

		[CustomPortBehavior(nameof(output))]
		IEnumerable< PortData > ChangeOutputPortType(List< SerializableEdge > edges)
		{
			yield return new PortData{
				displayName = "output",
				displayType = TextureUtils.GetTypeFromDimension(graph.outputNode.dimension),
				identifier = "outout",
			};
		}

		protected override void Process()
		{
			UpdateTempRenderTexture(ref output);

			if (computeShader == null)
			{
				Debug.LogError($"Can't process {name}, missing material/shader");
				return ;
			}

            int kernelId = -1;
            try {
                kernelId = computeShader.FindKernel(kernelName);
            } catch { return; } // We don't process if we can't find the kernel

            uint dispatchX, dispatchY, dispatchZ;
            computeShader.GetKernelThreadGroupSizes(kernelId, out dispatchX, out dispatchY, out dispatchZ);

			switch (output.dimension)
			{
				case TextureDimension.Tex2D:
				case TextureDimension.Tex2DArray:
				case TextureDimension.Tex3D:
                    computeShader.Dispatch(kernelId, output.width / (int)dispatchX, output.height / (int)dispatchY, output.volumeDepth / (int)dispatchZ);
					break ;
				default:
					Debug.LogError("Compute Shader Node output not supported");
					break;
			}
		}
	}
}