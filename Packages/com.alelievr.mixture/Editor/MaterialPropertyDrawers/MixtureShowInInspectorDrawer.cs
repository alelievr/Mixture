using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Mixture
{
    public class ShowInInspectorDecorator : MixturePropertyDrawer
    {
        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor) => 0;
    }
}