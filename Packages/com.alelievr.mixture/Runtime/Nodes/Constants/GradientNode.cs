using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;

namespace Mixture
{
    [System.Serializable, NodeMenuItem("Constants/Gradient")]
    public class GradientNode : MixtureNode
    {
        [Output(name = "Gradient")]
        public Texture2D texture;

        public Gradient gradient = new Gradient();

        public override bool hasSettings => false;
        public override string name => "Gradient";

        const int SIZE = 256;

        protected override void Enable()
        {
            base.Enable();
            UpdateTexture();
        }

        protected override bool ProcessNode(CommandBuffer cmd)
        {
            if (!base.ProcessNode(cmd))
                return false;

            // Sometimes the texture is destroyed by the C++ without any notification so we check for this
            if (texture == null)
                UpdateTexture();
            
            return true;
        }

        Color[] pixels = new Color[SIZE];

        public void UpdateTexture()
        {
            if (texture == null)
            {
                texture = new Texture2D(SIZE, 1, TextureFormat.RGBAFloat, false);
                texture.wrapMode = TextureWrapMode.Clamp;
                texture.filterMode = FilterMode.Bilinear;
            }

            for(int i = 0; i<SIZE; i++)
            {
                float t = (float)i / (SIZE - 1);
                pixels[i] = gradient.Evaluate(t);
                pixels[i] = pixels[i].linear;
            }
            texture.SetPixels(pixels);
            texture.Apply(false);
        }
    }
}