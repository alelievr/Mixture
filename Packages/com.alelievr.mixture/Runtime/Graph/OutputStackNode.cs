using UnityEngine;
using GraphProcessor;

namespace Mixture
{
    [System.Serializable]
    public class OutputStackNode : BaseStackNode
    {
        public OutputStackNode(Vector2 position) : base(position, "Output", false, false)
        {

        }
    }
}