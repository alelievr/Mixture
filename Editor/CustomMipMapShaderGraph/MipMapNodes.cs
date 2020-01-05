#if MIXTURE_SHADERGRAPH
using UnityEngine;
using UnityEditor.ShaderGraph;
using UnityEditor.Graphing;
using UnityEditor.ShaderGraph.Internal;

namespace Mixture
{
    [Title("Mip Map", "Sample Current Mip")]
    class MipMapInputNode : AbstractMaterialNode, IGeneratesBodyCode
    {
		private const string kInputSlotUVName = "UV";
		private const string kOutputSlotColorName = "Color";
		private const string kOutputSlotMipLevelName = "Mip Level";
		
        public const int InputSlotUVId = 0;
        public const int OutputSlotColorId = 1;
        public const int OutputSlotMipLevelId = 2;

        public MipMapInputNode()
        {
            name = "Sample Current Mip";
            UpdateNodeAfterDeserialization();
        }

        protected int[] validSlots => new[] { InputSlotUVId, OutputSlotColorId, OutputSlotMipLevelId };

        public sealed override void UpdateNodeAfterDeserialization()
        {
            AddSlot(new Vector3MaterialSlot(InputSlotUVId, kInputSlotUVName, kInputSlotUVName, SlotType.Input, Vector3.zero));
            AddSlot(new Vector4MaterialSlot(OutputSlotColorId, kOutputSlotColorName, kOutputSlotColorName, SlotType.Output, Vector4.zero));
            AddSlot(new Vector1MaterialSlot(OutputSlotMipLevelId, kOutputSlotMipLevelName, kOutputSlotMipLevelName, SlotType.Output, 0));
            RemoveSlotsNameNotMatching(validSlots);
        }

        public void GenerateNodeCode(ShaderStringBuilder sb, GenerationMode generationMode)
        {
            string mipLevel = generationMode == GenerationMode.Preview ? "0" : "_CurrentMipLevel";
            string sample = generationMode == GenerationMode.Preview ? "0" : $"SAMPLE_LOD_X(_InputTexture, {GetSlotValue(InputSlotUVId, generationMode)}, {GetSlotValue(InputSlotUVId, generationMode)}, _CurrentMipLevel)";
            // TODO: support of cubemaps
            sb.AppendLine($@"
$precision4 {GetVariableNameForSlot(OutputSlotColorId)} = {sample};
$precision {GetVariableNameForSlot(OutputSlotMipLevelId)} = {mipLevel};
            ");
        }
    }
}
#endif