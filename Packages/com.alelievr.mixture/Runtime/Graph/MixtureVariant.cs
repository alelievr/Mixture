using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System;
using System.Linq;
using Object = UnityEngine.Object;
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

        [SerializeReference, SerializeField]
        List<ExposedParameter> parametersStateAtLastUpdate = new List<ExposedParameter>();

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

        public event Action<ExposedParameter> parameterValueChanged;

        internal event Action variantTexturesUpdated;

        public void SetParent(MixtureGraph graph)
        {
            parentVariant = null;
            parentGraph = graph;
            depth = 0;
            if (!graph.variants.Contains(this))
                graph.variants.Add(this);
        }

        public void SetParent(MixtureVariant variant)
        {
            parentGraph = variant.parentGraph;
            parentVariant = variant;
            depth = variant.depth + 1;
            if (!parentGraph.variants.Contains(this))
                parentGraph.variants.Add(this);
        }

        public bool SetParameterValue(string name, object value)
        {
            var param = parentGraph.GetExposedParameter(name);

            if (param == null)
                return false;
            
            param = param.Clone();
            param.value = value;

            int index = overrideParameters.FindIndex(p => p == param);
            if (index == -1)
                overrideParameters.Add(param);
            else
                overrideParameters[index].value = value;

            return true;
        }

        public IEnumerable<ExposedParameter> GetAllOverrideParameters()
        {
            static IEnumerable<ExposedParameter> GetOverrideParamsForVariant(MixtureVariant variant)
            {
                if (variant.parentVariant != null)
                {
                    foreach (var param in GetOverrideParamsForVariant(variant.parentVariant))
                        yield return param;
                }

                foreach (var param in variant.overrideParameters)
                    yield return param;
            }

            return GetOverrideParamsForVariant(this);
        }

        public IEnumerable<ExposedParameter> GetAllParameters()
        {
            HashSet<ExposedParameter> parameters = new HashSet<ExposedParameter>();

            MixtureVariant variant = this;
            while (variant != null)
            {
                foreach (var param in variant.overrideParameters)
                    parameters.Add(param);
                variant = variant.parentVariant;
            }

            foreach (var param in parentGraph.exposedParameters)
                parameters.Add(param);

            return parameters;
        }

        public object GetDefaultParameterValue(ExposedParameter parameter)
        {
            foreach (var param in GetAllOverrideParameters())
                if (param == parameter)
                    return param.value;
            return parameter.value;
        }

        public void NotifyOverrideValueChanged(ExposedParameter parameter) => parameterValueChanged?.Invoke(parameter);

        public bool IsDirty()
        {
            var currentParams = GetAllParameters().ToList();

            if (currentParams.Count != parametersStateAtLastUpdate.Count)
                return true;

            for (int i = 0; i < parametersStateAtLastUpdate.Count; i++)
            {
                if (!currentParams[i].value.Equals(parametersStateAtLastUpdate[i].value))
                    return true;
            }

            return false;
        }

        public bool ContainsInParents(MixtureVariant variant)
        {
            MixtureVariant parent = parentVariant;

            while (parent != null)
            {
                if (parent == variant)
                    return true;
                parent = parent.parentVariant;
            }
            return false;
        }

        public IEnumerable<MixtureVariant> GetChildVariants()
        {
            foreach (var variant in parentGraph.variants)
            {
                if (variant == this)
                    continue;

                if (variant.ContainsInParents(this))
                    yield return variant;
            }
        }

        Texture FindOutputTexture(string name, bool isMain)
            => outputTextures.Find(t => t != null && (isMain ? mainOutputTexture != null && t.name == mainOutputTexture.name : t.name == name));

        public void UpdateAllVariantTextures()
        {
            ProcessGraphWithOverrides();

            if (parentGraph.type == MixtureGraphType.Realtime)
            {
                // Copy the result into the variant
                foreach (var output in parentGraph.outputNode.outputTextureSettings)
                {
                    var currentTexture = FindOutputTexture(output.name, output.isMain);
                    TextureUtils.CopyTexture(output.finalCopyRT, currentTexture);
                }
            }
#if UNITY_EDITOR
            else
            {
                // Readback the result render textures into the variant:
                foreach (var output in parentGraph.outputNode.outputTextureSettings)
                {
                    var currentTexture = FindOutputTexture(output.name, output.isMain);
                    var format = output.enableConversion ? (TextureFormat)output.conversionFormat : output.compressionFormat;
                    parentGraph.ReadBackTexture(parentGraph.outputNode, output.finalCopyRT, output.IsCompressionEnabled() || output.IsConversionEnabled(), format, output.compressionQuality, currentTexture);
                }
            }
#endif

            parametersStateAtLastUpdate = GetAllParameters().Select(p => p.Clone()).ToList();
            variantTexturesUpdated?.Invoke();
        }

        public void ProcessGraphWithOverrides()
        {
            // override graph parameter values:
            var graphParamsValues = parentGraph.exposedParameters.Select(p => p.value).ToList();

            foreach (var param in GetAllOverrideParameters())
            {
                var graphParam = parentGraph.exposedParameters.FirstOrDefault(p => p == param);
                graphParam.value = param.value;
            }

            MixtureGraphProcessor.RunOnce(parentGraph);

            // Set back the original params
            for (int i = 0; i < parentGraph.exposedParameters.Count; i++)
                parentGraph.exposedParameters[i].value = graphParamsValues[i];
        }

