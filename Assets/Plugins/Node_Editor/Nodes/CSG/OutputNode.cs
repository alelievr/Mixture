using UnityEngine;
using System.Collections;
using NodeEditorFramework;

[System.Serializable]
[Node (false, "CSG/Output", false)]
public class OutputNode : Node 
{
	public const string ID = "outputNode";
	public override string GetID { get { return ID; } }

	[HideInInspector]
	public bool assigned = false;
	public GameObject value = null;

	public override Node Create (Vector2 pos) 
	{ // This function has to be registered in Node_Editor.ContextCallback
		OutputNode node = CreateInstance <OutputNode> ();
		
		node.name = "Output Node";
		node.rect = new Rect (pos.x, pos.y, 150, 50);
		
		NodeInput.Create (node, "Value", "GameObject");

		return node;
	}
	
	public override void NodeGUI () 
	{
		Inputs [0].DisplayLayout (new GUIContent ("Value : " + (assigned? value.ToString () : ""), "The input GameObject to display"));
	}
	
	public override bool Calculate () 
	{
		if (!allInputsReady ()) 
		{
			value = null;
			assigned = false;
			return false;
		}

		value = Inputs[0].connection.GetValue<GameObject>();
		assigned = true;

		return true;
	}
}
