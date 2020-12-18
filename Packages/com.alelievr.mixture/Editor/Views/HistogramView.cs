using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace Mixture
{
	public class HistogramView : VisualElement 
	{
        class DrawHistogram : ImmediateModeElement
        {
            HistogramData data;

            public DrawHistogram(HistogramData data, int height = 200)
            {
                this.data = data;
                style.flexGrow = 1;
                style.height = height;
            }

            public void UpdateHistogramData(HistogramData data) => this.data = data;

            protected override void ImmediateRepaint()
            {
                HistogramUtility.SetupHistogramPreviewMaterial(data);
                EditorGUI.DrawPreviewTexture(contentRect, Texture2D.whiteTexture, data.previewMaterial);

                // We can also write stuff in the histogram graph with GUI. functions
                // GUI.Label(contentRect, "Hello world");
            }
        }

        public HistogramView(HistogramData data, MixtureGraphView graphView, int height = 200)
        {
            var modeField = new EnumField("Histogram Mode", data.mode);
            modeField.RegisterValueChangedCallback(e => {
                graphView.RegisterCompleteObjectUndo("Changed histogram Mode");
                data.mode = (HistogramMode)e.newValue;
            });
            Add(modeField);
            var drawHistogram = new DrawHistogram(data, height);
            Add(drawHistogram);

            var minMaxLabel = new Label();
            Add(minMaxLabel);
            schedule.Execute(() => {
                minMaxLabel.text = $"Luminance min: {data.minLuminance:F3}, max: {data.maxLuminance:F3}";
            }).Every(20);
        }
	}
}