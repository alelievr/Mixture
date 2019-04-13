using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GraphProcessor;
using UnityEditor.Callbacks;

public class MixtureAssetCallbacks
{
	[MenuItem("Assets/Create/Mixture", false, 100)]
	public static void CreateMixtureGraph()
	{
		var		obj = Selection.activeObject;
		string	path;

		if (obj == null)
			path = "Assets";
		else
			path = AssetDatabase.GetAssetPath(obj.GetInstanceID());

		var graph = ScriptableObject.CreateInstance< MixtureGraph >();
		ProjectWindowUtil.CreateAsset(graph, path + "/Mixture.asset");
	}

	[OnOpenAsset(0)]
	public static bool OnBaseGraphOpened(int instanceID, int line)
	{
		var asset = EditorUtility.InstanceIDToObject(instanceID);

		if (asset is MixtureGraph graph)
		{
			MixtureGraphWindow.Open().InitializeGraph(graph);
			return true;
		}
		return false;
	}
}
