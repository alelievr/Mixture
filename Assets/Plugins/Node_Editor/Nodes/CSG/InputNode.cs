using UnityEngine;
using UnityEditor;
using System.Collections;
using NodeEditorFramework;

[System.Serializable]
[Node (false, "CSG/Input", false)]
public class InputNode : Node 
{
	public const string ID = "inputNode";
	public override string GetID { get { return ID; } }

	public GameObject value = null;

	public override Node Create (Vector2 pos) 
	{ // This function has to be registered in Node_Editor.ContextCallback
		InputNode node = CreateInstance <InputNode> ();
		
		node.name = "Input Node";
		node.rect = new Rect (pos.x, pos.y, 140, 50);
		
		NodeOutput.Create (node, "Value", "GameObject");

		return node;
	}

	public override void NodeGUI () 
	{
		value = (GameObject)GUIExt.ObjectField ( value, this);
		OutputKnob (0);

		if (GUI.changed)
			NodeEditor.RecalculateFrom (this);
	}
	
	public override bool Calculate () 
	{
		if (value != null) {
			CSGObject csg = value.GetComponent<CSGObject>();
			if(csg == null)
				csg = value.AddComponent<CSGObject>();
			csg.GenerateSolid();
		}

		Outputs[0].SetValue<GameObject> (value);
		return true;
	}
}