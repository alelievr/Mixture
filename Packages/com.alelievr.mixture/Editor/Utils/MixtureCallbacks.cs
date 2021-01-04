using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Linq;
using UnityEditor.ProjectWindowCallback;
using System.IO;
using System.Reflection;
using UnityEngine.Experimental.Rendering;

#if MIXTURE_SHADERGRAPH
using UnityEditor.ShaderGraph;
#endif

namespace Mixture
{
	public class MixtureAssetCallbacks
	{
		public static readonly string	Extension = "asset";
		public static readonly string	customTextureShaderTemplate = "Templates/CustomTextureShaderTemplate";

		public static readonly string	shaderNodeCSharpTemplate = "Templates/FixedShaderNodeTemplate";
		public static readonly string	shaderNodeCGTemplate = "Templates/FixedShaderTemplate";
		public static readonly string	shaderNodeDefaultName = "MixtureShaderNode.cs";
		public static readonly string	shaderName = "MixtureShader.shader";
		public static readonly string	csharpComputeShaderNodeTemplate = "Templates/CsharpComputeShaderNodeTemplate";
		public static readonly string	computeShaderTemplate = "Templates/ComputeShaderTemplate";
		public static readonly string	computeShaderDefaultName = "MixtureCompute.compute";
		public static readonly string	computeShaderNodeDefaultName = "MixtureCompute.cs";
		public static readonly string	cSharpMixtureNodeTemplate = "Templates/CSharpMixtureNodeTemplate";
		public static readonly string	cSharpMixtureNodeName = "New Mixture Node.cs";
		public static readonly string	cSharpMixtureNodeViewTemplate = "Templates/CSharpMixtureNodeViewTemplate";
		public static readonly string	cSharpMixtureNodeViewName = "New Mixture Node View.cs";

		public static readonly string	customMipMapShader = "MixtureShader.shader";

		[MenuItem("Assets/Create/🎨 Static Mixture Graph", false, 83)]
		public static void CreateStaticMixtureGraph()
		{
			var graphItem = ScriptableObject.CreateInstance< StaticMixtureGraphAction >();
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, graphItem,
                $"New Static Mixture Graph.{Extension}", MixtureUtils.icon, null);
		}

		[MenuItem("Assets/Create/🌡️ Realtime Mixture Graph", false, 83)]
		public static void CreateRealtimeMixtureGraph()
		{
			var graphItem = ScriptableObject.CreateInstance< RealtimeMixtureGraphAction >();
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, graphItem,
                $"New Realtime Mixture Graph.{Extension}", MixtureUtils.realtimeIcon, null);
		}

		[MenuItem("Assets/Create/Mixture/Variant", false, 100)]
		public static void CreateMixtureVariant()
		{
			var selectedMixtures = Selection.GetFiltered<Texture>(SelectionMode.Assets);
			var selectedVariants = Selection.GetFiltered<MixtureVariant>(SelectionMode.Assets);

			foreach (var mixtureTexture in selectedMixtures)
			{
				var graph = MixtureDatabase.GetGraphFromTexture(mixtureTexture);
				if (graph != null)
					CreateMixtureVariant(graph, null);
			}

			foreach (var variant in selectedVariants)
				CreateMixtureVariant(null, variant);
		}

		[MenuItem("Assets/Create/Mixture/Variant", true)]
		public static bool CreateMixtureVariantEnabled()
		{
			var selectedMixtures = Selection.GetFiltered<Texture>(SelectionMode.Assets);
			int mixtureCount = selectedMixtures.Count(m => MixtureDatabase.GetGraphFromTexture(m) != null);
			var selectedVariants = Selection.GetFiltered<MixtureVariant>(SelectionMode.Assets);

			return (mixtureCount + selectedVariants.Length) > 0;
		}

		public static void CreateMixtureVariant(MixtureGraph targetGraph, MixtureVariant parentVariant)
		{
			string path;

			if (targetGraph != null)
				path = AssetDatabase.GetAssetPath(targetGraph);
			else
				path = AssetDatabase.GetAssetPath(parentVariant);

			// Patch path name to add Variant
			string fileName = Path.GetFileNameWithoutExtension(path) + " Variant";
			path = Path.Combine(Path.GetDirectoryName(path), fileName + Path.GetExtension(path));
			path = AssetDatabase.GenerateUniqueAssetPath(path);

			var action = ScriptableObject.CreateInstance< MixtureVariantAction >();
			action.targetGraph = targetGraph;
			action.parentVariant = parentVariant;
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, action,
            	Path.GetFileName(path), targetGraph.isRealtime ? MixtureUtils.realtimeVariantIcon : MixtureUtils.iconVariant, null);
		}

