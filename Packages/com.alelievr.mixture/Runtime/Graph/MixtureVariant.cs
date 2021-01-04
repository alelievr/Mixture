using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

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

        // Important: note that order is not guaranteed 
        [SerializeField]
        List<Texture>   _outputTextures = null;
        public List<Texture>   outputTextures
        {
            get
            {
#if UNITY_EDITOR
                if (_outputTextures == null || _outputTextures.Count == 0)
                    _outputTextures = AssetDatabase.LoadAllAssetsAtPath(mainAssetPath).OfType<Texture>().ToList();
#endif
                _outputTextures.RemoveAll(t => t == null);

				return _outputTextures;
			}
        }

		[SerializeField]
		Texture					_mainOutputTexture;
		public Texture			mainOutputTexture
		{
			get
			{
#if UNITY_EDITOR
				if (_mainOutputTexture == null)
					_mainOutputTexture = AssetDatabase.LoadAssetAtPath< Texture >(mainAssetPath);
#endif
				return _mainOutputTexture;
			}
			set
            {
                outputTextures.Remove(_mainOutputTexture);
                outputTextures.Add(value);
                _mainOutputTexture = value;
            }
		}

		public string			mainAssetPath
		{
			get
			{
#if UNITY_EDITOR
                return AssetDatabase.GetAssetPath(this);
#else
                return null;
#endif
			}
		}

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
