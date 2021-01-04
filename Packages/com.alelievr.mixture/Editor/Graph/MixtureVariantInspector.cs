using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.Collections.Generic;
using GraphProcessor;
using System.Linq;
using UnityEditor.UIElements;

namespace Mixture
{
   	// By default textures don't have any CustomEditors so we can define them for Mixture
	[CustomEditor(typeof(MixtureVariant), false)]
	class MixtureVariantInspector : MixtureEditor
	{
        MixtureVariant variant;

        [SerializeField, SerializeReference]
        List<ExposedParameter> visibleParameters = new List<ExposedParameter>();

        Dictionary<ExposedParameter, VisualElement> parameterViews = new Dictionary<ExposedParameter, VisualElement>();
        VisualTreeAsset overrideParameterView;

		protected override void OnEnable()
		{
            variant = target as MixtureVariant;
            graph = variant.parentGraph;
            if (graph != null)
			{
				graph.onExposedParameterListChanged += UpdateParameters;
				graph.onExposedParameterModified += UpdateParameters;
				graph.onExposedParameterValueChanged += UpdateParameters;
			}

            // Update serialized parameters (for the inspector)
            SyncParameters();
            exposedParameterFactory = new ExposedParameterFieldFactory(graph, visibleParameters);

            overrideParameterView = Resources.Load<VisualTreeAsset>("UI Blocks/MixtureVariantParameter");

			LoadInspectorFor(graph.mainOutputTexture.GetType(), new Object[]{ graph.mainOutputTexture });
		}

		protected override void OnDisable()
        {
            if (graph != null)
			{
				graph.onExposedParameterListChanged += UpdateParameters;
				graph.onExposedParameterModified += UpdateParameters;
				graph.onExposedParameterValueChanged += UpdateParameters;
			}
            exposedParameterFactory.Dispose();

            if (defaultTextureEditor != null)
				DestroyImmediate(defaultTextureEditor);
        }


		public override VisualElement CreateInspectorGUI()
        {
			if (graph == null)
				return base.CreateInspectorGUI();

			CreateRootElement();
            UpdateParentView();
			UpdateParameters();

            return root;
        }

        void UpdateParentView()
        {
            // var headerLabel = new Label("Parent");
            // headerLabel.AddToClassList("Header");
            var container = new VisualElement();
            // container.AddToClassList("Indent");

            var parentField = new ObjectField("Parent") { value = (Object)variant.parentVariant ?? variant.parentGraph};
            parentField.SetEnabled(false);
            container.Add(parentField);

            // root.Add(headerLabel);
            root.Add(container);
        }

        void SyncParameters()
        {
            visibleParameters.Clear();

            foreach (var graphParam in graph.exposedParameters)
            {
                ExposedParameter param = graphParam.Clone();
                int index = variant.overrideParameters.FindIndex(p => p == graphParam);

                // If the parameter has an override
                if (index != -1)
                {
                    if (param.GetValueType().IsAssignableFrom(variant.overrideParameters[index].GetValueType()))
                        param.value = variant.overrideParameters[index].value;
                }

                visibleParameters.Add(param);
            }
        }

		void UpdateParameters(ExposedParameter param) => UpdateParameters();
		void UpdateParameters()
		{
			if (parameters == null || !root.Contains(parameters))
			{
				parameters = new VisualElement();
				root.Add(parameters);
			}

            SyncParameters();
            var serializedInspector = new SerializedObject(this);

			parameters.Clear();
			bool header = true;
			bool showUpdateButton = false;

            foreach (var param in visibleParameters)
            {
                if (param.settings.isHidden)
                    continue;

				if (header)
				{
					var headerLabel = new Label("Exposed Parameters");
					headerLabel.AddToClassList("Header");
					parameters.Add(headerLabel);
					header = false;
					showUpdateButton = true;
				}

                var field = CreateParameterVariantView(param, serializedInspector);
                parameters.Add(field);
            }

			if (showUpdateButton)
			{
				var updateButton = new Button(() => {
					MixtureGraphProcessor.RunOnce(graph);
					graph.SaveAllTextures(false);
				}) { text = "Update Texture(s)" };
				updateButton.AddToClassList("Indent");
				parameters.Add(updateButton);
			}
		}

        VisualElement CreateParameterVariantView(ExposedParameter param, SerializedObject serializedInspector)
        {
            VisualElement prop = new VisualElement();
            prop.AddToClassList("Indent");
            prop.style.display = DisplayStyle.Flex;
            var parameterView = overrideParameterView.CloneTree();
            prop.Add(parameterView);

            var parameterValueField = exposedParameterFactory.GetParameterValueField(param, (newValue) => {
                param.value = newValue;
                UpdateOverrideParameter(param, newValue);
            });

            parameterValueField.AddManipulator(new ContextualMenuManipulator(builder => {
                builder.menu.AppendAction("Reset", _ => RemoveOverride(param));
            }));

            parameterValueField.Bind(serializedInspector);
            var paramContainer = parameterView.Q("Parameter");
            paramContainer.Add(parameterValueField);

            parameterViews[param] = parameterView;

            if (variant.overrideParameters.Contains(param))
                AddOverrideClass(parameterView);

            return prop;
        }

        void RemoveOverride(ExposedParameter parameter)
        {
            Undo.RegisterCompleteObjectUndo(variant, "Reset parameter");
            var graphParam = graph.exposedParameters.FirstOrDefault(p => p == parameter);
            parameter.value = graphParam.value;
            variant.overrideParameters.RemoveAll(p => p == parameter);
            exposedParameterFactory.ResetOldParameter(parameter);

            if (parameterViews.TryGetValue(parameter, out var view))
            {
                view.RemoveFromClassList("Override");
            }
        }

        void UpdateOverrideParameter(ExposedParameter parameter, object overrideValue)
        {
            Debug.Log(variant.overrideParameters.Count);
            if (!variant.overrideParameters.Contains(parameter))
            {
                Undo.RegisterCompleteObjectUndo(variant, "Override Parameter");
                variant.overrideParameters.Add(parameter);
                EditorUtility.SetDirty(variant);
            }
            else
            {
                Undo.RegisterCompleteObjectUndo(variant, "Override Parameter");
                int index = variant.overrideParameters.FindIndex(p => p == parameter);
                variant.overrideParameters[index].value = parameter.value;
                EditorUtility.SetDirty(variant);
            }

            // Enable the override overlay:
            if (parameterViews.TryGetValue(parameter, out var view))
                AddOverrideClass(view);
        }

        void AddOverrideClass(VisualElement view)
        {
            view.AddToClassList("Override");
        }
	}
}