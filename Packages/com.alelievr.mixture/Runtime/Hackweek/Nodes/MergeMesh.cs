// using System.Collections.Generic;
// using UnityEngine;
// using GraphProcessor;
// using System.Linq;
// using UnityEngine.Rendering;
// using System;
// namespace Mixture
// {
// 	[System.Serializable, NodeMenuItem("Merge Mesh")]
// 	public class MergeMesh : MixtureNode
// 	{
// 		[Output("Mesh")]
// 		public MixtureMesh output;
// 		public override string	name => "Merge Mesh";
// 		public override bool hasPreview => false;
// 		public override bool showDefaultInspector => true;
//         public Mesh mesh;
// 		public Vector3 scale = Vector3.one;
// 		// There is an issue with json serialization and new keyword :) 
// 		public Vector3 bug_position = Vector3.zero;
// 		// TODO: rotation
// 		protected override bool ProcessNode(CommandBuffer cmd)
// 		{
// 			if (mesh == null)
// 				return false;
//             output = new MixtureMesh{ mesh = mesh, localToWorld = Matrix4x4.TRS(bug_position, Quaternion.identity, scale) };
// 			return true;
// 		}
//     }
// }