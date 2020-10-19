using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainCubes : MonoBehaviour
{
    public GameObject cube;

    public int sizeX;
    public int sizeY;

    public float scale = 0.1f;
    public float heightMultiplier = 10;

    void Start()
    {
        for (int x = 0; x < sizeX; x++)
            for (int y = 0; y < sizeX; y++)
            {
                var obj = GameObject.Instantiate(cube, new Vector3(x * 2, 0, y * 2), Quaternion.identity);
                obj.transform.localScale = new Vector3(2, Mathf.PerlinNoise(x * scale, y * scale) * heightMultiplier, 2);
            }
    }
}
