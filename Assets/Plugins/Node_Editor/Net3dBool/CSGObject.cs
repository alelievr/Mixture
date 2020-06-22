using UnityEngine;
using System.Collections;
using Net3dBool;

public class CSGObject : MonoBehaviour {
	
	private	Solid 		solidObject; 

	public void GenerateSolid()
	{
		// Try to grab any meshes this is attached to - if not, allow setting!
		MeshFilter mf = gameObject.GetComponent<MeshFilter> ();
		if (mf == null) {
			Debug.LogError ("No mesh filter to get vertices from.");
			return;
		}
		
		solidObject = new Solid (mf.sharedMesh.vertices, mf.sharedMesh.triangles, mf.sharedMesh.colors);
		// Make sure the transform has been pushed into the solid.
		solidObject.ApplyMatrix (gameObject.transform.localToWorldMatrix);
	}

	public void AssignSolid(Solid _solid)
	{
		solidObject = _solid;
	}

	public Solid GetSolid()
	{
		return solidObject;
	}
}
