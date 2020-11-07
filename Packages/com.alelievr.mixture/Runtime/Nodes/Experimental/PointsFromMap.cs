#if MIXTURE_EXPERIMENTAL
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;
using System;
using UnityEngine.Experimental.Rendering;
using Unity.Collections;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Experimental/Points From Map")]
	public class PointsFromMap : MixtureNode
	{
		[Input("Map")]
		public Texture input;

        [Output("Points")]
        public MixtureAttributeList points;

        public float density = 0.5f;

		public override string	name => "Points From Map";

		public override bool hasPreview => true;
		public override bool showDefaultInspector => true;
        public override Texture previewTexture => cache ?? Texture2D.blackTexture;

        Texture2D cache;

		protected override bool ProcessNode(CommandBuffer cmd)
		{
            if (input == null)
                return false;

            if (input.dimension != TextureDimension.Tex2D)
                return false;

            Color32[] colors = null;
            cache = new Texture2D(input.width, input.height, rtSettings.GetGraphicsFormat(graph), TextureCreationFlags.None);
            cache.filterMode = FilterMode.Point;
            if (input is RenderTexture rt)
            {
                cmd.RequestAsyncReadback(rt, (r) => {
                    colors = r.GetData<Color32>().ToArray();
                });
                cmd.WaitAllAsyncReadbackRequests();
                MixtureGraphProcessor.AddGPUAndCPUBarrier(cmd);
            }
            else
            {
                colors = (input as Texture2D).GetPixels32();
            }

            var r = new System.Random(42);

            points = new MixtureAttributeList();

            for (int i = 0; i < colors.Length; i++)
            {
                float f = (float)r.NextDouble();
                var p = colors[i];

                float a = (float)p.a / 255.0f;
                if (f * a > density)
                {
                    colors[i] = Color.white;
                    Vector3 position = new Vector3((float)(i % input.width), 0, (Mathf.Floor(i / (float)input.width)));
                    Vector2 uv = new Vector2(position.x / (float)input.width, position.z / (float)input.height);
                    points.Add(new MixtureAttribute{
                        {"uv", uv},
                        {"position", position},
                        {"density", p.a}
                    });
                }
                else
                    colors[i] = Color.black;
            }

            cache.SetPixels32(colors);
            cache.Apply();

			return true;
		}
    }
}
#endif