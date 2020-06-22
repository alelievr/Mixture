using UnityEngine;
using System.Collections;
using NodeEditorFramework;

using Net3dBool;

[System.Serializable]
[Node (false, "CSG/Operator", false)]
public class OperatorNode : Node 
{
	public enum CalcType { Union, Intersection, Difference }
	public CalcType type = CalcType.Union;

	public const string ID = "operatorNode";
	public override string GetID { get { return ID; } }

	public GameObject Input1Val = null;
	public GameObject Input2Val = null;

	// The solid modeller to execute the operations 
	private BooleanModeller		modeller;
	private Solid 				solid1;
	private Solid				solid2;
	private Solid 				output;

	private Material 			objectMaterial;

	public override Node Create (Vector2 pos) 
	{
		OperatorNode node = CreateInstance <OperatorNode> ();
		
		node.name = "Operator Node";
		node.rect = new Rect (pos.x, pos.y, 200, 100);
		
		node.CreateInput ("Input 1", "GameObject");
		node.CreateInput ("Input 2", "GameObject");
		
		node.CreateOutput ("Output 1", "GameObject");

		return node;
	}

	public override void NodeGUI () 
	{
		GUILayout.BeginHorizontal ();
		GUILayout.BeginVertical ();

		if (Inputs [0].connection != null)
			GUILayout.Label (Inputs [0].name);
		else
			Input1Val = (GameObject)GUIExt.ObjectField (Input1Val, this);
		InputKnob (0);
		// --
		if (Inputs [1].connection != null)
			GUILayout.Label (Inputs [1].name);
		else
			Input2Val = (GameObject)GUIExt.ObjectField (Input2Val, this);
		InputKnob (1);

		objectMaterial = (Material)UnityEditor.EditorGUILayout.ObjectField (objectMaterial, typeof(Material));

		GUILayout.EndVertical ();
		GUILayout.BeginVertical ();

		Outputs [0].DisplayLayout ();

		GUILayout.EndVertical ();
		GUILayout.EndHorizontal ();

		type = (CalcType)UnityEditor.EditorGUILayout.EnumPopup (
			new GUIContent ("CSG Operation", "The type of calculation performed on Input 1 and Input 2"), type);

		if (GUI.changed)
			NodeEditor.RecalculateFrom (this);
	}

	public override bool Calculate () 
	{
		if (!allInputsReady ()) {
			return false;
		}

		if (Inputs [0].connection != null) {
			Input1Val = Inputs [0].connection.GetValue<GameObject> ();

			CSGObject csg = Input1Val.GetComponent<CSGObject>();
			if(csg == null)
				csg = Input1Val.AddComponent<CSGObject>();
			csg.GenerateSolid();
		}
		if (Inputs [1].connection != null) {

			Input2Val = Inputs [1].connection.GetValue<GameObject> ();
			CSGObject csg = Input2Val.GetComponent<CSGObject> ();
			if (csg == null)
				csg = Input2Val.AddComponent<CSGObject> ();
			csg.GenerateSolid ();
		}

		if ((Input1Val != null) && (Input2Val != null)) {

			solid1 = Input1Val.GetComponent<CSGObject> ().GetSolid ();
			solid2 = Input2Val.GetComponent<CSGObject> ().GetSolid ();

			modeller = new BooleanModeller (solid1, solid2);
			output = null;

			switch (type) {
			case CalcType.Union:
				output = modeller.getUnion ();
				break;
			case CalcType.Intersection:
				output = modeller.getIntersection ();
				break;
			case CalcType.Difference:
				output = modeller.getDifference ();
				break;
			}

			if (output != null) {
				string goname = string.Format ("CSGOut_{0}", (int)this.GetHashCode ());
				GameObject gout = GameObject.Find (goname); 
				if (gout == null)
					gout = new GameObject (goname);
				CSGObject csg = gout.GetComponent<CSGObject>();
				if(csg == null) csg = gout.AddComponent<CSGObject>();
				csg.AssignSolid(output);
				CSGGameObject.GenerateMesh (gout, objectMaterial, output);

				Outputs [0].SetValue<GameObject> (gout);
			}
		} else
			return false;

		return true;
	}
}
