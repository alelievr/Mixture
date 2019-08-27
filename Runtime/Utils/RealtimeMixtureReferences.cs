using System.Collections.Generic;
using UnityEngine;

namespace Mixture
{
	public static class RealtimeMixtureReferences
	{
		// Because we can't use LoadAllAssetsAtPath inside a CustomEditor, we have no
		// way of knowing if a CustomRenderTexture is a mixture or not.
		// Because of that, we store a list of CRT that are mixtures here
		public static HashSet< CustomRenderTexture > realtimeMixtureCRTs = new HashSet< CustomRenderTexture >();
	}
}