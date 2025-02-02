using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using GraphProcessor;
using UnityEngine.Rendering;

namespace Mixture
{
    [System.Serializable, NodeMenuItem("Utility/Gaussian Kernel")]
    public class GaussianKernelNode : MixtureNode
    {
        [Input] [SerializeField] private int kernelRadius;

        [Input] [SerializeField] private float kernelSigma;

        [Output] public ComputeBuffer kernelBuffer;
        [Output("Kernel Radius")] public int kernelRadiusOutput;

        public override string name => "Gaussian Kernel";

        public override bool showDefaultInspector => true; // Make the serializable field visible in the node UI.

        private int currentRadius;
        private float currentSigma;

        protected override bool ProcessNode(CommandBuffer cmd)
        {
            if (!base.ProcessNode(cmd))
                return false;
            
            UpdateKernel(kernelRadius, kernelSigma);
            kernelRadiusOutput = kernelRadius;
            return true;
        }

        private void UpdateKernel(int newRadius, float newSigma)
        {
            if (kernelBuffer == null || 
                !kernelBuffer.IsValid() || 
                !Mathf.Approximately(newRadius, currentRadius) || 
                !Mathf.Approximately(newSigma, currentSigma))
            {
                kernelBuffer?.Dispose();
                this.currentRadius = newRadius;
                this.currentSigma = newSigma;
                kernelBuffer = CreateKernel(newRadius, newSigma);
            }
        }

        private static ComputeBuffer CreateKernel(int gaussianRadius, float gaussianSigma)
        {
            var kernel = OneDimensinalKernel(gaussianRadius, gaussianSigma);// GenerateGaussianKernel(gaussianRadius, gaussianSigma);
            var buffer = new ComputeBuffer(kernel.Length, sizeof(float), ComputeBufferType.Default);
            buffer.SetData(kernel);
            return buffer;
        }
        
        private static float[] OneDimensinalKernel(int radius, float sigma)
        {
            float[] kernelResult = new float[radius * 2 + 1];
            float sum = 0.0f;
            for(int t = 0; t< radius; t++)
            {
                double newBlurWalue = 0.39894 * Mathf.Exp(-0.5f * t*t / (sigma * sigma)) / sigma;
                kernelResult[radius+t] = (float)newBlurWalue;
                kernelResult[radius-t] = (float)newBlurWalue;
                if(t!=0)
                    sum += (float)newBlurWalue*2.0f;
                else
                    sum += (float)newBlurWalue;
            }
            // normalize kernels
            for(int k = 0; k< radius*2 +1; k++)
            {
                kernelResult[k]/=sum;
            }
            return kernelResult;
        }

        protected override void Disable()
        {
            base.Disable();
            kernelBuffer?.Dispose();
        }
    }
}