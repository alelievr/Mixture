using System;
using System.Collections.Generic;
using UnityEngine;


public class TempMesh
{
    public List<Vector3> vertices;
    public List<Vector3> normals;
    public List<Vector2> uvs;
    public List<int> triangles;

    // Mappings of indices from original mesh to new mesh
    private Dictionary<int, int> vMapping;

    public float surfacearea;

    public TempMesh(int vertexCapacity)
    {
        vertices = new List<Vector3>(vertexCapacity);
        normals = new List<Vector3>(vertexCapacity);
        uvs = new List<Vector2>(vertexCapacity);
        triangles = new List<int>(vertexCapacity * 3);

        vMapping = new Dictionary<int, int>(vertexCapacity);

        surfacearea = 0;
    }

    public void Clear()
    {
        vertices.Clear();
        normals.Clear();
        uvs.Clear();
        triangles.Clear();

        vMapping.Clear();

        surfacearea = 0;
    }

    /// <summary>
    /// Add point and normal to arrays if not already present
    /// </summary>
    private void AddPoint(Vector3 point, Vector3 normal, Vector2 uv)
    {
        triangles.Add(vertices.Count);
        vertices.Add(point);
        normals.Add(normal);
        uvs.Add(uv);
    }

    /// <summary>
    /// Add triangles from the original mesh. Therefore, no new vertices to add 
    /// and no normals to compute
    /// </summary>
    public void AddOgTriangle(int[] indices)
    {
        for (int i = 0; i < 3; ++i)
            triangles.Add(vMapping[indices[i]]);

        //Compute triangle area
        surfacearea += GetTriangleArea(triangles.Count - 3);
    }

    public void AddSlicedTriangle(int i1, Vector3 v2, Vector2 uv2, int i3)
    {
        int v1 = vMapping[i1],
            v3 = vMapping[i3];
        Vector3 normal = Vector3.Cross(v2 - vertices[v1], vertices[v3] - v2).normalized;

        triangles.Add(v1);
        AddPoint(v2, normal, uv2);
        triangles.Add(vMapping[i3]);

        //Compute triangle area
        surfacearea += GetTriangleArea(triangles.Count - 3);
    }

    public void AddSlicedTriangle(int i1, Vector3 v2, Vector3 v3, Vector2 uv2, Vector2 uv3)
    {
        // Compute face normal?
        int v1 = vMapping[i1];
        Vector3 normal = Vector3.Cross(v2 - vertices[v1], v3 - v2).normalized;

        triangles.Add(v1);
        AddPoint(v2, normal, uv2);
        AddPoint(v3, normal, uv3);

        //Compute triangle area
        surfacearea += GetTriangleArea(triangles.Count - 3);
    }

    /// <summary>
    /// Add a completely new triangle to the mesh
    /// </summary>
    public void AddTriangle(Vector3[] points)
    {
        // Compute normal
        Vector3 normal = Vector3.Cross(points[1] - points[0], points[2] - points[1]).normalized;

        for (int i = 0; i < 3; ++i)
        {
            // TODO: Compute uv values for the new triangle?
            AddPoint(points[i], normal, Vector2.zero);
        }

        //Compute triangle area
        surfacearea += GetTriangleArea(triangles.Count - 3);
    }

    public void ContainsKeys(List<int> triangles, int startIdx, bool[] isTrue)
    {
        for (int i = 0; i < 3; ++i)
            isTrue[i] = vMapping.ContainsKey(triangles[startIdx + i]);
    }

    /// <summary>
    /// Add a vertex from the original mesh 
    /// while storing its old index in the dictionary of index mappings
    /// </summary>
    public void AddVertex(List<Vector3> ogVertices, List<Vector3> ogNormals, List<Vector2> ogUvs, int index)
    {
        vMapping[index] = vertices.Count;
        vertices.Add(ogVertices[index]);
        normals.Add(ogNormals[index]);
        uvs.Add(ogUvs[index]);
    }


    private float GetTriangleArea(int i)
    {
        var va = vertices[triangles[i + 2]] - vertices[triangles[i]];
        var vb = vertices[triangles[i + 1]] - vertices[triangles[i]];
        float a = va.magnitude;
        float b = vb.magnitude;
        float gamma = Mathf.Deg2Rad * Vector3.Angle(vb, va);

        return a * b * Mathf.Sin(gamma) / 2;
    }
}

