using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using System.Xml;
using UnityEngine.Rendering;

namespace Mixture
{
    [System.Serializable]
    public abstract class TerrainTopologyNode : ComputeShaderNode
    {
        public enum VisualizeMode
        {
            COLOR,
            GREYSCALE
        }
        
        [Input("Heightmap")] public Texture heightMap;
        [Input("Terrain Data")] public MixtureTerrain terrainData;
        [Input("Terrain Dimension")]
        public Vector2 terrainDimension = new Vector2(1000, 1000);

        [Input("Terrain Height")] public float terrainHeight = 600;

        public virtual bool DoSmoothPass => false;
        //public float terrainHeight;
        [Output("Output")] public CustomRenderTexture output;

        [HideInInspector] public Texture2D m_gradient;
        [HideInInspector] public Texture2D m_posGradient;
        [HideInInspector] public Texture2D m_negGradient;

        private RenderTexture smoothedHeightmap;

        public virtual VisualizeMode visualizeMode => VisualizeMode.COLOR;
        public override bool showDefaultInspector => true;
        public override string name => "TerrainTopologyNode";
        public override Texture previewTexture => output;

        protected override string computeShaderResourcePath => "Mixture/TerrainTopology/TerrainTopology";


        protected abstract string KernelName { get; }

        protected int kernel;
        
        protected override void Enable()
        {
            base.Enable();
            this.kernel = this.computeShader.FindKernel(KernelName);
        }
        
        protected override bool ProcessNode(CommandBuffer cmd)
        {
            if (!base.ProcessNode(cmd) || !heightMap)
                return false;

            UpdateTempRenderTexture(ref output);
            Vector2 cellSize = new Vector2(
                (float)this.heightMap.width / (float)terrainDimension.x,
                (float)this.heightMap.height / (float)terrainDimension.y);
            if (terrainData != null && terrainData.TerrainData != null)
            {
                cellSize = terrainData.CellSize;
                terrainHeight = terrainData.Height;
            }
            
            if (visualizeMode == VisualizeMode.GREYSCALE)
            {
                CreateGradients(false);
            }
            else
            {
                CreateGradients(true);
            }
            
            cmd.SetComputeTextureParam(computeShader, kernel, "_Gradient", this.m_gradient);      
            cmd.SetComputeTextureParam(computeShader, kernel, "_PosGradient", this.m_posGradient);
            cmd.SetComputeTextureParam(computeShader, kernel, "_NegGradient", this.m_negGradient);

            cmd.SetComputeFloatParam(computeShader, "_TerrainHeight", this.terrainHeight);
            cmd.SetComputeVectorParam(computeShader, "_CellSize", cellSize);

            SmoothPass(cmd);
            computeShader.SetTexture(kernel, "_Output", output);
            return true;
        }

        protected void SmoothPass(CommandBuffer cmd)
        {
            if (!DoSmoothPass)
            {
                cmd.SetComputeTextureParam(computeShader, kernel, "_Heightmap", this.heightMap);
                return;
            }
            
            if(smoothedHeightmap != null)
                smoothedHeightmap.Release();

            smoothedHeightmap = new RenderTexture(heightMap.width, heightMap.height, 0, RenderTextureFormat.RFloat);
            smoothedHeightmap.enableRandomWrite = true;
            smoothedHeightmap.Create();
            var k = computeShader.FindKernel("SmoothHeightmap");
            cmd.SetComputeTextureParam(computeShader, k, "_Heightmap", heightMap);
            cmd.SetComputeTextureParam(computeShader, k, "_Output", smoothedHeightmap);
            
            DispatchCompute(cmd, k, heightMap.width, heightMap.height, 0);
            
            cmd.SetComputeTextureParam(computeShader, kernel, "_Heightmap", smoothedHeightmap);
        }


        #region Gradients

        private void CreateGradients(bool colored)
        {
            if (colored)
            {
                m_gradient = CreateGradient(VISUALIZE_GRADIENT.COOL_WARM);
                m_posGradient = CreateGradient(VISUALIZE_GRADIENT.WARM);
                m_negGradient = CreateGradient(VISUALIZE_GRADIENT.COOL);
            }
            else
            {
                m_gradient = CreateGradient(VISUALIZE_GRADIENT.BLACK_WHITE);
                m_posGradient = CreateGradient(VISUALIZE_GRADIENT.GREY_WHITE);
                m_negGradient = CreateGradient(VISUALIZE_GRADIENT.GREY_BLACK);
            }

            m_gradient.Apply();
            m_posGradient.Apply();
            m_negGradient.Apply();
        }

        public enum VISUALIZE_GRADIENT
        {
            WARM,
            COOL,
            COOL_WARM,
            GREY_WHITE,
            GREY_BLACK,
            BLACK_WHITE
        };

        private Texture2D CreateGradient(VISUALIZE_GRADIENT g)
        {
            switch (g)
            {
                case VISUALIZE_GRADIENT.WARM:
                    return CreateWarmGradient();

                case VISUALIZE_GRADIENT.COOL:
                    return CreateCoolGradient();

                case VISUALIZE_GRADIENT.COOL_WARM:
                    return CreateCoolToWarmGradient();

                case VISUALIZE_GRADIENT.GREY_WHITE:
                    return CreateGreyToWhiteGradient();

                case VISUALIZE_GRADIENT.GREY_BLACK:
                    return CreateGreyToBlackGradient();

                case VISUALIZE_GRADIENT.BLACK_WHITE:
                    return CreateBlackToWhiteGradient();
            }

            return null;
        }

