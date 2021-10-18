using System;
using GraphProcessor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Mixture.ComplexSystems.CA1D
{
    [Documentation(@"
Provide a set of the basic initial states for CA 1D suitable for CA with the number of states <= 3.
")]
    [Serializable, NodeMenuItem("Complex Systems/Cellular Automata/1D/Initial Conditions CA1D")]
    public class InitialConditionsCA1DNode : ComputeShaderNode
    {
        public enum Mode
        {
            Random,
            Mode202,
            Mode212,
            Mode101,
            Mode121,
            Mode010,
            Mode020
        }

        public Mode mode;
        public int seed;
        [Output] public CustomRenderTexture output;

        public override string name => "Initial Conditions CA1D";
        public override bool showDefaultInspector => true;
        public override Texture previewTexture => output;

        protected override MixtureSettings defaultSettings => Get2DOnlyRTSettings(base.defaultSettings);
        protected override string computeShaderResourcePath => "Mixture/CA1D/InitialConditionsCA1D";

        protected override void Enable()
        {
            base.Enable();

            settings.filterMode = OutputFilterMode.Point;

            UpdateTempRenderTexture(ref output);
        }

        protected override bool ProcessNode(CommandBuffer cmd)
        {
            if (!base.canProcess) return false;

            var size = new Vector3(settings.GetResolvedWidth(graph), settings.GetResolvedHeight(graph),
                settings.GetResolvedDepth(graph));

            UpdateTempRenderTexture(ref output);

            cmd.SetComputeVectorParam(computeShader, "_Size", size);
            cmd.SetComputeIntParam(computeShader, "_InitMode", (int) mode);
            cmd.SetComputeIntParam(computeShader, "_Seed", seed);
            cmd.SetComputeTextureParam(computeShader, 0, "_Output", output);

            DispatchCompute(cmd, 0, output.width);

            return true;
        }

        protected override void Disable()
        {
            base.Disable();

            CoreUtils.Destroy(output);
        }
    }
}