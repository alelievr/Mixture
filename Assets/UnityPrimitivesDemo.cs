using UnityEngine;
using System.Collections;
using Net3dBool;

public class UnityPrimitivesDemo : MonoBehaviour
{
    // Member
    public GameObject goA;
    public GameObject goB;
    public GameObject resultGo;
    public Material resultGoMaterial;

    void Start()
    {
        // Mesh to Solid
        Solid solidA = Create3dSolid(goA.GetComponent<MeshFilter>().mesh);
        Solid solidB = Create3dSolid(goB.GetComponent<MeshFilter>().mesh);

        // Apply Boolean operations
        BooleanModeller modeller = new BooleanModeller(solidA, solidB);
        Solid resultSolid = modeller.getUnion();

        // Solid to Mesh
        Mesh resultMesh = Create3dMesh(resultSolid);

        // Apply Mesh to new GameObject
        MeshFilter mf = resultGo.AddComponent<MeshFilter>();
        MeshRenderer mr = resultGo.AddComponent<MeshRenderer>();

        mf.mesh = resultMesh;
        mr.materials = new Material[1];
        mr.materials[0] = resultGoMaterial;
        mr.material = resultGoMaterial;

        // Disable previous GameObjects
        goA.SetActive(false);
        goB.SetActive(false);
    }

    Solid Create3dSolid(Mesh mesh)
    {
        int[] indices = mesh.GetIndices(0);
        Vector3[] vertices = mesh.vertices;
        Vector3[] mappedVertices = new Vector3[vertices.Length];
        Vector3[] normals = new Vector3[vertices.Length];
        Vector3[] mappedNormals = new Vector3[vertices.Length];

        // Parse vertices and normals
        // for (int i = 0; i < vertices.Length; i++)
        // {
        //     mappedVertices[i] = vertices[i];
        // }

        // Parse normals
        for(int i = 0; i < mappedNormals.Length; i++)
        {
            // Generate color if null
            if(normals[i] != null)
            {
                normals[i] = Vector3.up;
            }
            mappedNormals[i] = normals[i];
        }
        return new Solid(mesh.vertices, indices, mappedNormals);
    }

    Mesh Create3dMesh(Solid solid)
    {
        Mesh mesh = new Mesh();

        Point3d[] vertices = solid.getVertices();
        Vector3[] mappedVertices = new Vector3[vertices.Length];
        Vector3[] normals = solid.getNormals();
        Vector3[] mappedNormals = new Vector3[normals.Length];

        // Parse vertices
        for (int i = 0; i < vertices.Length; i++)
        {
            mappedVertices[i] = new Vector3((float)vertices[i].x, (float)vertices[i].y, (float)vertices[i].z);
        }

        // Parse normals
        for (int i = 0; i < normals.Length; i++)
        {
            mappedNormals[i] = normals[i];
        }

        // Attention! Always set vertices before indices/triangles
        mesh.vertices = mappedVertices;
        mesh.triangles = solid.getIndices();
        mesh.normals = mappedNormals;
        // mesh.RecalculateNormals();

        return mesh;
    }
}
