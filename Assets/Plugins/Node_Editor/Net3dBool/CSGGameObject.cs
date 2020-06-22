using UnityEngine;
using System.Collections;
using Net3dBool;

public class CSGGameObject {

	// Update is called once per frame
	public static void GenerateMesh(GameObject go, Material ObjMaterial, Solid mesh) {

		MeshFilter mf = go.GetComponent<MeshFilter> ();
		if(mf == null)
			mf = go.AddComponent<MeshFilter> ();

		Mesh tmesh = new Mesh();
		int mlen = mesh.getVertices().Length;
		Vector3 [] vertices = new Vector3[mlen];
		for(int i=0; i<mlen; i++)
		{
			Net3dBool.Point3d p = mesh.getVertices()[i];
			vertices[i] = new Vector3((float)p.x, (float)p.y, (float)p.z);
		}
		tmesh.vertices = vertices;
		
		tmesh.triangles = mesh.getIndices ();
		int clen = mesh.getColors ().Length;
		Color [] clrs = new Color[clen];
		for (int j=0; j<clen; j++) {
			Net3dBool.Color3f c = mesh.getColors()[j];
			clrs[j] = new Color((float)c.r, (float)c.g, (float)c.b);
		}
		tmesh.colors = clrs;
		tmesh.RecalculateNormals();
		mf.mesh = tmesh;
		
		MeshRenderer mr = go.GetComponent<MeshRenderer> ();
		if(mr == null) mr = go.AddComponent<MeshRenderer> ();
		mr.sharedMaterials = new Material[1];
		mr.sharedMaterials[0] = ObjMaterial;
		mr.sharedMaterial = ObjMaterial;
	}
}
