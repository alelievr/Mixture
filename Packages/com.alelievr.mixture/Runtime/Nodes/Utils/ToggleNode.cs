using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Toggle")]
    public class ToggleNode : MixtureNode
    {
		public override bool hasSettings => false;
        
        public override string name => "Toggle";

        public override float nodeWidth => 120;

        [Output("Output"), SerializeField]
        public bool state;
    }
}