using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using GraphProcessor;
using System.Collections.Generic;

namespace Mixture
{
	[NodeCustomEditor(typeof(MaterialBinder))]
	public class MaterialBinderView : MixtureNodeView
	{
        public override void Enable()
		{
			base.Enable();

            var materialPicker = controlsContainer.Q(null, "unity-object-field") as ObjectField;

			materialPicker.RegisterValueChangedCallback(e => {
				Debug.Log("Update !");
				ForceUpdatePorts();
			});
        }
    }
}
