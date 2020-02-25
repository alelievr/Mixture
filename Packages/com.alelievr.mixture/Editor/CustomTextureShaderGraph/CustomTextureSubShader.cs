#if MIXTURE_SHADERGRAPH
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Graphing;
using UnityEditor;
using UnityEditor.ShaderGraph;
using UnityEngine.Rendering;
using UnityEditor.ShaderGraph.Internal;

namespace Mixture
{
    interface ICustomTextureSubShader : ISubShader
    {

    }

    [Serializable]
    class CustomTextureSubShader : ICustomTextureSubShader
    {
        readonly string customTextureTemplateGUID = "afa536a0de48246de92194c9e987b0b8";

        string GenerateGraph(CustomTextureMasterNode masterNode, string template)
        {
            // ----------------------------------------------------- //
            //                         SETUP                         //
            // ----------------------------------------------------- //

            // -------------------------------------
            // String builders

            var shaderProperties = new PropertyCollector();
            var shaderPropertyUniforms = new ShaderStringBuilder(1);
            var functionBuilder = new ShaderStringBuilder(1);
            var functionRegistry = new FunctionRegistry(functionBuilder);

            var defines = new ShaderStringBuilder(1);
            var graph = new ShaderStringBuilder(0);

            var surfaceDescriptionStruct = new ShaderStringBuilder(1);
            var surfaceDescriptionFunction = new ShaderStringBuilder(1);

            var pixelShader = new ShaderStringBuilder(2);
            var pixelShaderSurfaceInputs = new ShaderStringBuilder(2);
            var pixelShaderSurfaceRemap = new ShaderStringBuilder(2);

            // -------------------------------------
            // Get Slot and Node lists per stage

            var pixelSlots = masterNode.GetSlots<MaterialSlot>().ToList(); // All slots are pixels
            var pixelNodes = ListPool<AbstractMaterialNode>.Get();
            NodeUtils.DepthFirstCollectNodesFromNode(pixelNodes, masterNode, NodeUtils.IncludeSelf.Include, pixelSlots.Select(s => s.id).ToList());

            // -------------------------------------
            // Get Requirements

            var pixelRequirements = ShaderGraphRequirements.FromNodes(pixelNodes, ShaderStageCapability.Fragment);
            var graphRequirements = pixelRequirements;
            var surfaceRequirements = ShaderGraphRequirements.FromNodes(pixelNodes, ShaderStageCapability.Fragment, false);

            // ----------------------------------------------------- //
            //                START SHADER GENERATION                //
            // ----------------------------------------------------- //

            // -------------------------------------
            // Generate Output structure for Surface Description function

            SubShaderGenerator.GenerateSurfaceDescriptionStruct(surfaceDescriptionStruct, pixelSlots);

            // -------------------------------------
            // Generate Surface Description function

            List<int>[] perm = new List<int>[pixelNodes.Count];

            SubShaderGenerator.GenerateSurfaceDescriptionFunction(
                pixelNodes,
                perm,
                masterNode,
                masterNode.owner as GraphData,
                surfaceDescriptionFunction,
                functionRegistry,
                shaderProperties,
                new KeywordCollector(),
                GenerationMode.ForReals);
                // pixelRequirements,
                // GenerationMode.ForReals, // we'll handle preview later
                // "PopulateSurfaceData",
                // "SurfaceDescription",
                // null,
                // pixelSlots);

            // -------------------------------------
            // Property uniforms

            shaderProperties.GetPropertiesDeclaration(shaderPropertyUniforms, GenerationMode.ForReals, masterNode.owner.concretePrecision);

            // -------------------------------------
            // Generate standard transformations
            // This method ensures all required transform data is available in vertex and pixel stages

            // ShaderGenerator.GenerateStandardTransforms(
            //     3,
            //     10,
            //     // We don't need vertex things
            //     new ShaderStringBuilder(),
            //     new ShaderStringBuilder(),
            //     new ShaderStringBuilder(),
            //     new ShaderStringBuilder(),
            //     pixelShader,
            //     pixelShaderSurfaceInputs,
            //     pixelRequirements,
            //     surfaceRequirements,
            //     modelRequirements,
            //     new ShaderGraphRequirements(),
            //     CoordinateSpace.World);

            // -------------------------------------
            // Generate pixel shader surface remap

            foreach (var slot in pixelSlots)
            {
                pixelShaderSurfaceRemap.AppendLine("{0} = surf.{0};", slot.shaderOutputName);
            }

            // -------------------------------------
            // Extra pixel shader work

            var faceSign = new ShaderStringBuilder();

            if (pixelRequirements.requiresFaceSign)
                faceSign.AppendLine(", half FaceSign : VFACE");

            // ----------------------------------------------------- //
            //                      FINALIZE                         //
            // ----------------------------------------------------- //

            // -------------------------------------
            // Combine Graph sections

            graph.AppendLines(shaderPropertyUniforms.ToString());

            graph.AppendLine(functionBuilder.ToString());

            graph.AppendLine(surfaceDescriptionStruct.ToString());
            graph.AppendLine(surfaceDescriptionFunction.ToString());

            // -------------------------------------
            // Generate final subshader

            var resultPass = template.Replace("${Tags}", string.Empty);

            resultPass = resultPass.Replace("${Graph}", graph.ToString());

            resultPass = resultPass.Replace("${FaceSign}", faceSign.ToString());
            resultPass = resultPass.Replace("${PixelShader}", pixelShader.ToString());
            resultPass = resultPass.Replace("${PixelShaderSurfaceRemap}", pixelShaderSurfaceRemap.ToString());

            return resultPass;
        }

