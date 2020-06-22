using UnityEngine;
using System.Collections;
using Net3dBool;

/*
 * Solid to Unity Mesh Demo
*/



public class MyDemo: MonoBehaviour
{
    public enum Operator
    {
        Union,
        Intersection,
        Difference,
    }

    // Member
    public MeshFilter goA;
    public MeshFilter goB;
    public Material resultGoMaterial;
    public Operator op;
    GameObject resultGo;

    MeshRenderer mr;
    MeshFilter mf;

    Solid GetSolid(MeshFilter mf)
    {
		if (mf == null) {
			Debug.LogError ("No mesh filter to get vertices from.");
			return null;
		}

		var solidObject = new Solid (mf.sharedMesh.vertices, mf.sharedMesh.triangles, mf.sharedMesh.normals);
		// Make sure the transform has been pushed into the solid.
		solidObject.ApplyMatrix (mf.transform.localToWorldMatrix);

        return solidObject;
    }

	void Start ()
    {
        // initialize resulting GO
        resultGo = new GameObject();
        resultGo.transform.position = Vector3.zero;
        mf = resultGo.AddComponent<MeshFilter>();
        mr = resultGo.AddComponent<MeshRenderer>();

        // build Net3dBool solids, or implement BooleanModeller method which can handle the Unity stuff
        Solid a = GetSolid(goA);
        Solid b = GetSolid(goB);
        BooleanModeller modeller = new BooleanModeller(a, b);
        Solid newSolid;

        switch (op)
        {
            case Operator.Difference:
                newSolid = modeller.getDifference();
                break;
            case Operator.Intersection:
                newSolid = modeller.getIntersection();
                break;
            default:
            case Operator.Union:
                newSolid = modeller.getUnion();
                break;
        }

        // Apply vertices of new solid to a gameobject
        Mesh tMesh = new Mesh();
        var overtices = newSolid.getVertices();
        int mLen = overtices.Length;
        Vector3[] vertices = new Vector3[mLen];

        // parse Point3d to Vector3
        for(int i = 0; i < mLen; i++)
        {
            Point3d p = overtices[i];
            vertices[i] = new Vector3((float)p.x, (float)p.y, (float)p.z);
        }

        tMesh.vertices = vertices;
        tMesh.triangles = newSolid.getIndices();
        tMesh.normals = newSolid.getNormals();

        // parse colors
        // int cLen = newSolid.getColors().Length;
        // Color[] clrs = new Color[cLen];
        // for (int j = 0; j < cLen; j++)
        // {
        //     Net3dBool.Color3f c = newSolid.getColors()[j];
        //     clrs[j] = new Color((float)c.r, (float)c.g, (float)c.b);
        // }
        // tMesh.colors = clrs;

        tMesh.RecalculateNormals();
        mf.mesh = tMesh;

        mr.materials = new Material[1];
        mr.materials[0] = resultGoMaterial;
        mr.material = resultGoMaterial;
    }

    Color3f[] getColorArray(int length, Color c)
    {
        var ar = new Net3dBool.Color3f[length];
        for (var i = 0; i < length; i++)
            ar[i] = new Net3dBool.Color3f(c.r, c.g, c.b);
        return ar;
    }
}
