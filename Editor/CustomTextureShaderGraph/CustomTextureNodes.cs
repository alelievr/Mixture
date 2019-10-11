#if MIXTURE_SHADERGRAPH
using UnityEngine;
using UnityEditor.ShaderGraph;
using UnityEditor.Graphing;

namespace Mixture
{
    [Title("Custom Texture", "Size")]
    class CustomTextureSize : AbstractMaterialNode, IGeneratesFunction
    {
		private const string kOutputSlotWidthName = "Texture Width";
		private const string kOutputSlotHeightName = "Texture Height";
		private const string kOutputSlotDepthName = "Texture Depth";
		
        public const int OutputSlotWidthId = 0;
        public const int OutputSlotHeightId = 1;
        public const int OutputSlotDepthId = 2;

        public CustomTextureSize()
        {
            name = "Custom Texture Size";
            UpdateNodeAfterDeserialization();
        }

        protected int[] validSlots => new[] { OutputSlotWidthId, OutputSlotHeightId, OutputSlotDepthId };

        public sealed override void UpdateNodeAfterDeserialization()
        {
            AddSlot(new Vector1MaterialSlot(OutputSlotWidthId, kOutputSlotWidthName, kOutputSlotWidthName, SlotType.Output, 0));
            AddSlot(new Vector1MaterialSlot(OutputSlotHeightId, kOutputSlotHeightName, kOutputSlotHeightName, SlotType.Output, 0));
            AddSlot(new Vector1MaterialSlot(OutputSlotDepthId, kOutputSlotDepthName, kOutputSlotDepthName, SlotType.Output, 0));
            RemoveSlotsNameNotMatching(validSlots);
        }

        public override string GetVariableNameForSlot(int slotId)
        {
            switch (slotId)
            {
                case OutputSlotHeightId:
                    return "_CustomRenderTextureHeight";
                case OutputSlotDepthId:
                    return "_CustomRenderTextureDepth";
                default:
                    return "_CustomRenderTextureWidth";
            }
        }

        public void GenerateNodeFunction(FunctionRegistry registry, GenerationMode generationMode)
        {
            // For preview only we declare CRT defines
            if (generationMode == GenerationMode.Preview)
            {
                registry.builder.AppendLine("#define _CustomRenderTextureHeight 0.0");
                registry.builder.AppendLine("#define _CustomRenderTextureWidth 0.0");
                registry.builder.AppendLine("#define _CustomRenderTextureDepth 0.0");
            }
        }
    }

    [Title("Custom Texture", "Slice / Face")]
    class CustomTextureSlice : AbstractMaterialNode, IGeneratesFunction
    {
		private const string kOutputSlotCubeFaceName = "Texture Cube Face";
		private const string kOutputSlot3DSliceName = "Texture 3D Slice";
		
        public const int OutputSlotCubeFaceId = 3;
        public const int OutputSlot3DSliceId = 4;

        public CustomTextureSlice()
        {
            name = "Custom Texture Size";
            UpdateNodeAfterDeserialization();
        }

        protected int[] validSlots => new[] { OutputSlotCubeFaceId, OutputSlot3DSliceId };

        public sealed override void UpdateNodeAfterDeserialization()
        {
            AddSlot(new Vector1MaterialSlot(OutputSlotCubeFaceId, kOutputSlotCubeFaceName, kOutputSlotCubeFaceName, SlotType.Output, 0));
            AddSlot(new Vector1MaterialSlot(OutputSlot3DSliceId, kOutputSlot3DSliceName, kOutputSlot3DSliceName, SlotType.Output, 0));
            RemoveSlotsNameNotMatching(validSlots);
        }

        public override string GetVariableNameForSlot(int slotId)
        {
            switch (slotId)
            {
                case OutputSlotCubeFaceId:
                    return "_CustomRenderTextureCubeFace";
                case OutputSlot3DSliceId:
                    return "_CustomRenderTexture3DSlice";
                default:
                    return "_CustomRenderTextureWidth";
            }
        }

        public void GenerateNodeFunction(FunctionRegistry registry, GenerationMode generationMode)
        {
            // For preview only we declare CRT defines
            if (generationMode == GenerationMode.Preview)
            {
                registry.builder.AppendLine("#define _CustomRenderTextureCubeFace 0.0");
                registry.builder.AppendLine("#define _CustomRenderTexture3DSlice 0.0");
            }
        }
    }

    [Title("Custom Texture", "Self")]
    class CustomTextureSelf : AbstractMaterialNode, IGeneratesFunction
    {
		private const string kOutputSlotSelf2DName = "Self Texture 2D";
		private const string kOutputSlotSelfCubeName = "Self Texture Cube";
		private const string kOutputSlotSelf3DName = "Self Texture 3D";
		
        public const int OutputSlotSelf2DId = 5;
        public const int OutputSlotSelfCubeId = 6;
        public const int OutputSlotSelf3DId = 7;

        public CustomTextureSelf()
        {
            name = "Custom Texture Self";
            UpdateNodeAfterDeserialization();
        }

        protected int[] validSlots => new[] { OutputSlotSelf2DId, OutputSlotSelfCubeId, OutputSlotSelf3DId };

        public sealed override void UpdateNodeAfterDeserialization()
        {
            AddSlot(new Texture2DMaterialSlot(OutputSlotSelf2DId, kOutputSlotSelf2DName, kOutputSlotSelf2DName, SlotType.Output, ShaderStageCapability.Fragment, false));
            AddSlot(new CubemapMaterialSlot(OutputSlotSelfCubeId, kOutputSlotSelfCubeName, kOutputSlotSelfCubeName, SlotType.Output, ShaderStageCapability.Fragment, false));
            AddSlot(new Texture2DMaterialSlot(OutputSlotSelf3DId, kOutputSlotSelf3DName, kOutputSlotSelf3DName, SlotType.Output, ShaderStageCapability.Fragment, false));
            RemoveSlotsNameNotMatching(validSlots);
        }

        public override string GetVariableNameForSlot(int slotId)
        {
            switch (slotId)
            {
                case OutputSlotSelf2DId:
                    return "_SelfTexture2D";
                case OutputSlotSelfCubeId:
                    return "_SelfTextureCube";
                default:
                    return "_SelfTexture3D";
            }
        }

        public void GenerateNodeFunction(FunctionRegistry registry, GenerationMode generationMode)
        {
            // For preview only we declare CRT defines
            if (generationMode == GenerationMode.Preview)
            {
                registry.builder.AppendLine("TEXTURE2D(_SelfTexture2D);");
                registry.builder.AppendLine("SAMPLER(sampler_SelfTexture2D);");
                registry.builder.AppendLine("TEXTURE2D(_SelfTextureCube);");
                registry.builder.AppendLine("SAMPLER(sampler_SelfTextureCube);");
                registry.builder.AppendLine("TEXTURE2D(_SelfTexture3D);");
                registry.builder.AppendLine("SAMPLER(sampler_SelfTexture3D);");
            }
        }
    }
}
#endif