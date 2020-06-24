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
	[System.Serializable, NodeMenuItem("Points From Map")]
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

            if (input is RenderTexture rt)
            {
                cache = new Texture2D(input.width, input.height, rtSettings.GetGraphicsFormat(graph), TextureCreationFlags.None);
                cache.filterMode = FilterMode.Point;
                // RenderTexture.active = rt;
                // // Read pixels
                // cache.ReadPixels(new Rect(0, 0, input.width, input.height), 0, 0);
                // cache.Apply();
                // RenderTexture.active = null; // added to avoid errors 
                cmd.RequestAsyncReadback(rt, (r) => {
                    var array = r.GetData<Color32>();
                    cache.SetPixels32(array.ToArray());
                    cache.Apply();
                });
                cmd.WaitAllAsyncReadbackRequests();
                MixtureGraphProcessor.AddGPUAndCPUBarrier();
            }
            else
                cache = input as Texture2D;

            var r = new System.Random(42);

            Color32[] pixels = cache.GetPixels32();
            points = new MixtureAttributeList();

            for (int i = 0; i < pixels.Length; i++)
            {
                float f = (float)r.NextDouble();
                var p = pixels[i];

                float a = (float)p.a / 255.0f;
                if (f * a > density)
                {
                    pixels[i] = Color.white;
                    points.Add(new MixtureAttribute{
                        {"position", new Vector3(i % input.width, Mathf.Floor(i / input.width), 0)},
                        {"normal", Vector3.up},
                        {"density", p.a}
                    });
                }
                else
                    pixels[i] = Color.black;
            }

            cache.SetPixels32(pixels);
            cache.Apply();

			return true;
		}
    }
}