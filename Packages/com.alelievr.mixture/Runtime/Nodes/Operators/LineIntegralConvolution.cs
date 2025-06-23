using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using GraphProcessor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Mixture
{
    [System.Serializable][NodeMenuItem("Operator/Line Integral Convolution")]
    public class LineIntegralConvolution : ComputeShaderNode
    {
        protected override string computeShaderResourcePath => "Mixture/LineIntegralConvolution";
        
        public override string name => "Line Integral Convolution";
        public override Texture previewTexture => LIC;
        [Input("TFM")] public Texture tfm;
        [Input("Noise")] public Texture noise;
        [Input("Gaussian Kernel")][SerializeField] private ComputeBuffer gaussianKernel;
        [Input("Kernel Radius")][SerializeField] private int kernelRadius;
        [Output] public CustomRenderTexture LIC;

        private int lic;
        protected override void Enable()
        {
            base.Enable();
            lic = computeShader.FindKernel("LIC");
            UpdateTempRenderTexture(ref LIC);
        }


        protected override bool ProcessNode(CommandBuffer cmd)
        {
            if (!base.ProcessNode(cmd) || tfm == null || noise == null || gaussianKernel == null)
                return false;
            cmd.SetComputeBufferParam(computeShader, lic, "__Kernel", gaussianKernel);
            cmd.SetComputeTextureParam(computeShader, lic, "__TFM", tfm);
            cmd.SetComputeTextureParam(computeShader, lic, "__Noise", noise);
            cmd.SetComputeTextureParam(computeShader, lic, "_LIC", LIC);
            cmd.SetComputeIntParam(computeShader, "__KernelRadius", kernelRadius);
            
            DispatchCompute(cmd, computeShader, lic, tfm.width, tfm.height, 1);
            return true;
        }
    }
    
}