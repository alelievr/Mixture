using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using System;
using Object = UnityEngine.Object;
using Unity.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Mixture
{
	[System.Serializable]
	public class MixtureProcessor : MixtureGraph
	{
		public ProcessorInputNode	inputNode;
        // public bool     showAsContext; // TODO

		void Enabled()
		{
			// We should have only one OutputNode per graph
			outputNode = nodes.FirstOrDefault(n => n is ProcessorOutputNode) as OutputNode;
			inputNode = nodes.FirstOrDefault(n => n is ProcessorInputNode) as ProcessorInputNode;

			if (outputNode == null)
				outputNode = AddNode(BaseNode.CreateFromType< OutputNode >(Vector2.zero)) as OutputNode;

#if UNITY_EDITOR
			if (isRealtime)
				RealtimeMixtureReferences.realtimeMixtureCRTs.Add(outputTexture as CustomRenderTexture);
#endif
		}
    }
}