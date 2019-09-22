﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using System.IO;
using System;
using UnityEngine.Rendering;

using UnityEngine.Experimental.Rendering;

namespace Mixture
{
    [Serializable, NodeMenuItem("External Output")]
    public class ExternalOutputNode : OutputNode
    {
        public override string name => "External Output";

        public Texture asset;

        public override bool hasSettings => true;
        protected override MixtureRTSettings defaultRTSettings => new MixtureRTSettings
        {
            heightMode = OutputSizeMode.Fixed,
            widthMode = OutputSizeMode.Fixed,
            depthMode = OutputSizeMode.Fixed,
            height = 512,
            width = 512,
            sliceCount = 1,
            dimension = OutputDimension.Texture2D,
            targetFormat = OutputFormat.RGBA_LDR,
            editFlags = EditFlags.Height | EditFlags.Width| EditFlags.TargetFormat,
            wrapMode = TextureWrapMode.Repeat,
            filterMode = FilterMode.Bilinear,
        };

        protected override void Enable()
        {
            // Do NOT Call base.Enable() as it references the node as the main output of the graph.
            //base.Enable();

            // Sanitize the RT Settings for the output node, they must contains only valid information for the output node
            if (rtSettings.targetFormat == OutputFormat.Default)
                rtSettings.targetFormat = OutputFormat.RGBA_Float;
            if (rtSettings.dimension == OutputDimension.Default)
                rtSettings.dimension = OutputDimension.Texture2D;

            if (graph.isRealtime)
            {
                tempRenderTexture = graph.outputTexture as CustomRenderTexture;
            }
            else
            {
                UpdateTempRenderTexture(ref tempRenderTexture);
            }

            onSettingsChanged += () =>
            {
                ProcessNode();
            };
        }

        protected override bool ProcessNode()
        {
            return base.ProcessNode();
        }
    }
}