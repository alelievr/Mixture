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
	[System.Serializable, NodeMenuItem("Attribute From Map")]
	public class AttributeFromMap : MixtureNode
	{
        [Input("Input")]
        public MixtureAttributeList inputPoints;
        
		[Input("Map")]
		public Texture input;

        [Output("Output")]
        public MixtureAttributeList outputPoints;

        public enum MapAttribute
        {
            Position,
            Normal,
            Scale,
        }

		public override string	name => "Attribute From Map";

		public override bool hasPreview => true;
		public override bool showDefaultInspector => true;
        public override Texture previewTexture => cache ?? Texture2D.blackTexture;

        public MapAttribute attributeType = MapAttribute.Normal;

        Texture2D cache;

		protected override bool ProcessNode(CommandBuffer cmd)
		{
            if (input == null || inputPoints == null)
                return false;

            if (input.dimension != TextureDimension.Tex2D)
                return false;
            
            Color32[] colors = null;
            cache = new Texture2D(rtSettings.GetWidth(graph), rtSettings.GetHeight(graph), rtSettings.GetGraphicsFormat(graph), TextureCreationFlags.None);
            cache.filterMode = FilterMode.Point;

            if (input is RenderTexture rt)
            {
                cmd.RequestAsyncReadback(rt, (r) => {
                    colors = r.GetData<Color32>().ToArray();
                });
                cmd.WaitAllAsyncReadbackRequests();
                MixtureGraphProcessor.AddGPUAndCPUBarrier();
            }
            else
            {
                colors = (input as Texture2D).GetPixels32();
            }

            var r = new System.Random(42);

            outputPoints = new MixtureAttributeList();

            Color32[] previewColor = new Color32[cache.width * cache.height];
            foreach (var attr in inputPoints)
            {
                // Sample the input texture with points:
                if (attr.TryGetValue("uv", out var ouv) && ouv is Vector2 uv)
                {
                    int index = (int)(uv.x * input.width + uv.y * input.height * input.width);
                    Vector4 color = new Vector4(colors[index].r / 255f, colors[index].g / 255f, colors[index].b / 255f, colors[index].a / 255f);

                    AddAttribute(attr, uv, color, ref previewColor);
                }
                outputPoints.Add(attr);
            }

            cache.SetPixels32(previewColor);
            cache.Apply();

			return true;
		}

        void AddAttribute(MixtureAttribute attr, Vector2 uv, Vector4 data, ref Color32[] previewColor)
        {
            // Preview color
            int previewIndex = (int)(uv.x * cache.width + uv.y * cache.height * cache.width);
            previewColor[previewIndex] = new Color(data.x, data.y, data.z);

            string attribName = null;
            switch (attributeType)
            {
                case MapAttribute.Normal: attribName = "normal"; break;
                case MapAttribute.Position: attribName = "position"; break;
                case MapAttribute.Scale: attribName = "scale"; break;
            }
            attr[attribName] = (Vector3)data;
        }
    }
}