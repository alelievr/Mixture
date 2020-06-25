// using System.Collections.Generic;
// using UnityEngine;
// using GraphProcessor;
// using System.Linq;
// using UnityEngine.Rendering;

// namespace Mixture
// {
// 	[System.Serializable, NodeMenuItem("List")]
// 	public class ListNode : MixtureNode, INeedLoopReset
// 	{
// 		[Input("Add")]
// 		public MixtureMesh addElement;


//         [Output("List")]
//         public List<MixtureMesh> list;

// 		public override string	name => "List";

// 		public override bool    hasPreview => false;
// 		public override bool    showDefaultInspector => true;

// 		protected override void Enable()
// 		{
// 		}

// 		protected override bool ProcessNode(CommandBuffer cmd)
// 		{
//             list.Add(addElement);
// 			return true;
// 		}

//         public void PrepareNewIteration()
//         {
//             if (list == null)
//                 list = new List<MixtureMesh>();

//             list.Clear();
//         }
//     }
// }