#if UNITY_EDITOR
        public void CopyTexturesFromGraph(bool copyTextureContent = true)
        {
            var subAssets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(parentGraph));
            CopyTextureFromAssets(subAssets, copyTextureContent);
        }

        public void CopyTexturesFromParentVariant(bool copyTextureContent = true)
        {
            var subAssets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(parentVariant));
            CopyTextureFromAssets(subAssets, copyTextureContent);
        }

        void CopyTextureFromAssets(Object[] assets, bool copyTextureContent)
        {
            var oldTextures = outputTextures.ToList();
            var oldMainTexture = mainOutputTexture;

            // Duplicate all other mixture outputs
            var textures = new List<Texture>();
            Texture mainTexture = null;
            foreach (var asset in assets)
            {
                if ((asset.hideFlags & (HideFlags.HideInHierarchy | HideFlags.HideInInspector)) != 0)
                    continue;

                if (asset is Texture texture)
                {
                    bool isMain = texture.name == parentGraph.mainOutputTexture.name || (parentVariant != null && texture.name == parentVariant.mainOutputTexture.name);
                    var t = TextureUtils.DuplicateTexture(texture, copyTextureContent);
                    var oldTexture = FindOutputTexture(t.name, isMain);

                    // If the new texture is using the same type we can swap it's data instead of replacing
                    // the asset on the disk (so we can keep object selected / inspector locked on the mixture).
                    if (oldTexture != null && t.GetType() == oldTexture.GetType())
                    {
                        EditorUtility.CopySerialized(t, oldTexture);
                        oldTextures.Remove(oldTexture);
                        DestroyImmediate(t);
                        if (isMain)
                            mainTexture = oldTexture;
                    }
                    else
                    {
                        AssetDatabase.AddObjectToAsset(t, mainAssetPath);
                        textures.Add(t);
                        if (isMain)
                            mainTexture = t;
                    }
                }
            }

            // Set main output texture
            AssetDatabase.SetMainObject(mainTexture, mainAssetPath);

            foreach (var texture in oldTextures)
            {
                AssetDatabase.RemoveObjectFromAsset(texture);
                DestroyImmediate(texture, true);
            }

            _outputTextures = textures;
            _mainOutputTexture = mainTexture;

            parametersStateAtLastUpdate = GetAllParameters().Select(p => p.Clone()).ToList();
            variantTexturesUpdated?.Invoke();
        }
#endif
    }
}
