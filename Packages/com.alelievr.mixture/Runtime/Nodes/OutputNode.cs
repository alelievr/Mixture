using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System;
using System.Linq;
using UnityEngine.Rendering;
using UnityEngine.Profiling;

namespace Mixture
{
	[System.Serializable]
	public class OutputNode : MixtureNode, IUseCustomRenderTextureProcessing
	{
		[Input, SerializeField, HideInInspector]
		public List<OutputTextureSettings> outputTextureSettings = new List<OutputTextureSettings>();

		public OutputTextureSettings mainOutput => outputTextureSettings[0];

		public event Action			onTempRenderTextureUpdated;

		public override string		name => "Output Texture Asset";
		public override Texture 	previewTexture => graph.isRealtime ? graph.mainOutputTexture : outputTextureSettings.Count > 0 ? outputTextureSettings[0].finalCopyRT : null;
		public override float		nodeWidth => 350;

		// TODO: move this to NodeGraphProcessor
		[NonSerialized]
		protected HashSet< string > uniqueMessages = new HashSet< string >();

		protected override MixtureRTSettings defaultRTSettings
        {
            get => new MixtureRTSettings()
            {
                widthMode = OutputSizeMode.Fixed,
                heightMode = OutputSizeMode.Fixed,
                depthMode = OutputSizeMode.Fixed,
				outputChannels = OutputChannel.RGBA,
				outputPrecision = OutputPrecision.Half,
				potSize = POTSize._1024,
                editFlags = EditFlags.POTSize | EditFlags.Width | EditFlags.Height | EditFlags.Depth | EditFlags.Dimension | EditFlags.TargetFormat
            };
        }

        protected override void Enable()
        {
			// Sanitize the RT Settings for the output node, they must contains only valid information for the output node
			if (rtSettings.outputChannels == OutputChannel.SameAsOutput)
				rtSettings.outputChannels = OutputChannel.RGBA;
			if (rtSettings.outputPrecision == OutputPrecision.SameAsOutput)
				rtSettings.outputPrecision = OutputPrecision.Half;
			if (rtSettings.dimension == OutputDimension.SameAsOutput)
				rtSettings.dimension = OutputDimension.Texture2D;
			rtSettings.editFlags |= EditFlags.POTSize;

			// Checks that the output have always at least one element:
			if (outputTextureSettings.Count == 0)
				AddTextureOutput(OutputTextureSettings.Preset.Color);

			// Sanitize main texture value:
			if (outputTextureSettings.Count((o => o.isMain)) != 1)
			{
				outputTextureSettings.ForEach(o => o.isMain = false);
				outputTextureSettings.First().isMain = true;
			}
		}

		// Disable reset on output texture settings
		protected override bool CanResetPort(NodePort port) => false;

		// TODO: output texture setting presets when adding a new output

		public OutputTextureSettings AddTextureOutput(OutputTextureSettings.Preset preset)
		{
			var output = new OutputTextureSettings
			{
				inputTexture = null,
				name = $"Input {outputTextureSettings?.Count + 1}",
				finalCopyMaterial = CreateFinalCopyMaterial(),
			};

			if (graph.isRealtime)
				output.finalCopyRT = graph.mainOutputTexture as CustomRenderTexture;

			// output.finalCopyRT can be null here if the graph haven't been imported yet
			if (output.finalCopyRT != null)
				output.finalCopyRT.material = output.finalCopyMaterial;

			// Try to guess the correct setup for the user
#if UNITY_EDITOR
			var names = outputTextureSettings.Select(o => o.name).Concat(new List<string>{ graph.mainOutputTexture.name }).ToArray();
			output.SetupPreset(preset, (name) => UnityEditor.ObjectNames.GetUniqueName(names, name));
#endif

			// Output 0 is always Main Texture
			if (outputTextureSettings.Count == 0)
			{
				output.name = "Main Texture";
				output.isMain = true;
			}

			outputTextureSettings.Add(output);

#if UNITY_EDITOR
			if (graph.isRealtime)
				graph.UpdateRealtimeAssetsOnDisk();
#endif

			return output;
		}

		public void RemoveTextureOutput(OutputTextureSettings settings)
		{
			outputTextureSettings.Remove(settings);

#if UNITY_EDITOR
			// When the graph is realtime, we don't have the save all button, so we call is automatically
			if (graph.isRealtime)
				graph.UpdateRealtimeAssetsOnDisk();
#endif
		}

		Material CreateFinalCopyMaterial()
		{
			var finalCopyShader = Shader.Find("Hidden/Mixture/FinalCopy");

			if (finalCopyShader == null)
			{
				Debug.LogError("Can't find Hidden/Mixture/FinalCopy shader");
				return null;
			}

			return new Material(finalCopyShader){ hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector};
		}

        protected override void Disable()
		{
			base.Disable();

			foreach (var output in outputTextureSettings)
			{
				if (graph != null && !graph.isRealtime)
					CoreUtils.Destroy(output.finalCopyRT);
			}
		}

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			if (graph.mainOutputTexture == null)
			{
				Debug.LogError("Output Node can't write to target texture, Graph references a null output texture");
				return false;
			}

			UpdateMessages();

