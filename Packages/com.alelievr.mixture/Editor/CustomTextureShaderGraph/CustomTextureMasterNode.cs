#if MIXTURE_SHADERGRAPH
using System;
using System.Linq;
using UnityEditor.Graphing;
using UnityEditor.ShaderGraph.Drawing.Controls;
using UnityEngine;
using UnityEditor.ShaderGraph;

namespace Mixture
{
    [Serializable]
    [Title("Master", "Custom Texture")]
    class CustomTextureMasterNode : MasterNode<ICustomTextureSubShader>
    {
        public const string ColorSlotName = "Color";

        public const int ColorSlotId = 0;

        public CustomTextureMasterNode()
        {
            UpdateNodeAfterDeserialization();
        }

        /*
        public override string documentationURL
        {
            get { return "https://github.com/Unity-Technologies/ShaderGraph/wiki/CustomTexture-Master-Node"; }
        }
        */

        public sealed override void UpdateNodeAfterDeserialization()
        {
            base.UpdateNodeAfterDeserialization();
            name = "Custom Texture Master";
            AddSlot(new ColorRGBAMaterialSlot(ColorSlotId, ColorSlotName, ColorSlotName, SlotType.Input, Color.grey, ShaderStageCapability.Fragment));

            // clear out slot names that do not match the slots
            // we support
            RemoveSlotsNameNotMatching( new[]{ ColorSlotId } );
        }
    }
}
#endif