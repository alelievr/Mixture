using System;
using GraphProcessor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Mixture
{
    [Documentation(@"
The flow map represent the path a small amount of water flowing over the terrain will take.
It's a good way to make rivers or erosion effects

The VectorField output represent the direction took by the water in each point.
")]
    [System.Serializable, NodeMenuItem("Terrain Topology/Flow Map")]
    public class TerrainFlowMap : TerrainTopologyNode
    {
        public override string name => "Terrain Flow Map";

        [Input("Iteration")][ShowInInspector(true)] public int iteration = 5;
        [Output("Vector Field"), NonSerialized]public CustomRenderTexture vectorField;
        protected override string KernelName => "FillWaterMap";
        public override bool showDefaultInspector => true;
        public override Texture previewTexture => output;
        private RenderTexture waterMap;
        private RenderTexture outFlow;
        void CreateRenderTextures()
        {
            int width = settings.GetResolvedWidth(graph);
            int height = settings.GetResolvedHeight(graph);
            
            if(waterMap != null)
                waterMap.Release();
            
            waterMap = new RenderTexture(width, height, 0, RenderTextureFormat.R16)
            {
                enableRandomWrite = true
            };
            waterMap.Create();

            if (outFlow != null)
            {
                outFlow.Release();
            }

            outFlow = new RenderTexture(width, height, 0, RenderTextureFormat.RFloat)
            {
                dimension = TextureDimension.Tex3D,
                volumeDepth = 4,
                enableRandomWrite = true
            };
            outFlow.Create();

            if (vectorField != null)
            {
                vectorField.Release();
            }

            vectorField = new CustomRenderTexture(width, height, RenderTextureFormat.RG32,
                RenderTextureReadWrite.Default);
            vectorField.enableRandomWrite = true;
            
            vectorField.Create();
        }

        protected override void Disable()
        {
            base.Disable();
            if(waterMap != null)
                waterMap.Release();
            if(outFlow != null)
                outFlow.Release();
        }

        protected override bool ProcessNode(CommandBuffer cmd)
        {
            CreateRenderTextures();
            UpdateTempRenderTexture(ref vectorField);
            if (!base.ProcessNode(cmd))
                return false;

            if (computeShader == null)
                return false;
            
            int fillWaterMapKernel = computeShader.FindKernel("FillWaterMap");
            cmd.SetComputeTextureParam(computeShader, fillWaterMapKernel, "_WaterMap", waterMap);
            DispatchCompute(cmd, fillWaterMapKernel, output.width, output.height);

            int computeOutFlowKernel = computeShader.FindKernel("OutFlow");
            cmd.SetComputeTextureParam(computeShader, computeOutFlowKernel, "_OutFlow", outFlow);
            cmd.SetComputeTextureParam(computeShader, computeOutFlowKernel, "_WaterMap", waterMap);
            cmd.SetComputeTextureParam(computeShader, computeOutFlowKernel, "_Heightmap", heightMap);
            cmd.SetComputeTextureParam(computeShader, computeOutFlowKernel, "_VectorField", vectorField);
            int updateWaterKernel = computeShader.FindKernel("UpdateWaterMapKernel");
            cmd.SetComputeTextureParam(computeShader, updateWaterKernel, "_OutFlow", outFlow);
            cmd.SetComputeTextureParam(computeShader, updateWaterKernel, "_WaterMap", waterMap);
            cmd.SetComputeTextureParam(computeShader, updateWaterKernel, "_Heightmap", heightMap);
            for (int i = 0; i < iteration; i++)
            {
                DispatchCompute(cmd, computeOutFlowKernel, output.width, output.height);
                DispatchCompute(cmd, updateWaterKernel, output.width, output.height);
            }

            int velocityFieldKernel = computeShader.FindKernel("CalculateVelocityFieldKernel");
            cmd.SetComputeTextureParam(computeShader, velocityFieldKernel, "_OutFlow", outFlow);
            cmd.SetComputeTextureParam(computeShader, velocityFieldKernel, "_WaterMap", waterMap);
            cmd.SetComputeTextureParam(computeShader, velocityFieldKernel, "_Heightmap", heightMap);
            cmd.SetComputeTextureParam(computeShader, velocityFieldKernel, "_Output", output);
            cmd.SetComputeTextureParam(computeShader, velocityFieldKernel, "_VectorField", vectorField);
            DispatchCompute(cmd, velocityFieldKernel, output.width, output.height);
            UpdateTempRenderTexture(ref output);
            
            return true;
        }
    }
}