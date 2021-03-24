using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mixture
{
    [System.Serializable]
    public class MixtureMesh
    {
        public Mesh         mesh;
        public Matrix4x4    localToWorld = Matrix4x4.identity;

        public MixtureMesh(Mesh mesh = null) : this(mesh, Matrix4x4.identity) {}

        public MixtureMesh(Mesh mesh, Matrix4x4 localToWorld)
        {
            this.mesh = mesh;
            this.localToWorld = localToWorld;
        }

        public MixtureMesh Clone()
        {
            var clonedMesh = new Mesh{ indexFormat = mesh.indexFormat };
            clonedMesh.vertices = mesh.vertices;
            clonedMesh.triangles = mesh.triangles;
            clonedMesh.normals = mesh.normals;
            clonedMesh.uv = mesh.uv;
            clonedMesh.bounds = mesh.bounds;
            clonedMesh.colors = mesh.colors;
            clonedMesh.colors32 = mesh.colors32;
            return new MixtureMesh(clonedMesh, localToWorld);
        }
    }
}
