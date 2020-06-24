using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using UnityEngine.Rendering;

namespace Mixture
{
    public class MixtureAttribute : Dictionary<string, object>
    {
    }

    public class MixtureAttributeList : List<MixtureAttribute>
    {

    }

	[System.Serializable, NodeMenuItem("Attribute List")]
	public class AttributeList : MixtureNode
	{
        [Output("Output")]
        public MixtureAttributeList attributes = new MixtureAttributeList();

		public override string	name => "Attribute List";

		public override bool    hasPreview => false;
		public override bool    showDefaultInspector => true;

        public int              attributeCount = 24;

		protected override void Enable()
		{
		}

		// Functions with Attributes must be either protected or public otherwise they can't be accessed by the reflection code
		// [CustomPortBehavior(nameof(inputMeshes))]
		// public IEnumerable< PortData > ListMaterialProperties(List< SerializableEdge > edges)
		// {
        //     yield return new PortData
        //     {
        //         identifier = nameof(inputMeshes),
        //         displayName = "Input Meshes",
        //         allowMultiple = true,
        //         displayType()
        //     };
		// }

		// [CustomPortInput(nameof(inputMeshes), typeof(MixtureMesh))]
		// protected void GetMaterialInputs(List< SerializableEdge > edges)
		// {
        //     if (inputMeshes == null)
        //         inputMeshes = new List<MixtureMesh>();
        //     inputMeshes.Clear();
		// 	foreach (var edge in edges)
        //     {
        //         if (edge.passThroughBuffer is MixtureMesh m)
        //             inputMeshes.Add(m);
        //     }
		// }

		protected override bool ProcessNode(CommandBuffer cmd)
		{
            attributes.Clear();

            for (int i = 0; i < attributeCount; i++)
            {
                attributes.Add(new MixtureAttribute{
                    {"position", new Vector3((float)i, 0, 0)},
                    {"scale", new Vector3(0.5f, 0.5f, 2)},
                    {"normal", new Vector3(1.0f, 0.0f, 0.0f)},
                });
            }
			return true;
		}
    }
}