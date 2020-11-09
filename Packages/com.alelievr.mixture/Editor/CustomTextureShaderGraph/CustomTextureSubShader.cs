#if MIXTURE_SHADERGRAPH
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.ShaderGraph;
using UnityEditor.ShaderGraph.Legacy;

namespace Mixture
{
    sealed class CustomTextureSubTarget : SubTarget<FullScreenPassTarget>, ILegacyTarget
    {
        const string kAssetGuid = "5b2d4724a38a5485ba5e7dc2f7d86f1a";
        const string kTemplateGuid = "afa536a0de48246de92194c9e987b0b8";

        internal static FieldDescriptor colorField = new FieldDescriptor(String.Empty, "Color", string.Empty);

        public CustomTextureSubTarget()
        {
            isHidden = false;
            displayName = "Custom Render Texture";
        }

        public override bool IsActive() => true;

        public override void Setup(ref TargetSetupContext context)
        {
            context.AddAssetDependency(new GUID(kAssetGuid), AssetCollection.Flags.SourceDependency);
            context.AddAssetDependency(new GUID(kTemplateGuid), AssetCollection.Flags.SourceDependency);
            context.AddSubShader(SubShaders.SpriteUnlit);
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
            if(!(masterNode is CustomTextureMasterNode crtMasterNode))
                return false;

            blockMap = new Dictionary<BlockFieldDescriptor, int>()
            {
                { FullScreenBlocks.colorBlock, 0 },
            };

            return true;
        }

        static class SubShaders
        {
            public static SubShaderDescriptor SpriteUnlit = new SubShaderDescriptor()
            {
                generatesPreview = true,
                passes = new PassCollection
                {
                    { FullscreePasses.CustomRenderTexture },
                },
            };
        }
    }
}
#endif