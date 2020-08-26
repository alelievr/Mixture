using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using GraphProcessor;

namespace Mixture
{
	[NodeCustomEditor(typeof(CustomTextureNode))]
	public class CustomTextureNodeView : MixtureNodeView
	{
		Editor				customTextureEditor;
		CustomTextureNode	node;
		
		public override void OnCreated()
		{
			if (node.customTexture != null)
			{
				owner.graph.AddObjectToGraph(node.customTexture);
				owner.graph.AddObjectToGraph(node.updateMaterial);
				owner.graph.AddObjectToGraph(node.initializationMaterial);
			}
		}

		public override void Enable(bool fromInspector)
		{
			base.Enable(fromInspector);

			node = nodeTarget as CustomTextureNode;

			// Create your fields using node's variables and add them to the controlsContainer

			Editor.CreateCachedEditor(node.customTexture, null, ref customTextureEditor);

			// TODO: store the CRT and the materials into the asset so we're saved

			AddShaderFields();

			controlsContainer.Add(new IMGUIContainer(CustomTextureEditor));
		}

		void AddShaderFields()
		{
			ObjectField initializationShader = new ObjectField("Initialization")
			{
				value = node.initializationMaterial.shader,
				objectType = typeof(Shader),
			};

			initializationShader.RegisterValueChangedCallback((v) => {
				owner.RegisterCompleteObjectUndo("Updated Shader of ShaderNode");
				node.initializationMaterial.shader = (Shader)v.newValue;

				// TODO: factorize the code
				// UpdateShaderCreationUI();

				// We fore the update of node ports
				ForceUpdatePorts();
			});

			controlsContainer.Add(new Button(OpenCurrentShader){
				text = "Open"
			});

			void OpenCurrentShader()
			{
				AssetDatabase.OpenAsset(node.updateMaterial.shader);
			}

			ObjectField shaderField = new ObjectField("Update")
			{
				value = node.updateMaterial.shader,
				objectType = typeof(Shader),
			};

			shaderField.RegisterValueChangedCallback((v) => {
				owner.RegisterCompleteObjectUndo("Updated Shader of ShaderNode");
				node.updateMaterial.shader = (Shader)v.newValue;

				// TODO: factorize the code
				// UpdateShaderCreationUI();

				// We fore the update of node ports
				ForceUpdatePorts();
			});

			controlsContainer.Add(initializationShader);
			controlsContainer.Add(shaderField);
		}

		void CustomTextureEditor()
		{
			EditorGUILayout.Space();

			EditorGUIUtility.labelWidth = 180;
			customTextureEditor.OnInspectorGUI();

			// Because of animation bugs in ImGUI context we're forced to repaint in loop
			MarkDirtyRepaint();
		}

		public override void OnRemoved()
		{
			owner.graph.RemoveObjectFromGraph(node.customTexture);
			owner.graph.RemoveObjectFromGraph(node.updateMaterial);
			owner.graph.RemoveObjectFromGraph(node.initializationMaterial);
		}
	}
}