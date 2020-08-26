using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Linq;
using UnityEditor.ProjectWindowCallback;
using System.IO;
using System.Reflection;

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

#if MIXTURE_SHADERGRAPH
		[MenuItem("Assets/Create/Mixture/Fixed Shader Graph", false, 202)]
		[MenuItem("Assets/Create/Shader/Custom Texture Graph", false, 200)]
		public static void CreateCustomTextureShaderGraph()
		{
			var graphItem = ScriptableObject.CreateInstance< CustomtextureShaderGraphAction >();
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, graphItem,
                $"New Custom Texture Graph.{ShaderGraphImporter.Extension}", Resources.Load<Texture2D>("sg_graph_icon@64"), null);
		}

		[MenuItem("Assets/Create/Mixture/Custom Mip Map", false, 403)]
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
				mixture.UpdateOutputTexture(false);

				// Then set it as main object
				AssetDatabase.AddObjectToAsset(mixture.outputTexture, mixture);
				AssetDatabase.SetMainObject(mixture.outputTexture, pathName);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();

				ProjectWindowUtil.ShowCreatedAsset(mixture.outputTexture);
				Selection.activeObject = mixture.outputTexture;
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
						var m = s.material;
						s.material = new Material(s.material);
						s.material.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
					}
				}

				return g;
			}
		}

		class RealtimeMixtureGraphAction : MixtureGraphAction
		{
			public static readonly string template = $"{MixtureEditorUtils.mixtureEditorResourcesPath}Templates/RealtimeMixtureGraphTemplate.asset";

			static MixtureGraph	_realtimeGraph;
			static MixtureGraph realtimeGraph
			{
				get
				{
					if (_realtimeGraph == null)
					{
						_realtimeGraph = ScriptableObject.CreateInstance< MixtureGraph >();
						_realtimeGraph.isRealtime = true;
						_realtimeGraph.realtimePreview = true;
					}
					return _realtimeGraph;
				}
			}

			public override MixtureGraph CreateMixtureGraphAsset()
			// We use Instantiate instead of CreateObject because we can use anther mixture graph that
			// have the parameters setup for realtime mixtures. This avoid the issue to have the ScriptableObject
			// OnEnable() function called before the isRealtime is assigned
				=> Object.Instantiate(realtimeGraph);
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

		class CustomtextureShaderGraphAction : EndNameEditAction
		{
			public static readonly string template = "Templates/CustomTextureGraphTemplate";

			public override void Action(int instanceId, string pathName, string resourceFile)
			{
				var s = Resources.Load(template, typeof(Shader));
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
				AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(s), pathName);
				ProjectWindowUtil.ShowCreatedAsset(AssetDatabase.LoadAssetAtPath<Shader>(pathName));
			}
		}
	}
}