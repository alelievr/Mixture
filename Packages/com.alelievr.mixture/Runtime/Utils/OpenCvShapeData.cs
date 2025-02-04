using UnityEngine;

namespace Mixture
{
    [System.Serializable]
    public struct OpenCvShapeData
    {
        public int id;
        public Rect boundingBox;
        public Vector2 center;
        public int area;
    }
}