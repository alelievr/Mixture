#if MIXTURE_SHADERGRAPH
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.ShaderGraph;
using UnityEditor.ShaderGraph.Legacy;

namespace Mixture
{
    sealed class MipMapSubTarget : SubTarget<FullScreenPassTarget>, ILegacyTarget
    {
        const string kAssetGuid = "d7b64263837222248a1154da16e33396";
        const string kTemplateGuid = "e2bd3f2f69902b64c8cd77ba7948c1b9";

        internal static FieldDescriptor colorField = new FieldDescriptor(String.Empty, "Color", string.Empty);

        public MipMapSubTarget()
        {
            isHidden = false;
            displayName = "Custom Render Texture";
        }

        public override bool IsActive() => true;

        public override void Setup(ref TargetSetupContext context)
        {
            context.AddAssetDependencyPath(AssetDatabase.GUIDToAssetPath(kAssetGuid));
            context.AddAssetDependencyPath(AssetDatabase.GUIDToAssetPath(kTemplateGuid));
            context.AddSubShader(new SubShaderDescriptor()
            {
                generatesPreview = true,
                passes = new PassCollection
                {
                    { FullscreePasses.MipMap },
                },
            });
        }

        public override void GetFields(ref TargetFieldContext context)
        {
            context.AddField(colorField, true);
        }

        public override void GetActiveBlocks(ref TargetActiveBlockContext context)
        {
            context.AddBlock(FullScreenBlocks.colorBlock, true);
        }

        public override void GetPropertiesGUI(ref TargetPropertyGUIContext context, Action onChange, Action<String> registerUndo)
        {
        }

        public bool TryUpgradeFromMasterNode(IMasterNode1 masterNode, out Dictionary<BlockFieldDescriptor, int> blockMap)
        {
            blockMap = null;
            if(!(masterNode is MipMapMasterNode mipmapMaster))
                return false;

            blockMap = new Dictionary<BlockFieldDescriptor, int>()
            {
                { FullScreenBlocks.colorBlock, 0 },
            };

            return true;
        }
    }
}
#endif