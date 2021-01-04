using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System;

namespace Mixture
{
    public class MixtureVariant : ScriptableObject 
    {
        public MixtureGraph parentGraph;
        public MixtureVariant parentVariant;

        [SerializeField]
        int depth = 0;

        [SerializeReference]
        public List<ExposedParameter> overrideParameters = new List<ExposedParameter>();

        public void SetParent(MixtureGraph graph)
        {
            parentVariant = null;
            parentGraph = graph;
            depth = 0;
        }

        public void SetParent(MixtureVariant variant)
        {
            parentGraph = variant.parentGraph;
            parentVariant = variant;
            depth = variant.depth + 1;
        }
    }
}
