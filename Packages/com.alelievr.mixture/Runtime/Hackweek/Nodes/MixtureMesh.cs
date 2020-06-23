using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mixture
{
    public class MixtureMesh
    {
        public Mesh         mesh;
        public Matrix4x4    localToWorld = Matrix4x4.identity;

        public MixtureMesh Clone() => new MixtureMesh{ mesh = Object.Instantiate(mesh), localToWorld = localToWorld };
    }
}
