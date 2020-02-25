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
    [Title("Master", "Mip Map")]
    class MipMapMasterNode : MasterNode<IMipMapSubShader>
    {
        public const string ColorSlotName = "Color";

        public const int ColorSlotId = 0;

        public MipMapMasterNode()
        {
            UpdateNodeAfterDeserialization();
        }

        /*
        public override string documentationURL
        {
            get { return "https://github.com/Unity-Technologies/ShaderGraph/wiki/MipMap-Master-Node"; }
        }
        */

        public sealed override void UpdateNodeAfterDeserialization()
        {
            base.UpdateNodeAfterDeserialization();
            name = "Custom Mip Map";
            AddSlot(new ColorRGBAMaterialSlot(ColorSlotId, ColorSlotName, ColorSlotName, SlotType.Input, Color.grey, ShaderStageCapability.Fragment));

            // clear out slot names that do not match the slots
            // we support
            RemoveSlotsNameNotMatching( new[]{ ColorSlotId } );
        }
    }
}
#endif