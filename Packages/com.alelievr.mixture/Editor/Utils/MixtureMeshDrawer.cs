using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;
using GraphProcessor;

namespace Mixture
{
    [FieldDrawer(typeof(MixtureMesh))]
    public class MixtureMeshDrawer : VisualElement, INotifyValueChanged<MixtureMesh>
    {
        public MixtureMesh mesh;

        public ObjectField meshField;

        public MixtureMeshDrawer() : this(null) {}

        public MixtureMeshDrawer(string label) 
        {
            meshField = new ObjectField(label)
            {
                allowSceneObjects = false,
                objectType = typeof(Mesh),
            };

            // When mesh is changed, we propagate the event to the INotifyValueChanged interface
            meshField.RegisterValueChangedCallback(e => {
                if (e.newValue != mesh?.mesh)
                    value = new MixtureMesh(e.newValue as Mesh);
            });

            Add(meshField);
        }

        public MixtureMesh value
        {
            get => mesh;
            set
            {
                if (panel != null)
                {
                    using (var evt = ChangeEvent<MixtureMesh>.GetPooled(mesh, value))
                    {
                        evt.target = this;
                        SetValueWithoutNotify(value);
                        SendEvent(evt);
                    }
                }
                else
                {
                    SetValueWithoutNotify(value);
                }
            }
        }

        public void SetValueWithoutNotify(MixtureMesh newValue)
        {
            meshField.SetValueWithoutNotify(newValue?.mesh);
            mesh = newValue;
        }
    }
}