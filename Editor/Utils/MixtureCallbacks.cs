using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GraphProcessor;
using UnityEditor.Callbacks;
using System.Linq;
using UnityEditor.ProjectWindowCallback;
using System.IO;

namespace Mixture
{
	public class MixtureAssetCallbacks
	{
		public static readonly string	Extension = "asset";

		[MenuItem("Assets/Create/Mixture Graph", false, 100)]
		public static void CreateMixtureGraph()
		{
			var graphItem = ScriptableObject.CreateInstance< MixtureGraphAction >();
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, graphItem,
                string.Format("New Mixture Graph.{0}", Extension), MixtureUtils.icon, null);
		}

		[OnOpenAsset(0)]
		public static bool OnBaseGraphOpened(int instanceID, int line)
		{
			var asset = EditorUtility.InstanceIDToObject(instanceID);

			if (asset is Texture)
			{
				// Check if the CustomRenderTexture we're opening is a Mixture graph
				var path = AssetDatabase.GetAssetPath(EditorUtility.InstanceIDToObject(instanceID));
				var graph = AssetDatabase.LoadAllAssetsAtPath(path).FirstOrDefault(o => o is MixtureGraph) as MixtureGraph;

				if (graph == null)
					return false;

				MixtureGraphWindow.Open().InitializeGraph(graph);
				return true;
			}
			return false;
		}

		class MixtureGraphAction : EndNameEditAction
		{
			public override void Action(int instanceId, string pathName, string resourceFile)
			{
				var mixture = ScriptableObject.CreateInstance< MixtureGraph >();
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

				UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath< Texture >(pathName);

				if (obj != null)
					EditorGUIUtility.PingObject(obj);
			}
		}
	}
}