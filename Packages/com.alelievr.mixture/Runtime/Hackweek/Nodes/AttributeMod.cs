using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;
using System;
namespace Mixture
{
	[System.Serializable, NodeMenuItem("Attribute Modifier")]
	public class AttributeMod : MixtureNode
	{
		[Input("Attr")]
		public MixtureAttributeList input;
		[Output("Attr")]
		public MixtureAttributeList output;

		public override string	name => "Attribute Modifier";
		public override bool hasPreview => false;
		public override bool showDefaultInspector => false;

        public string modifierSourceCode = "Debug.Log(\"Hello World\")";

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			if (input == null)
				return false;

            output = new MixtureAttributeList();
            foreach (var i in input)
            {
                // Apply mod to i:
                
                output.Add(i);
            }

			return true;
		}
    }
}