        string ISubShader.GetSubshader(IMasterNode iMasterNode, GenerationMode mode, List<string> sourceAssetDependencyPaths)
        {
            // this is the format string for building the 'C# qualified assembly type names' for $buildType() commands
            // string buildTypeAssemblyNameFormat = "UnityEditor.Rendering.HighDefinition.HDRPShaderStructs+{0}, " + typeof(HDSubShaderUtilities).Assembly.FullName.ToString();

            if (sourceAssetDependencyPaths != null)
            {
                sourceAssetDependencyPaths.Add(AssetDatabase.GUIDToAssetPath("5b2d4724a38a5485ba5e7dc2f7d86f1a")); // CustomTextureSubShader.cs
                sourceAssetDependencyPaths.Add(AssetDatabase.GUIDToAssetPath(customTextureTemplateGUID));
            }

            string templateLocation = AssetDatabase.GUIDToAssetPath(customTextureTemplateGUID);
            string templateCode = File.ReadAllText(templateLocation);
            return GenerateGraph(iMasterNode as CustomTextureMasterNode, templateCode);

            // Get the template file
            // var templateLocation = ShaderGenerator.GetTemplatePath("CustomTextureSubshader.template");
            // if (!File.Exists(templateLocation))
            //     return string.Empty;

            // var subShaderTemplate = File.ReadAllText(templateLocation);

            // var masterNode = iMasterNode as CustomTextureMasterNode;
            // var subShader = new ShaderGenerator();

            // var builder = new ShaderStringBuilder();
            // builder.IncreaseIndent();
            // builder.IncreaseIndent();

            // var surfaceDescriptionFunction = new ShaderGenerator();
            // var surfaceDescriptionStruct = new ShaderGenerator();
            // var functionRegistry = new FunctionRegistry(builder);

            // var shaderProperties = new PropertyCollector();

            // var activeNodeList = ListPool<INode>.Get();
            // NodeUtils.DepthFirstCollectNodesFromNode(activeNodeList, masterNode, NodeUtils.IncludeSelf.Include);

            // var requirements = ShaderGraphRequirements.FromNodes(activeNodeList);

            // var slots = new List<MaterialSlot>();
            // slots.Add(masterNode.FindSlot<MaterialSlot>(0));

            // GraphUtil.GenerateSurfaceDescriptionStruct(surfaceDescriptionStruct, slots, true);

            // GraphUtil.GenerateSurfaceDescription(
            //     activeNodeList,
            //     masterNode,
            //     masterNode.owner as AbstractMaterialGraph,
            //     surfaceDescriptionFunction,
            //     functionRegistry,
            //     shaderProperties,
            //     requirements,
            //     mode);

            // var graph = new ShaderGenerator();
            // graph.AddShaderChunk(shaderProperties.GetPropertiesDeclaration(2), false);
            // graph.AddShaderChunk(builder.ToString(), false);
            // graph.AddShaderChunk(surfaceDescriptionStruct.GetShaderString(2), false);
            // graph.AddShaderChunk(surfaceDescriptionFunction.GetShaderString(2), false);

            // var tagsVisitor = new ShaderGenerator();
            // var blendingVisitor = new ShaderGenerator();
            // var cullingVisitor = new ShaderGenerator();
            // var zTestVisitor = new ShaderGenerator();
            // var zWriteVisitor = new ShaderGenerator();

            // var materialOptions = new SurfaceMaterialOptions();
            // materialOptions.GetTags(tagsVisitor);
            // materialOptions.GetBlend(blendingVisitor);
            // materialOptions.GetCull(cullingVisitor);
            // materialOptions.GetDepthTest(zTestVisitor);
            // materialOptions.GetDepthWrite(zWriteVisitor);

            // var localPixelShader = new ShaderGenerator();
            // var localSurfaceInputs = new ShaderGenerator();
            // var surfaceOutputRemap = new ShaderGenerator();

            // foreach (var channel in requirements.requiresMeshUVs.Distinct())
            //     localSurfaceInputs.AddShaderChunk(string.Format("surfaceInput.{0} = {1};", channel.GetUVName(), string.Format("half4(input.texCoord{0}, 0, 0)", (int)channel)), false);


            // // MY CODE
            
            // var properties = new ShaderGenerator();
            // properties.AddShaderChunk(shaderProperties.GetPropertiesDeclaration(2), false);

            // var thing = new ShaderGenerator();
            // thing.AddShaderChunk(surfaceDescriptionFunction.GetShaderString(2), false);


            // var subShaderOutput = subShaderTemplate;

            
            // subShaderOutput = subShaderOutput.Replace("${Graph}", graph.GetShaderString(3));

            // subShaderOutput = subShaderOutput.Replace("${SurfaceOutputRemap}", surfaceOutputRemap.GetShaderString(3));

            // //subShaderOutput = subShaderOutput.Replace("${ShaderPropertyUsages}", "");

            // //subShaderOutput = subShaderOutput.Replace("${ShaderFunctions}", "");
            // //subShaderOutput = subShaderOutput.Replace("${PixelShaderBody}", localPixelShader.GetShaderString(3));
            // //subShaderOutput = subShaderOutput.Replace("${PixelShaderBody}", "return float4(0,1,0,1);");

            // //EditorUtility.DisplayDialog("Shader", subShaderOutput, "Close");

            // return subShaderOutput;
        }

        // Supports all SRPs
        bool ISubShader.IsPipelineCompatible(RenderPipelineAsset renderPipelineAsset) => true;

        int ISubShader.GetPreviewPassIndex() => 0;
    }
}
#endif