        private Texture2D CreateWarmGradient()
        {
            var gradient = new Texture2D(5, 1, TextureFormat.ARGB32, false, true);
            gradient.SetPixel(0, 0, new Color32(80, 230, 80, 255));
            gradient.SetPixel(1, 0, new Color32(180, 230, 80, 255));
            gradient.SetPixel(2, 0, new Color32(230, 230, 80, 255));
            gradient.SetPixel(3, 0, new Color32(230, 180, 80, 255));
            gradient.SetPixel(4, 0, new Color32(230, 80, 80, 255));
            gradient.wrapMode = TextureWrapMode.Clamp;

            return gradient;
        }

        private Texture2D CreateCoolGradient()
        {
            var gradient = new Texture2D(5, 1, TextureFormat.ARGB32, false, true);
            gradient.SetPixel(0, 0, new Color32(80, 230, 80, 255));
            gradient.SetPixel(1, 0, new Color32(80, 230, 180, 255));
            gradient.SetPixel(2, 0, new Color32(80, 230, 230, 255));
            gradient.SetPixel(3, 0, new Color32(80, 180, 230, 255));
            gradient.SetPixel(4, 0, new Color32(80, 80, 230, 255));
            gradient.wrapMode = TextureWrapMode.Clamp;

            return gradient;
        }

        private Texture2D CreateCoolToWarmGradient()
        {
            var gradient = new Texture2D(9, 1, TextureFormat.ARGB32, false, true);
            gradient.SetPixel(0, 0, new Color32(80, 80, 230, 255));
            gradient.SetPixel(1, 0, new Color32(80, 180, 230, 255));
            gradient.SetPixel(2, 0, new Color32(80, 230, 230, 255));
            gradient.SetPixel(3, 0, new Color32(80, 230, 180, 255));
            gradient.SetPixel(4, 0, new Color32(80, 230, 80, 255));
            gradient.SetPixel(5, 0, new Color32(180, 230, 80, 255));
            gradient.SetPixel(6, 0, new Color32(230, 230, 80, 255));
            gradient.SetPixel(7, 0, new Color32(230, 180, 80, 255));
            gradient.SetPixel(8, 0, new Color32(230, 80, 80, 255));
            gradient.wrapMode = TextureWrapMode.Clamp;

            return gradient;
        }

        private Texture2D CreateGreyToWhiteGradient()
        {
            var gradient = new Texture2D(3, 1, TextureFormat.ARGB32, false, true);
            gradient.SetPixel(0, 0, new Color32(128, 128, 128, 255));
            gradient.SetPixel(1, 0, new Color32(192, 192, 192, 255));
            gradient.SetPixel(2, 0, new Color32(255, 255, 255, 255));
            gradient.wrapMode = TextureWrapMode.Clamp;

            return gradient;
        }

        private Texture2D CreateGreyToBlackGradient()
        {
            var gradient = new Texture2D(3, 1, TextureFormat.ARGB32, false, true);
            gradient.SetPixel(0, 0, new Color32(128, 128, 128, 255));
            gradient.SetPixel(1, 0, new Color32(64, 64, 64, 255));
            gradient.SetPixel(2, 0, new Color32(0, 0, 0, 255));
            gradient.wrapMode = TextureWrapMode.Clamp;

            return gradient;
        }

        private Texture2D CreateBlackToWhiteGradient()
        {
            var gradient = new Texture2D(5, 1, TextureFormat.ARGB32, false, true);
            gradient.SetPixel(0, 0, new Color32(0, 0, 0, 255));
            gradient.SetPixel(1, 0, new Color32(64, 64, 64, 255));
            gradient.SetPixel(2, 0, new Color32(128, 128, 128, 255));
            gradient.SetPixel(3, 0, new Color32(192, 192, 192, 255));
            gradient.SetPixel(4, 0, new Color32(255, 255, 255, 255));
            gradient.wrapMode = TextureWrapMode.Clamp;

            return gradient;
        }

        #endregion

        [CustomPortBehavior(nameof(terrainDimension))]
        public IEnumerable<PortData> ShowPortDatas(List<SerializableEdge> edges)
        {
            if (this.GetAllEdges().FirstOrDefault(edge => edge.inputPort.fieldName == nameof(this.terrainData)) != null)
            {
                yield break;
            }

            yield return new PortData()
            {
                displayType = typeof(Vector2),
                displayName = "Terrain Dimension",
                identifier = "dimension",
            };
        }
        
        [CustomPortBehavior(nameof(terrainHeight))]
        public IEnumerable<PortData> ShowPortDataForTerrainHeight(List<SerializableEdge> edges)
        {
            if (this.GetAllEdges().FirstOrDefault(edge => edge.inputPort.fieldName == nameof(this.terrainData)) != null)
            {
                yield break;
            }

            yield return new PortData()
            {
                displayType = typeof(float),
                displayName = "Terrain Height",
                identifier = "height",
            };
        }
        
        
    }
}