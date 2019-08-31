using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
    [System.Serializable, NodeMenuItem("Constants/Curve")]
    public class CurveNode : MixtureNode
    {
        [Output(name = "Curve")]
        public Texture2D texture;

        public AnimationCurve curve = new AnimationCurve(new Keyframe(0,0), new Keyframe(1,1));

        public override bool hasSettings => false;
        public override string name => "Curve";

        const int SIZE = 256;

        protected override void Enable()
        {
            base.Enable();
            UpdateTexture();
        }

        Color[] pixels = new Color[SIZE];

        public void UpdateTexture()
        {
            if (texture == null)
            {
                texture = new Texture2D(SIZE, 1, TextureFormat.RFloat, false);
                texture.wrapMode = TextureWrapMode.Clamp;
                texture.filterMode = FilterMode.Bilinear;
            }

            for(int i = 0; i<SIZE; i++)
            {
                float t = (float)i / (SIZE - 1);
                float v = curve.Evaluate(t);
                pixels[i] = new Color(v,1,1,1);
            }
            texture.SetPixels(pixels);
            texture.Apply(false);
        }
    }
}