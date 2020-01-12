#if MIXTURE_SHADERGRAPH
using UnityEngine;
using UnityEditor.ShaderGraph;
using UnityEditor.Graphing;
using UnityEditor.ShaderGraph.Internal;

namespace Mixture
{
    [Title("Mip Map", "Sample Current Mip")]
    class SampleMipMap : AbstractMaterialNode, IGeneratesBodyCode
    {
		private const string kInputSlotUVName = "UV";
		private const string kOutputSlotColorName = "Color";
		
        public const int InputSlotUVId = 0;
        public const int OutputSlotColorId = 1;

        public SampleMipMap()
        {
            name = "Sample Current Mip";
            UpdateNodeAfterDeserialization();
        }

        protected int[] validSlots => new[] { InputSlotUVId, OutputSlotColorId };

        public sealed override void UpdateNodeAfterDeserialization()
        {
            AddSlot(new Vector3MaterialSlot(InputSlotUVId, kInputSlotUVName, kInputSlotUVName, SlotType.Input, Vector3.zero));
            AddSlot(new Vector4MaterialSlot(OutputSlotColorId, kOutputSlotColorName, kOutputSlotColorName, SlotType.Output, Vector4.zero));
            RemoveSlotsNameNotMatching(validSlots);
        }

        public void GenerateNodeCode(ShaderStringBuilder sb, GenerationMode generationMode)
        {
            // TODO: support of cubemaps
            string sample = generationMode == GenerationMode.Preview ? "0" : $"SAMPLE_LOD_X_LINEAR_CLAMP(_InputTexture, {GetSlotValue(InputSlotUVId, generationMode)}, {GetSlotValue(InputSlotUVId, generationMode)}, _CurrentMipLevel)";
            sb.AppendLine($@"$precision4 {GetVariableNameForSlot(OutputSlotColorId)} = {sample};");
        }
    }

    [Title("Mip Map", "Current Mip Level")]
    class MipLevel : AbstractMaterialNode, IGeneratesBodyCode
    {
		private const string kOutputSlotMipLevelName = "Mip Level";
		private const string kOutputSlotMaxMipName = "Mip Level";
		
        public const int OutputSlotMipLevelId = 0;
        public const int OutputSlotMaxMipLevelId = 1;

        public MipLevel()
        {
            name = "Current Mip Level";
            UpdateNodeAfterDeserialization();
        }

        protected int[] validSlots => new[] { OutputSlotMipLevelId, OutputSlotMaxMipLevelId };

        public sealed override void UpdateNodeAfterDeserialization()
        {
            AddSlot(new Vector1MaterialSlot(OutputSlotMipLevelId, kOutputSlotMipLevelName, kOutputSlotMipLevelName, SlotType.Output, 0));
            AddSlot(new Vector1MaterialSlot(OutputSlotMaxMipLevelId, kOutputSlotMaxMipName, kOutputSlotMaxMipName, SlotType.Output, 0));
            RemoveSlotsNameNotMatching(validSlots);
        }

        public void GenerateNodeCode(ShaderStringBuilder sb, GenerationMode generationMode)
        {
            // TODO: max mip level
            string mipLevel = generationMode == GenerationMode.Preview ? "0" : "_CurrentMipLevel";
            string maxMipLevel = generationMode == GenerationMode.Preview ? "0" : "_MaxMipLevel";
            sb.AppendLine($@"$precision {GetVariableNameForSlot(OutputSlotMipLevelId)} = {mipLevel};");
            sb.AppendLine($@"$precision {GetVariableNameForSlot(OutputSlotMaxMipLevelId)} = {maxMipLevel};");
        }
    }

    [Title("Mip Map", "Current Mip Size")]
    class MipMapSize : AbstractMaterialNode, IGeneratesBodyCode
    {
		private const string kOutputSlotSizeName = "Size In Pixel";
		private const string kOutputSlotRcpSizeName = "Rcp Size";
		private const string kOutputSlotHalfRcpSizeName = "Half Rcp Size";
		
        public const int OutputSlotSizeId = 0;
        public const int OutputSlotRcpSizeId = 1;
        public const int OutputSlotHalfRcpSizeId = 2;

        public MipMapSize()
        {
            name = "Current Mip Size";
            UpdateNodeAfterDeserialization();
        }

        protected int[] validSlots => new[] { OutputSlotSizeId, OutputSlotRcpSizeId, OutputSlotHalfRcpSizeId };

        public sealed override void UpdateNodeAfterDeserialization()
        {
            AddSlot(new Vector3MaterialSlot(OutputSlotSizeId, kOutputSlotSizeName, kOutputSlotSizeName, SlotType.Output, Vector3.zero));
            AddSlot(new Vector3MaterialSlot(OutputSlotRcpSizeId, kOutputSlotRcpSizeName, kOutputSlotRcpSizeName, SlotType.Output, Vector3.zero));
            AddSlot(new Vector3MaterialSlot(OutputSlotHalfRcpSizeId, kOutputSlotHalfRcpSizeName, kOutputSlotHalfRcpSizeName, SlotType.Output, Vector3.zero));
            RemoveSlotsNameNotMatching(validSlots);
        }

        public void GenerateNodeCode(ShaderStringBuilder sb, GenerationMode generationMode)
        {
            string size = generationMode == GenerationMode.Preview ? "0" : "_InputTextureSize";
            string rcpSize = generationMode == GenerationMode.Preview ? "0" : "_InputTextureSizeRcp";
            sb.AppendLine($@"$precision3 {GetVariableNameForSlot(OutputSlotSizeId)} = {size};");
            sb.AppendLine($@"$precision3 {GetVariableNameForSlot(OutputSlotRcpSizeId)} = {rcpSize};");
            sb.AppendLine($@"$precision3 {GetVariableNameForSlot(OutputSlotHalfRcpSizeId)} = {rcpSize} * 0.5;");
        }
    }
}
#endif