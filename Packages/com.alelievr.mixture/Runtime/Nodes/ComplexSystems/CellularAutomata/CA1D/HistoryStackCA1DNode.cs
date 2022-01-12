using System;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Mixture.ComplexSystems.CA1D
{
    [Documentation(@"
Generates a time evolution history of CA 1D based on the initial state, where each row represents one evolution step. 
The state of 0 represents with black color (0), the (max state - 1) - white (1), time goes from top to bottom.

The time mode defines the number of evolution steps:
- Fixed: the number of steps is fixed and equal to the texture height (height - 1 to be more precise)
- Periodic: the number of steps formally is not restricted (limited only by int overflow), and CA grows by shifting the whole history and write a new generation to the last row.
")]
    [Serializable, NodeMenuItem("Complex Systems/Cellular Automata/1D/History Stack CA1D")]
    public class HistoryStackCA1DNode : ComputeShaderNode, IRealtimeReset
    {
        public enum TimeMode
        {
            Fixed,
            Periodic
        }

        private static readonly int SizeId = Shader.PropertyToID("_Size");
        private static readonly int StatesCountId = Shader.PropertyToID("_StatesCount");
        private static readonly int RuleEvaluationMethodId = Shader.PropertyToID("_Method");
        private static readonly int RowIndexId = Shader.PropertyToID("_RowIndex");
        private static readonly int RulesId = Shader.PropertyToID("_Rules");
        private static readonly int InitId = Shader.PropertyToID("_Init");
        private static readonly int OutputId = Shader.PropertyToID("_Output");

        [Input("Initial State")] public Texture input;
        [Input("Rules")] public ComputeBuffer rules;
        [Input("State Count")] public int stateCount;
        [Output] public CustomRenderTexture output;
        public TimeMode timeMode;
        public RuleEvaluationMethod evaluationMethod;

        public override string name => "History Stack CA1D";

        public override List<OutputDimension> supportedDimensions => new List<OutputDimension>()
        {
            OutputDimension.Texture2D,
        };

        public override bool showDefaultInspector => true;
        public override Texture previewTexture => output;

        protected override MixtureSettings defaultSettings => Get2DOnlyRTSettings(base.defaultSettings);
        protected override string computeShaderResourcePath => "Mixture/CA1D/HistoryStackCA1D";

        private Vector3 _size;
        private int _step;

        private int _writeInitialStateKernel;
        private int _stepKernel;
        private int _shiftOverTimeKernel;

    #region Mixtured

        public void RealtimeReset()
        {
            Reset();
        }

        protected override void Enable()
        {
            base.Enable();

            settings.filterMode = OutputFilterMode.Point;

            CoreUtils.Destroy(output);

            _writeInitialStateKernel = computeShader.FindKernel("WriteInitialState");
            _stepKernel = computeShader.FindKernel("Step");
            _shiftOverTimeKernel = computeShader.FindKernel("ShiftInTime");

            Reset();
        }

        protected override void Disable()
        {
            base.Disable();

            CoreUtils.Destroy(output);
        }

        protected override bool ProcessNode(CommandBuffer cmd)
        {
            if (!base.ProcessNode(cmd) || !input || rules == null || stateCount < 2) return false;

            UpdateTempRenderTexture(ref output);

            _size = new Vector3(settings.GetResolvedWidth(graph), settings.GetResolvedHeight(graph),
                settings.GetResolvedDepth(graph));

            if (timeMode == TimeMode.Fixed)
            {
                Initialize(cmd);
                StepOverAvailableTime(cmd);
            }
            else if (timeMode == TimeMode.Periodic)
            {
                if (_step == 1)
                {
                    Initialize(cmd);
                    StepOverAvailableTime(cmd);

                    _step = (int)_size.y - 1;

                    return true;
                }

                ShiftInTime(cmd);
                Step(cmd, 0);
            }

            return true;
        }

    #endregion

        protected virtual void Reset()
        {
            UpdateTempRenderTexture(ref output);

            _step = 1;
        }

        private void StepOverAvailableTime(CommandBuffer cmd)
        {
            for (var i = (int)_size.y - 2; i >= 0; i--)
            {
                Step(cmd, i);
            }
        }

        private void Initialize(CommandBuffer cmd)
        {
            cmd.SetComputeVectorParam(computeShader, SizeId, _size);
            cmd.SetComputeTextureParam(computeShader, _writeInitialStateKernel, InitId, input);
            cmd.SetComputeTextureParam(computeShader, _writeInitialStateKernel, OutputId, output);

            DispatchCompute(cmd, _writeInitialStateKernel, output.width);
        }

        private void Step(CommandBuffer cmd, int rowIndex)
        {
            cmd.SetComputeIntParam(computeShader, StatesCountId, stateCount);
            cmd.SetComputeIntParam(computeShader, RuleEvaluationMethodId, (int)evaluationMethod);
            cmd.SetComputeIntParam(computeShader, RowIndexId, rowIndex);
            cmd.SetComputeVectorParam(computeShader, SizeId, _size);
            cmd.SetComputeBufferParam(computeShader, _stepKernel, RulesId, rules);
            cmd.SetComputeTextureParam(computeShader, _stepKernel, InitId, output);
            cmd.SetComputeTextureParam(computeShader, _stepKernel, OutputId, output);

            DispatchCompute(cmd, _stepKernel, output.width);
        }

        private void ShiftInTime(CommandBuffer cmd)
        {
            cmd.SetComputeTextureParam(computeShader, _shiftOverTimeKernel, OutputId, output);

            DispatchCompute(cmd, _shiftOverTimeKernel, output.width, output.height);
        }
    }
}