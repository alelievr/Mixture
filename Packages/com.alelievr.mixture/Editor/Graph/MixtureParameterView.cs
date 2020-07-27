using System;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;

namespace Mixture
{
    public class MixtureParameterView : ExposedParameterView
    {
		new const string title = "Parameters";

		protected override IEnumerable< Type > GetExposedParameterTypes()
        {
			// We only accept these types:
			yield return typeof(bool);
			yield return typeof(string);
			yield return typeof(Texture);
			yield return typeof(Texture2D);
			yield return typeof(Texture3D);
			yield return typeof(Cubemap);
			yield return typeof(RenderTexture);
			yield return typeof(CustomRenderTexture);
			yield return typeof(float);
			yield return typeof(Vector2);
			yield return typeof(Vector3);
			yield return typeof(Vector4);
			yield return typeof(Mesh);
			yield return typeof(ComputeBuffer);
            // // filter the slot types because we don't want generic types (i.e lists)
            // foreach (var type in base.GetExposedParameterTypes())
            // {
            //     yield return type;
            // }
        }
    }
}
