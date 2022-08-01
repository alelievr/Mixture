using System;
using System.Collections.Generic;
using GraphProcessor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Mixture
{
    [Documentation(@"
The curvature map represents the convexity/concavity of the surface.

There are a lot of ways to measure the curvature. The most useful are verical, horizontal, mean, gaussian, minimal and maximal.
In Color Mode, Concave areas are represented with blue while convex areas are represented with red
In Grayscale mode, concave areas are represented with black while convex areas are represented with white.
    ")]
    [System.Serializable, NodeMenuItem("Terrain Topology/Curvature")]
    public class TerrainCurvatureNode : TerrainTopologyNode
    {
        [ShowInInspector(true)] public CurvatureMode mode;
        public override string name => "Terrain Curvature Map";
        public override bool isRenamable => true;
        
        protected override string KernelName => "Curvature";

        public VisualizeMode _visualizeMode;

        public override bool DoSmoothPass => true;

        public override VisualizeMode visualizeMode => _visualizeMode;

        public enum CurvatureMode
        {
            PLAN,
            HORIZONTAL,
            VERTICAL,
            MEAN,
            GAUSSIAN,
            MINIMAL,
            MAXIMAL,
            UNSPHERICITY,
            ROTOR,
            DIFFERENCE,
            HORIZONTAL_EXCESS,
            VERTICAL_EXCESS,
            RING,
            ACCUMULATION
        }

        private Dictionary<CurvatureMode, string> keywordMap = new Dictionary<CurvatureMode, string>()
        {
            {CurvatureMode.PLAN, "CURVATURE_PLAN"},
            {CurvatureMode.HORIZONTAL, "CURVATURE_HORIZONTAL"},
            {CurvatureMode.VERTICAL, "CURVATURE_VERTICAL"},
            {CurvatureMode.MEAN, "CURVATURE_MEAN"},
            {CurvatureMode.GAUSSIAN, "CURVATURE_GAUSSIAN"},
            {CurvatureMode.MINIMAL, "CURVATURE_MINIMAL"},
            {CurvatureMode.MAXIMAL, "CURVATURE_MAXIMAL"},
            {CurvatureMode.UNSPHERICITY, "CURVATURE_UNSPHERICITY"},
            {CurvatureMode.ROTOR, "CURVATURE_ROTOR"},
            {CurvatureMode.DIFFERENCE, "CURVATURE_DIFFERENCE"},
            {CurvatureMode.HORIZONTAL_EXCESS, "CURVATURE_HORIZONTAL_EXCESS"},
            {CurvatureMode.VERTICAL_EXCESS, "CURVATURE_VERTICAL_EXCESS"},
            {CurvatureMode.RING, "CURVATURE_RING"},
            {CurvatureMode.ACCUMULATION, "CURVATURE_ACCUMULATION"},
            
        };

        public override bool showDefaultInspector => true;

        
        
        protected override bool ProcessNode(CommandBuffer cmd)
        {
            if (!base.ProcessNode(cmd))
                return false;

            foreach (var item in keywordMap)
            {
                if(item.Key == mode)
                    computeShader.EnableKeyword(item.Value);
                else
                {
                    computeShader.DisableKeyword(item.Value);
                }
            }
            
            //cmd.SetComputeIntParam(computeShader, "_UseRamp", this.visualizeMode == VisualizeMode.COLOR ? 1 : 0);
            
            DispatchCompute(cmd, kernel, output.width, output.height);
            UpdateTempRenderTexture(ref output);
            return true;
        }
    }
}