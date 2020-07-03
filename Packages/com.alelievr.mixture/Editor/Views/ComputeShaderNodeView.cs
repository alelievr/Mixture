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
		protected VisualElement		openButtonUI;
		ComputeShaderNode	computeShaderNode => nodeTarget as ComputeShaderNode;

		ObjectField			debugCustomRenderTextureField = null;

		DateTime			lastModified;
		[NonSerialized]
		protected string	computePath = null;

		bool				isDerived;
		VisualElement		allocList;

        protected event Action computeShaderChanged;

		public override void Enable(bool fromInspector)
		{
			base.Enable(fromInspector);

            openButtonUI = new VisualElement();
            controlsContainer.Add(openButtonUI);
        
            AddOpenButton();

			InitializeDebug();

			if (computePath == null)
				computePath = AssetDatabase.GetAssetPath(computeShaderNode.computeShader);
			if (!String.IsNullOrEmpty(computePath))
				lastModified = File.GetLastWriteTime(computePath);
			var detector = schedule.Execute(DetectComputeShaderChanges);
			detector.Every(200);
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

		protected void AddOpenButton()
		{
			openButtonUI.Clear();

			if (computeShaderNode.showOpenButton && computeShaderNode.computeShader != null)
			{
				openButtonUI.Add(new Button(OpenCurrentComputeShader){
					text = "Open"
				});
			}

			void OpenCurrentComputeShader()
			{
				AssetDatabase.OpenAsset(computeShaderNode.computeShader);
			}
		}

		void DetectComputeShaderChanges()
		{
			if (computeShaderNode.computeShader == null)
				return;

			if (computePath == null)
				computePath = AssetDatabase.GetAssetPath(computeShaderNode.computeShader);
			
			var modificationDate = File.GetLastWriteTime(computePath);

			if (lastModified != modificationDate)
			{
				schedule.Execute(() => {
					// Reimport the compute shader:
					AssetDatabase.ImportAsset(computePath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive | ImportAssetOptions.DontDownloadFromCacheServer);

                    computeShaderChanged?.Invoke();

					NotifyNodeChanged();

					// ShaderUtil.GetComputeShaderMessages returns nothing if we call it without delay ...
					schedule.Execute(() => computeShaderNode.ComputeIsValid()).ExecuteLater(500);
				}).ExecuteLater(100);
			}
			lastModified = modificationDate;
		}

	}
}