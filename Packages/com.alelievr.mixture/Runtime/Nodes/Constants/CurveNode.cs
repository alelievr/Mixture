using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
    [Documentation(@"
Generate a texture based on curves, you can choose to have a different curve per channel with the Mode property.
This node is a great alternative to the gradient because it's not limited to 8 keys!

Note that the internal texture resolution is 512x1 pixels and the format is 32 bit float per channel.
")]

    [System.Serializable, NodeMenuItem("Constants/Curve")]
    public class CurveNode : MixtureNode
    {
        public enum CurveOutputMode
        {
            RRRR,
            R,
            RG,
            RGB,
            RGBA,
        }

        [Output(name = "Curve")]
        public Texture2D texture;

        public CurveOutputMode mode = CurveOutputMode.RRRR;

        public AnimationCurve redCurve = new AnimationCurve(new Keyframe(0,0), new Keyframe(1,1));
        public AnimationCurve greenCurve = new AnimationCurve(new Keyframe(0,0), new Keyframe(1,1));
        public AnimationCurve blueCurve = new AnimationCurve(new Keyframe(0,0), new Keyframe(1,1));
        public AnimationCurve alphaCurve = new AnimationCurve(new Keyframe(0,0), new Keyframe(1,1));

        public override bool hasSettings => false;
        public override string name => "Curve";

        const int CurveTextureResolution = 512;

        protected override void Enable()
        {
            base.Enable();
            UpdateTexture();
        }

        Color[] pixels = new Color[CurveTextureResolution];

        Color GetPixelColor(float t)
        {
            float r = redCurve.Evaluate(t);
            float g = greenCurve.Evaluate(t);
            float b = blueCurve.Evaluate(t);
            float a = alphaCurve.Evaluate(t);

            switch (mode)
            {
                default:
                case CurveOutputMode.RRRR:
                    return new Color(r, r, r, r);
                case CurveOutputMode.R:
                    return new Color(r, 1, 1, 1);
                case CurveOutputMode.RG:
                    return new Color(r, g, 1, 1);
                case CurveOutputMode.RGB:
                    return new Color(r, g, b, 1);
                case CurveOutputMode.RGBA:
                    return new Color(r, g, b, a);
            }
        }

        public void UpdateTexture()
        {
            if (texture == null)
            {
                texture = new Texture2D(CurveTextureResolution, 1, TextureFormat.RGBAFloat, false, true);
                texture.wrapMode = TextureWrapMode.Clamp;
                texture.filterMode = FilterMode.Bilinear;
                texture.hideFlags = HideFlags.HideAndDontSave;
            }

            for (int i = 0; i<CurveTextureResolution; i++)
            {
                float t = (float)i / (CurveTextureResolution - 1);
                pixels[i] = GetPixelColor(t);
            }
            texture.SetPixels(pixels);
            texture.Apply(false);
        }
    }
}