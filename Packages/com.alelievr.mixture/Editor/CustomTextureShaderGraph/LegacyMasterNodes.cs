#if MIXTURE_SHADERGRAPH
using System;
using System.Linq;
using UnityEditor.Graphing;
using UnityEditor.ShaderGraph.Drawing.Controls;
using UnityEngine;
using UnityEditor.ShaderGraph;
using UnityEditor.ShaderGraph.Legacy;

namespace Mixture
{
    [Serializable]
    class MipMapMasterNode : AbstractMaterialNode, IMasterNode1
    {
        public const string ColorSlotName = "Color";

        public const int ColorSlotId = 0;
    }

    [Serializable]
    class CustomTextureMasterNode : AbstractMaterialNode, IMasterNode1
    {
        public const string ColorSlotName = "Color";

        public const int ColorSlotId = 0;
    }
}
#endif