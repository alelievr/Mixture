using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using GraphProcessor;
using UnityEngine.Experimental.Rendering;

[NodeCustomEditor(typeof(OutputNode))]
public class OutputNodeView : BaseNodeView
{
	VisualElement	shaderCreationUI;
	VisualElement	materialEditorUI;
	MaterialEditor	materialEditor;
	OutputNode		outputNode;

	public override void OnCreated()
	{
		if (outputNode.outputTexture != null)
        {
			AssetDatabase.AddObjectToAsset(outputNode.outputTexture, owner.graph);
        }
	}

	public override void Enable()
	{
        // Fix the size of the node
        var currentPos = GetPosition();
        SetPosition(new Rect(currentPos.x, currentPos.y, 200, 400));

		outputNode = nodeTarget as OutputNode;

        var targetSizeField = FieldFactory.CreateField(typeof(Vector2Int), outputNode.targetSize, (newValue) => {
            owner.RegisterCompleteObjectUndo("Updated " + newValue);
            outputNode.targetSize = (Vector2Int)newValue;
        });
        (targetSizeField as Vector2IntField).label = "Final size";

        var graphicsFormatField = new EnumField(outputNode.format) {
            label = "format",
        };
        graphicsFormatField.RegisterValueChangedCallback(e => {
            owner.RegisterCompleteObjectUndo("Updated " + e.newValue);
            outputNode.format = (GraphicsFormat)e.newValue;
        });

        controlsContainer.Add(targetSizeField);
        controlsContainer.Add(graphicsFormatField);

        controlsContainer.Add(new Image {
            image = outputNode.outputTexture,
            scaleMode = ScaleMode.ScaleToFit,
        });

        controlsContainer.Add(new Label("TODO: export image button"));
	}

	public override void OnRemoved()
	{
		AssetDatabase.RemoveObjectFromAsset(outputNode.outputTexture);
	}
}