			foreach (var output in outputTextureSettings)
			{
				// Update the renderTexture reference for realtime graph
				if (graph.isRealtime)
				{
					var finalCopyRT = graph.FindOutputTexture(output.name, output.isMain) as CustomRenderTexture;
					if (finalCopyRT != null && output.finalCopyRT != finalCopyRT)
						onTempRenderTextureUpdated?.Invoke();
					output.finalCopyRT = finalCopyRT;

					// Only the main output CRT is marked as realtime because it's processing will automatically
					// trigger the processing of it's graph, and thus all the outputs in the graph.
					if (output.isMain)
						output.finalCopyRT.updateMode = CustomRenderTextureUpdateMode.Realtime;
					else
						output.finalCopyRT.updateMode = CustomRenderTextureUpdateMode.OnDemand;
					
					if (output.finalCopyRT.dimension != rtSettings.GetTextureDimension(graph))
					{
						output.finalCopyRT.Release();
						output.finalCopyRT.depth = 0;
						output.finalCopyRT.dimension = rtSettings.GetTextureDimension(graph);
						output.finalCopyRT.Create();
					}
				}
				else
				{
					// Update the renderTexture size and format:
					if (UpdateTempRenderTexture(ref output.finalCopyRT, output.hasMipMaps))
						onTempRenderTextureUpdated?.Invoke();
				}

				if (!UpdateFinalCopyMaterial(output))
					continue;

				bool inputHasMips = output.inputTexture != null && output.inputTexture.mipmapCount > 1;
				CustomTextureManager.crtExecInfo[output.finalCopyRT] = new CustomTextureManager.CustomTextureExecInfo{
					runOnAllMips = inputHasMips
				};

				// The CustomRenderTexture update will be triggered at the begining of the next frame so we wait one frame to generate the mipmaps
				// We need to do this because we can't generate custom mipMaps with CustomRenderTextures

				if (output.hasMipMaps && !inputHasMips)
				{
					// TODO: check if input has mips and copy them instead of overwritting them.
					cmd.GenerateMips(output.finalCopyRT);
				}
			}

			return true;
		}

		void UpdateMessages()
		{
			if (inputPorts.All(p => p?.GetEdges()?.Count == 0))
			{
				if (uniqueMessages.Add("OutputNotConnected"))
					AddMessage("Output node input is not connected", NodeMessageType.Warning);
			}
			else
			{
				uniqueMessages.Clear();
				ClearMessages();
			}
		}

		bool UpdateFinalCopyMaterial(OutputTextureSettings targetOutput)
		{
			if (targetOutput.finalCopyMaterial == null)
			{
				targetOutput.finalCopyMaterial = CreateFinalCopyMaterial();
				if (!graph.IsObjectInGraph(targetOutput.finalCopyMaterial))
					graph.AddObjectToGraph(targetOutput.finalCopyMaterial);
			}

			// Manually reset all texture inputs
			ResetMaterialPropertyToDefault(targetOutput.finalCopyMaterial, "_Source_2D");
			ResetMaterialPropertyToDefault(targetOutput.finalCopyMaterial, "_Source_3D");
			ResetMaterialPropertyToDefault(targetOutput.finalCopyMaterial, "_Source_Cube");

			var input = targetOutput.inputTexture;
			if (input != null)
			{
				if (input.dimension != (TextureDimension)rtSettings.dimension)
				{
					Debug.LogError("Error: Expected texture type input for the OutputNode is " + graph.mainOutputTexture.dimension + " but " + input?.dimension + " was provided");
					return false;
				}

				MixtureUtils.SetupDimensionKeyword(targetOutput.finalCopyMaterial, input.dimension);

				if (input.dimension == TextureDimension.Tex2D)
					targetOutput.finalCopyMaterial.SetTexture("_Source_2D", input);
				else if (input.dimension == TextureDimension.Tex3D)
					targetOutput.finalCopyMaterial.SetTexture("_Source_3D", input);
				else
					targetOutput.finalCopyMaterial.SetTexture("_Source_Cube", input);

				targetOutput.finalCopyMaterial.SetInt("_IsSRGB", targetOutput.sRGB ? 1 : 0);
			}

			if (targetOutput.finalCopyRT != null)
				targetOutput.finalCopyRT.material = targetOutput.finalCopyMaterial;

			return true;
		}

		[CustomPortBehavior(nameof(outputTextureSettings))]
		protected IEnumerable< PortData > ChangeOutputPortType(List< SerializableEdge > edges)
		{
			TextureDimension dim = (GetType() == typeof(ExternalOutputNode)) ? rtSettings.GetTextureDimension(graph) : (TextureDimension)rtSettings.dimension;
			Type displayType = TextureUtils.GetTypeFromDimension(dim);

			foreach (var output in outputTextureSettings)
			{
				yield return new PortData{
					displayName = "", // display name is handled by the port settings UI element
					displayType = displayType,
					identifier = output.name,
				};
			}
		}

		[CustomPortInput(nameof(outputTextureSettings), typeof(Texture))]
		protected void AssignSubTextures(List< SerializableEdge > edges)
		{
			foreach (var edge in edges)
			{
				// Find the correct output texture:
				var output = outputTextureSettings.Find(o => o.name == edge.inputPort.portData.identifier);

				if (output != null)
				{
					output.inputTexture = edge.passThroughBuffer as Texture;
				}
			}
		}

        public IEnumerable<CustomRenderTexture> GetCustomRenderTextures()
        {
			foreach (var output in outputTextureSettings)
				yield return output.finalCopyRT;
        }
    }
}