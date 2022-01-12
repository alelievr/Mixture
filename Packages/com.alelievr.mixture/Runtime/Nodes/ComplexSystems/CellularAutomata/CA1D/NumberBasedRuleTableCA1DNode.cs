using System;
using GraphProcessor;
using UnityEngine;

namespace Mixture.ComplexSystems.CA1D
{
    [Documentation(@"
Generates a rule table for 1D cellular automata based on the number base system, where the number of states determines the base (or radix) of the number system. 
The radius of neighbor lookup is fixed and equals 1.
The evaluation method determines how the state of the cell on the next generation will be evaluated:
- Anisotropic: transition depends on the state of each cell and on the alignment to each other, i.e. (1, 1, 0) != (0, 1, 1)
- Totalistic: transition depends only on the average sum of the states of each cell, i.e. (1, 2, 0) == (2, 0, 1) and etc.

This approach allows to easily generate examples from Wolfram MathWorld and the 'New Kind Of Science' book.
    ")]
    [Serializable, NodeMenuItem("Complex Systems/Cellular Automata/1D/Number Based Rule Table CA1D")]
    public class NumberBasedRuleTableCA1DNode : BaseRuleTableCA1DNode
    {
        public long rule;

        public override string name => "Number Based Rule Table CA1D";
        public override bool showDefaultInspector => true;

        private long _rule;
        private int _stateCount;
        private RuleEvaluationMethod _evaluationMethod;

        protected override bool RulesNeedsUpdate() =>
            rule != _rule || stateCount != _stateCount || evaluationMethod != _evaluationMethod;

        protected override void UpdateRules()
        {
            rules?.Release();

            _rule = rule;
            _stateCount = stateCount;
            _evaluationMethod = evaluationMethod;
            var rulesData = GenerateRulesData(rule, stateCount, RuleCount);
            rules = new ComputeBuffer(RuleCount, sizeof(uint), ComputeBufferType.Structured);

            rules.SetData(rulesData);
        }

        private uint[] GenerateRulesData(long number, int radix, int size)
        {
            var result = new uint[size];

            if (number == 0) return result;

            var max = (long)Math.Pow(radix, size) - 1;
            if (max < number)
            {
                number = max;
                Debug.LogWarning($"[{GetType().Name}] Rule has truncated to states^size = {max}");
            }

            var closestPowerOfBase = (int)Math.Floor(Math.Log(number, radix));

            for (var i = 0; i <= closestPowerOfBase; i++)
            {
                var index = i;
                var remainder = number % radix;
                number /= radix;

                result[index] = (uint)remainder;
            }

            return result;
        }
    }
}