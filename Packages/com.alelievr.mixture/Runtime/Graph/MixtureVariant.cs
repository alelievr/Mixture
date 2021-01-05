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

        public void NotifyOverrideValueChanged(ExposedParameter parameter) => parameterValueChanged?.Invoke(parameter);

#if UNITY_EDITOR
        Texture FindOutputTexture(string name, bool isMain)
            => outputTextures.Find(t => t != null && (isMain ? t.name == mainOutputTexture.name : t.name == name));

        public void UpdateAllVariantTextures()
        {
            // override graph parameter values:
            var graphParamsValues = parentGraph.exposedParameters.Select(p => p.value).ToList();

            foreach (var param in GetAllOverrideParameters())
            {
                var graphParam = parentGraph.exposedParameters.FirstOrDefault(p => p == param);
                graphParam.value = param.value;
            }

            MixtureGraphProcessor.RunOnce(parentGraph);

            if (parentGraph.isRealtime)
            {
                // Copy the result into the variant
                foreach (var output in parentGraph.outputNode.outputTextureSettings)
                {
                    var currentTexture = FindOutputTexture(output.name, output.isMain);
                    for (int slice = 0; slice < TextureUtils.GetSliceCount(output.finalCopyRT); slice++)
                        for (int mipLevel = 0; mipLevel < output.finalCopyRT.mipmapCount; mipLevel++)
                            Graphics.CopyTexture(output.finalCopyRT, slice, mipLevel, currentTexture, slice, mipLevel);
                }
            }
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

            // Set back the original params
            for (int i = 0; i < parentGraph.exposedParameters.Count; i++)
                parentGraph.exposedParameters[i].value = graphParamsValues[i];
        }

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
        }
#endif
    }
}
