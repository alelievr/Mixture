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
        Point3d[] mappedVertices = new Point3d[vertices.Length];
        Color[] colors = new Color[vertices.Length];
        Color3f[] mappedColors = new Color3f[vertices.Length];

        // Parse vertices and colors
        for (int i = 0; i < vertices.Length; i++)
        {
            mappedVertices[i] = new Point3d(vertices[i].x, vertices[i].y, vertices[i].z);
        }

        // Parse colors
        for(int i = 0; i < mappedColors.Length; i++)
        {
            // Generate color if null
            if(colors[i] != null)
            {
                colors[i] = Color.black;
            }
            mappedColors[i] = new Color3f(colors[i].r, colors[i].g, colors[i].b);
        }
        return new Solid(mappedVertices, indices, mappedColors);
    }

    Mesh Create3dMesh(Solid solid)
    {
        Mesh mesh = new Mesh();

        Point3d[] vertices = solid.getVertices();
        Vector3[] mappedVertices = new Vector3[vertices.Length];
        Color3f[] colors = solid.getColors();
        Color[] mappedColors = new Color[colors.Length];

        // Parse vertices
        for (int i = 0; i < vertices.Length; i++)
        {
            mappedVertices[i] = new Vector3((float)vertices[i].x, (float)vertices[i].y, (float)vertices[i].z);
        }

        // Parse colors
        for (int i = 0; i < colors.Length; i++)
        {
            mappedColors[i] = new Color((float)colors[i].r, (float)colors[i].g, (float)colors[i].b);
        }

        // Attention! Always set vertices before indices/triangles
        mesh.vertices = mappedVertices;
        mesh.triangles = solid.getIndices();
        mesh.colors = mappedColors;
        mesh.RecalculateNormals();

        return mesh;
    }
}
