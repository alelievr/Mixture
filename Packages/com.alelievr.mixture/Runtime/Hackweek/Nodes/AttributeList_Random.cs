using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;
//using System;
using Net3dBool;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Attribute List Random")]
	public class AttributeList_Random : MixtureNode
    {
        [Output("Output")]
        public MixtureAttributeList attributes = new MixtureAttributeList();

		public override string	name => "Attribute List Random";

		public override bool    hasPreview => false;
		public override bool    showDefaultInspector => true;

        public int              attributeCount = 24;

        [Input("Seed")]
        public int seed = 0;


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
                Random.InitState(seed + i);
                Vector3 normal = new Vector3(Random.Range(-1.0f,1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));

                attributes.Add(new MixtureAttribute{
                    {"position", new Vector3(0, 0, 0)},
                    {"scale", new Vector3(1, 1, 1)},
                    {"normal", normal},
                });
            }
			return true;
		}
    }
}