#if MIXTURE_SHADERGRAPH
		[MenuItem("Assets/Create/Mixture/Fixed Shader Graph", false, 202)]
		[MenuItem("Assets/Create/Shader/Custom Texture Graph", false, 200)]
		public static void CreateCustomTextureShaderGraph()
		{
			var graphItem = ScriptableObject.CreateInstance< CustomtextureShaderGraphAction >();
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, graphItem,
                $"New Custom Texture Graph.{ShaderGraphImporter.Extension}", Resources.Load<Texture2D>("sg_graph_icon@64"), null);
		}

		[MenuItem("Assets/Create/Mixture/Custom Mip Map Shader Graph", false, 903)]
		public static void CreateCustomMipMapShaderGraph()
		{
			var graphItem = ScriptableObject.CreateInstance< MipMapShaderGraphAction >();
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, graphItem,
                $"New Custom Mip Map.{ShaderGraphImporter.Extension}", Resources.Load<Texture2D>("sg_graph_icon@64"), null);
		}
#endif
		
		[MenuItem("Assets/Create/Mixture/C# Fixed Shader Node", false, 200)]
		public static void CreateCSharpFixedShaderNode()
		{
			var template = Resources.Load< TextAsset >(shaderNodeCSharpTemplate);
			ProjectWindowUtil.CreateScriptAssetFromTemplateFile(AssetDatabase.GetAssetPath(template), shaderNodeDefaultName);
		}

		[MenuItem("Assets/Create/Mixture/Fixed Shader", false, 201)]
		public static void CreateCGFixedShaderNode()
		{
			var template = Resources.Load< TextAsset >(shaderNodeCGTemplate);
			ProjectWindowUtil.CreateScriptAssetFromTemplateFile(AssetDatabase.GetAssetPath(template), shaderName);
		}

		[MenuItem("Assets/Create/Shader/Custom Texture", false, 100)]
		public static void CreateCustomTextureShader()
		{
			var shaderAction = ScriptableObject.CreateInstance< CustomTextureShaderAction >();
			var shaderTemplate = Resources.Load(customTextureShaderTemplate, typeof(Shader));
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, shaderAction,
                $"New Custom Texture Shader.shader",
				EditorGUIUtility.ObjectContent(null, typeof(Shader)).image as Texture2D,
				AssetDatabase.GetAssetPath(shaderTemplate)
			);
		}
		
		[MenuItem("Assets/Create/Mixture/C# Compute Shader Node", false, 300)]
		public static void CreateCSharpComuteShaderNode()
		{
			var template = Resources.Load< TextAsset >(csharpComputeShaderNodeTemplate);
			ProjectWindowUtil.CreateScriptAssetFromTemplateFile(AssetDatabase.GetAssetPath(template), computeShaderNodeDefaultName);
		}

		[MenuItem("Assets/Create/Mixture/Compute Shader", false, 301)]
		public static void CreateComuteShaderFile()
		{
			var template = Resources.Load< TextAsset >(computeShaderTemplate);
			ProjectWindowUtil.CreateScriptAssetFromTemplateFile(AssetDatabase.GetAssetPath(template), computeShaderDefaultName);
		}

		[MenuItem("Assets/Create/Mixture/C# Mixture Node", false, 401)]
		public static void CreateCSharpMixtureNodeFile()
		{
			var template = Resources.Load< TextAsset >(cSharpMixtureNodeTemplate);
			ProjectWindowUtil.CreateScriptAssetFromTemplateFile(AssetDatabase.GetAssetPath(template), cSharpMixtureNodeName);
		}

		[MenuItem("Assets/Create/Mixture/C# Mixture Node View", false, 402)]
		public static void CreateCSharpMixtureViewNodeFile()
		{
			var template = Resources.Load< TextAsset >(cSharpMixtureNodeViewTemplate);
			ProjectWindowUtil.CreateScriptAssetFromTemplateFile(AssetDatabase.GetAssetPath(template), cSharpMixtureNodeViewName);
		}

		[OnOpenAsset(0)]
		public static bool OnBaseGraphOpened(int instanceID, int line)
		{
			var asset = EditorUtility.InstanceIDToObject(instanceID);

			if (asset is Texture)
			{
				// Check if the CustomRenderTexture we're opening is a Mixture graph
				var path = AssetDatabase.GetAssetPath(EditorUtility.InstanceIDToObject(instanceID));
				var graph = MixtureEditorUtils.GetGraphAtPath(path);

				if (graph == null)
					return false;

				MixtureGraphWindow.Open(graph);
				return true;
			}
			else if (asset is MixtureVariant variant)
			{
				MixtureGraphWindow.Open(variant.parentGraph);
			}
			return false;
		}

		abstract class MixtureGraphAction : EndNameEditAction
		{
			public abstract MixtureGraph CreateMixtureGraphAsset();

			public override void Action(int instanceId, string pathName, string resourceFile)
			{
				var mixture = CreateMixtureGraphAsset();
				mixture.name = Path.GetFileNameWithoutExtension(pathName);
				mixture.hideFlags = HideFlags.HideInHierarchy;

				AssetDatabase.CreateAsset(mixture, pathName);

				// Generate the output texture:
				mixture.outputTextures.Clear();
				if (mixture.isRealtime)
				{
					mixture.UpdateRealtimeAssetsOnDisk();
				}
				else
				{
					MixtureGraphProcessor.RunOnce(mixture);
					mixture.SaveAllTextures(false);
				}

				ProjectWindowUtil.ShowCreatedAsset(mixture.mainOutputTexture);
				Selection.activeObject = mixture.mainOutputTexture;
				EditorApplication.delayCall += () => EditorGUIUtility.PingObject(mixture.mainOutputTexture);
			}
		}

		class StaticMixtureGraphAction : MixtureGraphAction
		{
			public static readonly string template = $"{MixtureEditorUtils.mixtureEditorResourcesPath}Templates/StaticMixtureGraphTemplate.asset";

			// By default isRealtime is false so we don't need to initialize it like in the realtime mixture create function
			public override MixtureGraph CreateMixtureGraphAsset()
			{
				var g = MixtureEditorUtils.GetGraphAtPath(template);
				g = ScriptableObject.Instantiate(g) as MixtureGraph;

				g.ClearObjectReferences();

				foreach (var node in g.nodes)
				{
					// Duplicate all the materials from the template
					if (node is ShaderNode s && s.material != null)
					{
						s.material = new Material(s.material);
						s.material.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
					}
					else if (node is OutputNode outputNode)
					{
						foreach (var outputSettings in outputNode.outputTextureSettings)
						{
							outputSettings.finalCopyMaterial = new Material(outputSettings.finalCopyMaterial);
							outputSettings.finalCopyMaterial.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
						}
					}
				}

				return g;
			}
		}

		class RealtimeMixtureGraphAction : MixtureGraphAction
		{
			public static readonly string template = $"{MixtureEditorUtils.mixtureEditorResourcesPath}Templates/RealtimeMixtureGraphTemplate.asset";

			public override MixtureGraph CreateMixtureGraphAsset()
			{
				var g = MixtureEditorUtils.GetGraphAtPath(template);
				g = ScriptableObject.Instantiate(g) as MixtureGraph;

				g.ClearObjectReferences();

				foreach (var node in g.nodes)
				{
					// Duplicate all the materials from the template
					if (node is ShaderNode s && s.material != null)
					{
						s.material = new Material(s.material);
						s.material.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
					}
					else if (node is OutputNode outputNode)
					{
						foreach (var outputSettings in outputNode.outputTextureSettings)
						{
							outputSettings.finalCopyMaterial = new Material(outputSettings.finalCopyMaterial);
							outputSettings.finalCopyMaterial.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
						}
					}
				}

				return g;
			}
		}

		class MixtureVariantAction : EndNameEditAction
		{
			public MixtureGraph targetGraph;
			public MixtureVariant parentVariant;

			public override void Action(int instanceId, string pathName, string resourceFile)
			{
				var variant = ScriptableObject.CreateInstance<MixtureVariant>();
				if (targetGraph == null)
					variant.SetParent(parentVariant);
				else
					variant.SetParent(targetGraph);

				variant.name = Path.GetFileNameWithoutExtension(pathName);
				variant.hideFlags = HideFlags.HideInHierarchy;

				AssetDatabase.CreateAsset(variant, pathName);

				// Duplicate all other mixture outputs
				var subAssets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath((Object)targetGraph ?? parentVariant));
				Texture mainTexture = null;
				foreach (var asset in subAssets)
				{
					if ((asset.hideFlags & (HideFlags.HideInHierarchy | HideFlags.HideInInspector)) != 0)
						continue;

					if (asset is Texture texture)
					{
						var t = DuplicateTexture(texture);
						AssetDatabase.AddObjectToAsset(t, pathName);
						if (texture.name == targetGraph.mainOutputTexture.name)
							mainTexture = t;
						Debug.Log("Duplicate " + texture);
					}
				}

				// Set main output texture
				AssetDatabase.SetMainObject(mainTexture, pathName);

				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();

				EditorApplication.delayCall += () => {
					Selection.activeObject = mainTexture;
					EditorGUIUtility.PingObject(mainTexture);
				};

				Texture DuplicateTexture(Texture source)
				{
					TextureCreationFlags flags = source.mipmapCount > 1 ? TextureCreationFlags.MipChain : TextureCreationFlags.None;

					switch (source)
					{
						case Texture2D t2D:
							var new2D = new Texture2D(t2D.width, t2D.height, t2D.graphicsFormat, t2D.mipmapCount, flags);
							new2D.name = source.name;

							for (int mipLevel = 0; mipLevel < t2D.mipmapCount; mipLevel++)
								new2D.SetPixelData(t2D.GetPixelData<byte>(mipLevel), mipLevel);

							return new2D;
						case Texture3D t3D:
							var new3D = new Texture3D(t3D.width, t3D.height, t3D.depth, t3D.graphicsFormat, flags, t3D.mipmapCount);
							new3D.name = source.name;

							for (int mipLevel = 0; mipLevel < t3D.mipmapCount; mipLevel++)
								new3D.SetPixelData(t3D.GetPixelData<byte>(mipLevel), mipLevel);

							return new3D;
						case Cubemap cube:
							var newCube = new Cubemap(cube.width, cube.graphicsFormat, flags, cube.mipmapCount);
							
							for (int slice = 0; slice < TextureUtils.GetSliceCount(cube); slice++)
								for (int mipLevel = 0; mipLevel < cube.mipmapCount; mipLevel++)
									newCube.SetPixelData(cube.GetPixelData<byte>(mipLevel, (CubemapFace)slice), mipLevel, (CubemapFace)slice);

							newCube.name = source.name;
							return newCube;
						case RenderTexture rt:
							var newRT = new RenderTexture(rt.width, rt.height, 0, rt.graphicsFormat, rt.mipmapCount);
							newRT.name = source.name;
							newRT.enableRandomWrite = rt.enableRandomWrite;

							for (int slice = 0; slice < TextureUtils.GetSliceCount(rt); slice++)
								for (int mipLevel = 0; mipLevel < rt.mipmapCount; mipLevel++)
									Graphics.CopyTexture(rt, slice, mipLevel, newRT, slice, mipLevel);

							return newRT;
						default:
							throw new System.Exception("Can't duplicate texture of type " + source.GetType());
					}
				}
			}
		}

		class CustomTextureShaderAction : EndNameEditAction
		{
			static MethodInfo	createScriptAsset = typeof(ProjectWindowUtil).GetMethod("CreateScriptAssetFromTemplate", BindingFlags.Static | BindingFlags.NonPublic);

			public override void Action(int instanceId, string pathName, string resourceFile)
			{
				if (!File.Exists(resourceFile))
                {
                    Debug.LogError("Can't find template: " + resourceFile);
                    return ;
                }

				createScriptAsset.Invoke(null, new object[]{ pathName, resourceFile });
				ProjectWindowUtil.ShowCreatedAsset(AssetDatabase.LoadAssetAtPath<Shader>(pathName));
				AssetDatabase.Refresh();
			}
		}

		static Shader ReimportShaderGraphResource(string resourcePath)
		{
			string fullPath = "Packages/com.alelievr.mixture/Editor/Resources/" + resourcePath;

			AssetDatabase.ImportAsset(fullPath, ImportAssetOptions.DontDownloadFromCacheServer | ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);

			return Resources.Load< Shader >(resourcePath);
		}

		class CustomtextureShaderGraphAction : EndNameEditAction
		{
			public static readonly string template = "Templates/CustomTextureGraphTemplate";

			public override void Action(int instanceId, string pathName, string resourceFile)
			{
				var s = Resources.Load(template, typeof(Shader));

				// In case there was a compilation error sg files can be broken so we re-import them
				if (s == null)
					s = ReimportShaderGraphResource(template);

				AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(s), pathName);
				ProjectWindowUtil.ShowCreatedAsset(AssetDatabase.LoadAssetAtPath<Shader>(pathName));
			}
		}

		class MipMapShaderGraphAction : EndNameEditAction
		{
			public static readonly string template = "Templates/CustomMipMapTemplate";

			public override void Action(int instanceId, string pathName, string resourceFile)
			{
				var s = Resources.Load(template, typeof(Shader));

				// In case there was a compilation error sg files can be broken so we re-import them
				if (s == null)
					s = ReimportShaderGraphResource(template);

				AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(s), pathName);
				ProjectWindowUtil.ShowCreatedAsset(AssetDatabase.LoadAssetAtPath<Shader>(pathName));
			}
		}
	}
}