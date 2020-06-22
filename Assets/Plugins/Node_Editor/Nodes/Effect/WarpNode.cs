using UnityEngine;
using NodeEditorFramework;

[Node (false, "Effect/Warp Node", false)]
public class WarpNode : Node 
{
	public enum WarpType { Shear, Project, Extrude }
	public WarpType type = WarpType.Shear;
	public Vector3 strength = new Vector3(1,1,1);
	
	public const string ID = "warpNode";
	public override string GetID { get { return ID; } }

	// Store original input verts - only change when input changes
	private Vector3 [] storeVerts;
	// Input Object - if this changes - update input verts
	private GameObject inObj;

	public override Node Create (Vector2 pos) 
	{
		WarpNode node = CreateInstance<WarpNode> ();
		
		node.rect = new Rect (pos.x, pos.y, 200, 100);
		node.name = "Warp Node";
		
		node.CreateInput ("In Obj", "GameObject");
		node.CreateOutput ("Out Obj", "GameObject");
		
		return node;
	}
	
	public override void NodeGUI () 
	{
		UnityEditor.EditorGUIUtility.labelWidth = 100;
		
		GUILayout.BeginHorizontal ();
		GUILayout.BeginVertical ();
		
		Inputs [0].DisplayLayout ();
		
		GUILayout.EndVertical ();
		GUILayout.BeginVertical ();
		
		Outputs [0].DisplayLayout ();
		
		GUILayout.EndVertical ();
		GUILayout.EndHorizontal ();
		
		type = (WarpType)UnityEditor.EditorGUILayout.EnumPopup (
			new GUIContent ("CSG Operation", "The type of warp performed on Input 1"), type);
		strength = UnityEditor.EditorGUILayout.Vector3Field ("Strength", strength);
		
		if (GUI.changed)
			NodeEditor.RecalculateFrom (this);
	}
	
	public override bool Calculate () 
	{
		if (!allInputsReady ())
			return false;
		
		GameObject tempObj = Inputs [0].connection.GetValue<GameObject> ();
		if (tempObj == null) 
			return false;
		if (inObj == null || tempObj != inObj) {
			inObj = tempObj;
			// Collect verts when input changes - flushes output!!
			MeshFilter tmpMF = inObj.GetComponent<MeshFilter>();
			if(tmpMF) storeVerts = tmpMF.sharedMesh.vertices;
		}

		MeshFilter mf = inObj.GetComponent<MeshFilter> ();
		if (mf == null)
			return false;
		
		// Check the type and apply the operation on it
		switch (type) {
		case WarpType.Shear:
			Matrix4x4 m = Matrix4x4.identity;
			m.m01 = strength.x;
			m.m02 = strength.x;
			m.m10 = strength.y;
			m.m12 = strength.y;
			m.m20 = strength.z;
			m.m21 = strength.z;

			Vector3 [] newverts = new Vector3[storeVerts.Length];
			for(int i=0; i<storeVerts.Length; i++)
				newverts[i] = m.MultiplyPoint3x4( storeVerts[i] );
			mf.sharedMesh.vertices = newverts;
			break;
		case WarpType.Project:
			break;
		case WarpType.Extrude:
			break;
		}
		
		Outputs[0].SetValue<GameObject> (inObj);
		return true;
	}
}
