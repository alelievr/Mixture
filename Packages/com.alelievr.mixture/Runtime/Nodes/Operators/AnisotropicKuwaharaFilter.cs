using GraphProcessor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Mixture
{
    [System.Serializable][NodeMenuItem("Operators/Anisotropic Kuwahara Filter")]
    public class AnisotropicKuwaharaFilter : ComputeShaderNode
    {
        protected override string computeShaderResourcePath => "Mixture/AnisotropicKuwaharaFilter";

        public override string name => "Anisotropic Kuwahara Filter";

        public override Texture previewTexture => output;
        public override bool showDefaultInspector => true;
        [Input("Source")] public Texture source;
        [Input("TFM")] public Texture tfm;
        [Output] public CustomRenderTexture output;
        [ShowInInspector] public int passCount = 2;
        [ShowInInspector] public int kuwaharaRadius = 5;
        [ShowInInspector] public float kuwaharaAlpha = 1.0f;
        [ShowInInspector] public int kuwaharaQ = 1;
        [ShowInInspector] public float strokeScale = 0.5f;
        [ShowInInspector] public bool useZeta;
        [ShowInInspector] public float zeta;
        [ShowInInspector] public float sharpness = 8;
        [ShowInInspector] public float hardness = 8;
        [ShowInInspector] public float zeroCrossing = 0.58f;
        private RenderTexture inputCopy;

        private int kuwaharaComputeKernel;

        protected override void Enable()
        {
            base.Enable();
            UpdateTempRenderTexture(ref output);
            kuwaharaComputeKernel = computeShader.FindKernel("AnisotropicKuwahara");
        }


        private void ValidateTempRenderTexture()
        {
            if (inputCopy == null || inputCopy.width != output.width ||
                inputCopy.height != output.height)
            {
                inputCopy?.Release();
                inputCopy = new RenderTexture(output.width, output.height, 0, output.format);
                inputCopy.enableRandomWrite = true;
                inputCopy.Create();
            }
        }

        protected override void Disable()
        {
            base.Disable();
            inputCopy?.Release();
        }

        protected override bool ProcessNode(CommandBuffer cmd)
        {
            if (!base.ProcessNode(cmd) || tfm == null || source == null)
                return false;
            ValidateTempRenderTexture();
            for (int i = 0; i < passCount; i++)
            {
                // Copy previous output
                cmd.Blit(i == 0 ? source : output, inputCopy);

                cmd.SetComputeTextureParam(computeShader, kuwaharaComputeKernel, "_Source", inputCopy);
                cmd.SetComputeTextureParam(computeShader, kuwaharaComputeKernel, "_TFM", tfm);
                cmd.SetComputeTextureParam(computeShader, kuwaharaComputeKernel, "_Output", output);
                cmd.SetComputeIntParam(computeShader, "_KuwaharaRadius", kuwaharaRadius);
                cmd.SetComputeFloatParam(computeShader, "_KuwaharaAlpha", kuwaharaAlpha);
                cmd.SetComputeFloatParam(computeShader, "_KuwaharaQ", kuwaharaQ);
                cmd.SetComputeFloatParam(computeShader, "_StrokeScale", strokeScale);
                cmd.SetComputeFloatParam(computeShader, "_Hardness", hardness);
                cmd.SetComputeFloatParam(computeShader, "_ZeroCrossing", zeroCrossing);
                cmd.SetComputeFloatParam(computeShader, "_Zeta",
                    useZeta ? zeta : 2.0f / 2.0f / (kuwaharaRadius / 2.0f));
                DispatchCompute(cmd, kuwaharaComputeKernel, output.width, output.height, 1);
            }

            return true;
        }
    }
}