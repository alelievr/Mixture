using UnityEngine;
using UnityEditor;
using System.Collections;
using NodeEditorFramework;

[System.Serializable]
[Node (false, "CSG/Primitive", false)]
public class PrimitiveNode : Node 
{
	public enum PrimitiveType { Plane, Cube, Cone, Tube, Sphere, Torus }
	public PrimitiveType	type;
	
	// What sort of detail to use on the primitive - how many triangles to use (mainly)
	public float 			detail = 1.0f;
	// Dimensions of the object in X,Y,Z spaceFree
	public Vector3 			dimension = new Vector3(1,1,1);
	// Position of the object - and object can be provided as a parent for position. 
	public Vector3 			location = new Vector3(0,0,0);		

	public const string ID = "primitiveNode";
	public override string GetID { get { return ID; } }

	public GameObject value = null;
	public GameObject parent = null;

	public override Node Create (Vector2 pos) 
	{ // This function has to be registered in Node_Editor.ContextCallback
		PrimitiveNode node = CreateInstance <PrimitiveNode> ();
		
		node.name = "Primitive Node";
		node.rect = new Rect (pos.x, pos.y, 200, 100);

		NodeInput.Create (node, "Parent", "GameObject");
		NodeOutput.Create (node, "GameObject", "GameObject");

		return node;
	}

	public override void NodeGUI () 
	{
		GUILayout.BeginHorizontal ();
		GUILayout.BeginVertical ();

        parent = (GameObject)GUIExt.ObjectField(parent, this);
        InputKnob(0);
		
		location = UnityEditor.EditorGUILayout.Vector3Field( "", location, GUILayout.Height(20));

		type = (PrimitiveType)UnityEditor.EditorGUILayout.EnumPopup("Primitive", type);

		Outputs [0].DisplayLayout ();
		GUILayout.EndVertical ();
		GUILayout.EndHorizontal ();

		if (GUI.changed)
			NodeEditor.RecalculateFrom (this);
	}
	
	public override bool Calculate () 
	{
		if (Inputs [0].connection != null)
			parent = Inputs [0].connection.GetValue<GameObject> ();

        string name = "";

		// Make a primitive everytime calculate is called (in case type or loc changes)
		switch (type) {
		case PrimitiveType.Cube:
            name = CSGPrimitives.MakePrimitiveName(this.GetHashCode());
            value = GameObject.Find(name);
            if (value != null) DestroyImmediate(value);
            value = GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Cube);
            value.name = name;
			break;
		case PrimitiveType.Plane:
            name = CSGPrimitives.MakePrimitiveName(this.GetHashCode());
            value = GameObject.Find(name);
            if (value != null) DestroyImmediate(value);
            value = GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Plane);
            value.name = name;
			break;
		case PrimitiveType.Cone:
            value = CSGPrimitives.CreateCone(this.GetHashCode());
			break;
		case PrimitiveType.Tube:
            name = CSGPrimitives.MakePrimitiveName(this.GetHashCode());
            value = GameObject.Find(name);
            if (value != null) DestroyImmediate(value);
            value = GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Cylinder);
            value.name = name;
            break;
		case PrimitiveType.Sphere:
            name = CSGPrimitives.MakePrimitiveName(this.GetHashCode());
            value = GameObject.Find(name);
            if (value != null) DestroyImmediate(value);
            value = GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Sphere);
            value.name = name;
			break;
		case PrimitiveType.Torus:
            value = CSGPrimitives.CreateTorus(this.GetHashCode());
			break;
		}
		
		if (value != null) {
			CSGObject csg = value.GetComponent<CSGObject>();
			if(csg == null)
				csg = value.AddComponent<CSGObject>();
			csg.GenerateSolid();

            if (parent != null){
                value.transform.SetParent(parent.transform);
                value.transform.localPosition = location;
            }
            else {
                value.transform.SetParent(null);
                value.transform.position = location;
            }
        }

		Outputs[0].SetValue<GameObject> (value);
		return true;
	}
}