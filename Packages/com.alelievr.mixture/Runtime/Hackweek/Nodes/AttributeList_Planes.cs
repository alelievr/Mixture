using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;
//using System;
using Net3dBool;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Attribute List Planes")]
	public class AttributeList_Planes : MixtureNode
    {
        [Output("Output")]
        public MixtureAttributeList attributes = new MixtureAttributeList();

		public override string	name => "Attribute List Planes";

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
                Random.InitState(seed);
                Vector3 normal = new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1));
                normal.Normalize();

                attributes.Add(new MixtureAttribute{
                    {"position", new Vector3((float)i, 0, 0)},
                    {"normal", normal},
                });
            }
			return true;
		}
    }
}