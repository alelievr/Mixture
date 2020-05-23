using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Collections.Generic;
using GraphProcessor;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEditor.Rendering;
using System;

namespace Mixture
{
	[NodeCustomEditor(typeof(ComputeShaderNode))]
	public class ComputeShaderNodeView : MixtureNodeView
	{
		VisualElement		shaderCreationUI;
		ComputeShaderNode	computeShaderNode => nodeTarget as ComputeShaderNode;

		ObjectField			debugCustomRenderTextureField;
		ObjectField			shaderField;

		int					materialCRC;

		public override void Enable()
		{
			base.Enable();

			shaderField = new ObjectField
			{
				value = computeShaderNode.computeShader,
				objectType = typeof(ComputeShader),
			};

			shaderField.RegisterValueChangedCallback((v) => {
				SetComputeShader((ComputeShader)v.newValue);
			});
			
			InitializeDebug();

			controlsContainer.Add(shaderField);

			shaderCreationUI = new VisualElement();
			controlsContainer.Add(shaderCreationUI);
			UpdateShaderCreationUI();

			if (computeShaderNode.computeShader != null)
				UpdateComputeShaderData(computeShaderNode.computeShader);
		}

		void InitializeDebug()
		{
			// computeShaderNode.onProcessed += () => {
			// 	debugCustomRenderTextureField.value = computeShaderNode.output;
			// };

			// debugCustomRenderTextureField = new ObjectField("Output")
			// {
			// 	value = computeShaderNode.output
			// };
			
			debugContainer.Add(debugCustomRenderTextureField);
		}

		void UpdateShaderCreationUI()
		{
			shaderCreationUI.Clear();

			if (computeShaderNode.computeShader == null)
			{
				shaderCreationUI.Add(new Button(CreateNewComputeShader) {
					text = "New Compute Shader"
				});
			}
			else
			{
				shaderCreationUI.Add(new Button(OpenCurrentComputeShader){
					text = "Open"
				});

				shaderCreationUI.Add(new PopupField<string>("Kernel Name", computeShaderNode.kernelNames, computeShaderNode.kernelNames[computeShaderNode.kernelIndex]));
			}

			void CreateNewComputeShader()
			{
				SetComputeShader(MixtureEditorUtils.CreateComputeShader(title));
			}

			void OpenCurrentComputeShader()
			{
				AssetDatabase.OpenAsset(computeShaderNode.computeShader);
			}
		}

		void SetComputeShader(ComputeShader newShader)
		{
			owner.RegisterCompleteObjectUndo("Updated Shader of Compute Shader Node");
			computeShaderNode.computeShader = newShader;
			shaderField.value = newShader;
			UpdateShaderCreationUI();

			title = newShader?.name ?? "Null Compute";

			if (newShader != null)
				UpdateComputeShaderData(newShader);

			// We fore the update of node ports
			ForceUpdatePorts();
		}

		static Regex kernelRegex = new Regex(@"^\s*#pragma\s+kernel\s+(\w+)", RegexOptions.Multiline);
		static Regex readWriteObjects = new Regex(@"^\s*(RW|Append)(\w+)(?:<(\w+)>)?\s+(\w+)\s*;", RegexOptions.Multiline);
		static Regex readOnlyObjects = new Regex(@"^\s*(Consume|)(\w+)(?:<(\w+)>)?\s+(\w+)\s*;", RegexOptions.Multiline);

		void UpdateComputeShaderData(ComputeShader shader)
		{
			if (ShaderUtil.GetComputeShaderMessages(shader).Any(m => m.severity == ShaderCompilerMessageSeverity.Error))
			{
				Debug.LogError("Compute Shader " + shader + " has errors");
				return;
			}

			var path = AssetDatabase.GetAssetPath(shader);
			string fileContent = File.ReadAllText(path);

			// First remove all the functions from the text
			int functionMarkLocation = fileContent.IndexOf("{");
			fileContent = fileContent.Substring(0, functionMarkLocation).Trim();

			// Fill kernel names
			computeShaderNode.kernelNames.Clear();
			foreach (Match match in kernelRegex.Matches(fileContent))
				computeShaderNode.kernelNames.Add(match.Groups[1].Value);
			
			// Fill output properties name
			computeShaderNode.computeOutputs.Clear();
			foreach (Match match in readWriteObjects.Matches(fileContent))
			{
				if (match.Groups[4].Value.StartsWith("__"))
					continue;

				Debug.Log("Output: " + match.Groups[4].Value);

				computeShaderNode.computeOutputs.Add(new ComputeShaderNode.ComputeParameter{
					name = match.Groups[4].Value,
					specificType = match.Groups[3].Value,
					type = ComputeShaderTypeToCSharp(match.Groups[2].Value),
				});
			}
			// Remove output properties
			fileContent = readWriteObjects.Replace(fileContent, "");

			// We can then select input properties
			computeShaderNode.computeInputs.Clear();
			foreach (Match match in readOnlyObjects.Matches(fileContent))
			{
				if (match.Groups[4].Value.StartsWith("__"))
					continue;
				
				Debug.Log("Input: " + match.Groups[4].Value);

				computeShaderNode.computeInputs.Add(new ComputeShaderNode.ComputeParameter{
					name = match.Groups[4].Value,
					specificType = match.Groups[3].Value,
					type = ComputeShaderTypeToCSharp(match.Groups[2].Value),
				});
			}
		}
		
		Type ComputeShaderTypeToCSharp(string computeShaderType)
		{
			switch (computeShaderType)
			{
				case "Texture2D": return typeof(Texture2D);
				case "Texture2DArray": return typeof(Texture2DArray);
				case "Texture3D": return typeof(Texture3D);
				case "TextureCube": return typeof(Cubemap);
				case "TextureCubeArray": return typeof(CubemapArray);
				case "Buffer":
				case "ByteAddressBuffer":
				case "StructuredBuffer": return typeof(ComputeBuffer);
				default: throw new Exception("Unknown compute shader type " + computeShaderType);
			}
		}

		public override void OnRemoved()
		{
			// TODO: check if compute shader is embbeded and remove it
			// owner.graph.RemoveObjectFromGraph(computeShaderNode.material);
		}
	}
}