#if MIXTURE_EXPERIMENTAL
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;
using System;

using Random = System.Random;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Get Element From List")]
	public class RandomElementFromList : MixtureNode
	{
        [Input("List")]
        public List<MixtureMesh> list;

		[Input("Index")]
		public int index;

		[Output("Element")]
		public MixtureMesh elem;

		public override string	name => "Get Element From List";

		public override bool    hasPreview => true;
		public override bool    showDefaultInspector => true;
		public override Texture previewTexture
		{
			get
			{
				if (list == null || list.Count == 0)
					return Texture2D.blackTexture;
				
				int i = (int)Time.time;
				var e = list[i % (list.Count - 1)];
				if (e.mesh == null)
					return Texture2D.blackTexture;

#if UNITY_EDITOR
				return UnityEditor.AssetPreview.GetAssetPreview(e.mesh) ?? Texture2D.blackTexture;
#else
				return Texture2D.blackTexture;
#endif
			}
		}

		// Random random;

		// protected override void Enable()
		// {
		// 	random = new Random(4242);
		// }

		protected override bool ProcessNode(CommandBuffer cmd)
		{
            if (list == null || list.Count == 0)
                return false;

			index = Mathf.Clamp(index, 0, list.Count - 1);
            elem = list[index];

			return true;
		}
    }
}
#endif