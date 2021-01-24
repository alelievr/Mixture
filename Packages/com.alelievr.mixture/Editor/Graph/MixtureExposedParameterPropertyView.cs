using UnityEngine.UIElements;
using GraphProcessor;

namespace Mixture
{
	public class MixtureExposedParameterPropertyView : VisualElement
	{
		protected MixtureGraphView mixtureGraphView;

		public ExposedParameter parameter { get; private set; }

		public Toggle     hideInInspector { get; private set; }

		public MixtureExposedParameterPropertyView(BaseGraphView graphView, ExposedParameter param)
		{
			mixtureGraphView = graphView as MixtureGraphView;
			parameter      = param;

			var valueField = graphView.exposedParameterFactory.GetParameterValueField(param, (newValue) => {
				graphView.RegisterCompleteObjectUndo("Updated Parameter Value");
				param.value = newValue;
				graphView.graph.NotifyExposedParameterValueChanged(param);
				mixtureGraphView.ProcessGraph();
			});

			var field = graphView.exposedParameterFactory.GetParameterSettingsField(param, (newValue) => {
				param.settings = newValue as ExposedParameter.Settings;
			});

			Add(valueField);

			Add(field);
		}
	}
} 