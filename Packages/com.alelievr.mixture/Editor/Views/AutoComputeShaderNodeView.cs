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
	[NodeCustomEditor(typeof(AutoComputeShaderNode))]
	class AutoComputeShaderNodeView : ComputeShaderNodeView
	{
		AutoComputeShaderNode	computeShaderNode => nodeTarget as AutoComputeShaderNode;

		List<(VisualElement shaderCreationUI, ObjectField shaderField)> uiList = new List<(VisualElement, ObjectField)>();

		DateTime			lastModified;

		VisualElement		allocList;

		public override void Enable(bool fromInspector)
		{
			base.Enable(fromInspector);

            var shaderCreationUI = new VisualElement();
            controlsContainer.Add(shaderCreationUI);
			var shaderField = new ObjectField("Compute Shader") { value = computeShaderNode.computeShader, objectType = typeof(ComputeShader) };
			uiList.Add((shaderCreationUI, shaderField));
			UpdateShaderCreationUI(shaderCreationUI, shaderField);

			controlsContainer.Add(shaderField);

			// If the user created a custom version of compute, he doesn't need the selector + UI
			shaderField.RegisterValueChangedCallback((v) =>
			{
				SetComputeShader((ComputeShader)v.newValue);
			});
		
			if (!fromInspector)
			{
				owner.graph.onOutputTextureUpdated += () => {
					foreach (var res in computeShaderNode.managedResources)
						if (res.allocatedTexture != null && res.textureAllocMode == AutoComputeShaderNode.TextureAllocMode.SameAsOutput)
							computeShaderNode.UpdateManagedResource(res);
				};

				if (computeShaderNode.computeShader != null)
				{
					UpdateComputeShaderData(computeShaderNode.computeShader);
					ForceUpdatePorts();
				}

				computeShaderChanged += () => {
					UpdateComputeShaderData(computeShaderNode.computeShader);
					RefreshPorts();
				};
			}
		}

		internal void SetComputeShader(ComputeShader newShader)
		{
			owner.RegisterCompleteObjectUndo("Updated Shader of Compute Shader Node");
			computeShaderNode.computeShader = newShader;
			foreach (var kp in uiList)
				if (kp.shaderField != null)
					kp.shaderField.value = newShader;
			computePath = AssetDatabase.GetAssetPath(newShader);

			string resourcePath = null;
			if (computePath.Contains("Resources/"))
				resourcePath = Path.ChangeExtension(computePath.Substring(computePath.LastIndexOf("Resources/") + 10), null);
			computeShaderNode.resourcePath = resourcePath;

			title = newShader?.name ?? "New Compute";

			if (newShader != null)
				UpdateComputeShaderData(newShader);

			foreach (var kp in uiList)
				UpdateShaderCreationUI(kp.shaderCreationUI, kp.shaderField);

			ForceUpdatePorts();

			computeShaderNode.ComputeIsValid();
		}

		protected override VisualElement CreateSettingsView()
		{
			var settings = base.CreateSettingsView();

			var allocResourceHeader = new Label("Auto Alloc Resources");
			allocResourceHeader.AddToClassList(MixtureSettingsView.headerStyleClass);
			settings.Add(allocResourceHeader);

			allocList = new VisualElement();
			settings.Add(allocList);

			UpdateAllocUI();

			return settings;
		}

		void UpdateShaderCreationUI(VisualElement shaderCreationUI, ObjectField shaderField)
		{
			shaderCreationUI.Clear();
			foreach (var openButtonUI in openButtonsUI)
				if (openButtonUI != null)
					openButtonUI.Clear();

			if (computeShaderNode.computeShader == null)
			{
				shaderCreationUI.Add(new Button(CreateNewComputeShader) {
					text = "New Compute Shader"
				});
			}
			else
			{
				foreach (var openButtonUI in openButtonsUI)
					AddOpenButton(openButtonUI);
			}

			void CreateNewComputeShader()
			{
				SetComputeShader(MixtureEditorUtils.CreateComputeShader(owner.graph, title));
			}
		}

		void UpdateAllocUI()
		{
			if (allocList == null)
				return;

			allocList.Clear();

			// Sync allocated resources struct in the node:
			foreach (var output in computeShaderNode.computeOutputs)
			{
				if (!computeShaderNode.managedResources.Any(r => r.propertyName == output.propertyName))
				{
					computeShaderNode.AddManagedResource(new AutoComputeShaderNode.ResourceDescriptor{
						propertyName = output.propertyName,
						autoAlloc = false,
						sType = output.sType,
					});
				}
			}
			foreach (var resource in computeShaderNode.managedResources)
			{
				if (!computeShaderNode.computeOutputs.Any(o => o.propertyName == resource.propertyName))
				{
					computeShaderNode.RemoveManagedResource(resource);
				}
			}

			foreach (var res in computeShaderNode.managedResources)
			{
				var customAlloc = new Toggle{ label = res.propertyName, value = res.autoAlloc };
				allocList.Add(customAlloc);

				customAlloc.RegisterValueChangedCallback((e) => {
					res.autoAlloc = e.newValue;
					UpdateComputeShaderData(computeShaderNode.computeShader);
					UpdateAllocUI();
					ForceUpdatePorts();
					NotifyNodeChanged();
				});

				// Select all RWTexture and display a simple alloc UI for this 
				if (typeof(Texture).IsAssignableFrom(res.sType.type))
				{
					// Texture alloc settings:
					if (res.autoAlloc)
					{
						var textureAllocSettings = new VisualElement();
						textureAllocSettings.style.paddingLeft = 15;

						var allocMode = new EnumField("Resolution", res.textureAllocMode) { value = res.textureAllocMode };
						textureAllocSettings.Add(allocMode);

						allocMode.RegisterValueChangedCallback(e => {
							owner.RegisterCompleteObjectUndo("Update Alloc Mode");
							res.textureAllocMode = (AutoComputeShaderNode.TextureAllocMode)e.newValue;
							computeShaderNode.UpdateManagedResource(res);
							NotifyNodeChanged();
						});

						allocList.Add(textureAllocSettings);
					}
				}
				else if (res.sType.type == typeof(ComputeBuffer))
				{
					if (res.autoAlloc)
					{
						var bufferAllocSettings = new VisualElement();
						bufferAllocSettings.style.paddingLeft = 15;

						var size = new IntegerField("Buffer Element Count") { value = res.bufferSize };
						var stride = new IntegerField("Element Stride") { value = res.bufferStride };
						bufferAllocSettings.Add(size);
						bufferAllocSettings.Add(stride);

						size.RegisterValueChangedCallback(e => {
							owner.RegisterCompleteObjectUndo("Update Buffer Element Count");
							res.bufferSize = e.newValue;
							computeShaderNode.UpdateManagedResource(res);
							NotifyNodeChanged();
						});
						stride.RegisterValueChangedCallback(e => {
							owner.RegisterCompleteObjectUndo("Update Element Stride");
							res.bufferSize = e.newValue;
							computeShaderNode.UpdateManagedResource(res);
							NotifyNodeChanged();
						});

						allocList.Add(bufferAllocSettings);
					}
				}
			}
		}

		internal void AutoAllocResource(string resourceName)
		{
			var desc = computeShaderNode.managedResources.FirstOrDefault(r => r.propertyName == resourceName);

			if (desc != null)
			{
				desc.autoAlloc = true;
				UpdateComputeShaderData(computeShaderNode.computeShader);
				UpdateAllocUI();
				ForceUpdatePorts();
				NotifyNodeChanged();
			}
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
				
				if (match.Groups[4].Value == computeShaderNode.previewTexturePropertyName)
					continue;

				computeShaderNode.computeOutputs.Add(new AutoComputeShaderNode.ComputeParameter{
					displayName = ObjectNames.NicifyVariableName(match.Groups[4].Value),
					propertyName = match.Groups[4].Value,
					specificType = match.Groups[3].Value,
					sType = new SerializableType(ComputeShaderTypeToCSharp(match.Groups[2].Value)),
				});
			}

			// We can then select input properties
			computeShaderNode.computeInputs.Clear();
			foreach (Match match in readOnlyObjects.Matches(fileContent))
			{
				var propertyName = match.Groups[4].Value;

				if (propertyName.StartsWith("__"))
					continue;

				if (propertyName == computeShaderNode.previewTexturePropertyName)
					continue;

				// If the resource is allocated by this node, we don't display it as input
				if (computeShaderNode.managedResources.Any(r => r.propertyName == propertyName && r.autoAlloc))
					continue;

				computeShaderNode.computeInputs.Add(new AutoComputeShaderNode.ComputeParameter{
					displayName = ObjectNames.NicifyVariableName(match.Groups[4].Value),
					propertyName = match.Groups[4].Value,
					specificType = match.Groups[3].Value,
					sType = new SerializableType(ComputeShaderTypeToCSharp(match.Groups[2].Value)),
				});
			}

			UpdateAllocUI();

			computeShaderNode.UpdateComputeShader();
		}
		
		Type ComputeShaderTypeToCSharp(string computeShaderType)
		{
			if (computeShaderType.StartsWith("RW"))
				computeShaderType = computeShaderType.Remove(0, 2);

			switch (computeShaderType)
			{
				case "bool": return typeof(bool);
				case "int": return typeof(int);
				case "float": return typeof(float);
				case "int2":
				case "float2": return typeof(Vector2);
				case "int3":
				case "float3": return typeof(Vector3);
				case "int4":
				case "float4": return typeof(Vector4);
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
	}
}