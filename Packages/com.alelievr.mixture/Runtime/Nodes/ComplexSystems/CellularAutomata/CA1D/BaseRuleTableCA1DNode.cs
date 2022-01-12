using System;
using GraphProcessor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Mixture.ComplexSystems.CA1D
{
    [Serializable]
    public abstract class BaseRuleTableCA1DNode : MixtureNode
    {
        [Input("State Count")] public int stateCount;
        [Output("Rules")] public ComputeBuffer rules;

        public RuleEvaluationMethod evaluationMethod;

        public override string name => "Rule Table CA1D";
        public override bool showDefaultInspector => true;

        protected int RuleCount => evaluationMethod == RuleEvaluationMethod.Anisotropic
            ? (int)Mathf.Pow(stateCount, 2 * Constants.Radius + 1)
            : 3 * stateCount - 2;

    #region Mixtured

        protected override void Disable()
        {
            base.Disable();

            rules?.Release();
        }

        protected override bool ProcessNode(CommandBuffer cmd)
        {
            if (!base.ProcessNode(cmd) || !RulesNeedsUpdate()) return false;

            if (stateCount < 2 || stateCount > 36)
            {
                Debug.LogError($"[{GetType().Name}] States count must be in between 2 and 36.");
                return false;
            }

            UpdateRules();

            return true;
        }

    #endregion

        protected abstract bool RulesNeedsUpdate();
        protected abstract void UpdateRules();
    }
}