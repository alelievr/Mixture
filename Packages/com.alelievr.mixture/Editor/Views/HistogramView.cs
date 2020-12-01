using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	public class HistogramView : ImmediateModeElement 
	{
        Material        histogramPreview;
        ComputeBuffer   histogramBuffer;

        public HistogramView()
        {
			histogramPreview = new Material(Shader.Find("Hidden/HistogramPreview"));
            style.width = 120;
            style.height = 120;
        }

        public void SetHistogramBuffer(ComputeBuffer buffer)
        {
            histogramBuffer = buffer;
        }

        protected override void ImmediateRepaint()
        {
            histogramPreview.SetBuffer("_Histogram", histogramBuffer);
            Graphics.DrawTexture(contentRect, Texture2D.whiteTexture, histogramPreview, 0);
        }
	}
}