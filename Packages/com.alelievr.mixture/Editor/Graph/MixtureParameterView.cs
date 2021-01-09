using System;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Mixture
{
    public class MixtureParameterView : ExposedParameterView
    {
		new const string title = "Parameters";
		
        public MixtureParameterView()
        {
            var style = Resources.Load<StyleSheet>("MixtureParameterView");
            if (style != null)
                styleSheets.Add(style);
        }

		protected override IEnumerable< Type > GetExposedParameterTypes()
        {
			// We only accept these types:
			yield return typeof(bool);
			yield return typeof(string);
			yield return typeof(Color);
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
        }

		protected override void UpdateParameterList()
        {
            content.Clear();

            foreach (var param in graphView.graph.exposedParameters)
            {
                var row = new BlackboardRow(new ExposedParameterFieldView(graphView, param), new MixtureExposedParameterPropertyView(graphView, param));
                row.expanded = param.settings.expanded;
                row.RegisterCallback<GeometryChangedEvent>(e => {
                    param.settings.expanded = row.expanded;
                });

                content.Add(row);
            }
        }
    }
}
