using System.Collections.Generic;
using UnityEngine;

public static class MeshUtils
{

    /// <summary>
    /// Find center of polygon by averaging vertices
    /// </summary>
    public static Vector3 FindCenter(List<Vector3> pairs)
    {
        Vector3 center = Vector3.zero;
        int count = 0;

        for (int i = 0; i < pairs.Count; i += 2)
        {
            center += pairs[i];
            count++;
        }

        return center / count;
    }


    /// <summary>
    /// Reorder a list of pairs of vectors (one dimension list where i and i + 1 defines a line segment)
    /// So that it forms a closed polygon 
    /// </summary>
    public static void ReorderList(List<Vector3> pairs)
    {
        int nbFaces = 0;
        int faceStart = 0;
        int i = 0;

        while (i < pairs.Count)
        {
            // Find next adjacent edge
            for (int j = i + 2; j < pairs.Count; j += 2)
            {
                if (pairs[j] == pairs[i + 1])
                {
                    // Put j at i+2
                    SwitchPairs(pairs, i + 2, j);
                    break;
                }
            }


            if (i + 3 >= pairs.Count)
            {
                // Why does this happen?
                // Debug.Log("Huh?");
                break;
            }
            else if (pairs[i + 3] == pairs[faceStart])
            {
                // A face is complete.
                nbFaces++;
                i += 4;
                faceStart = i;
            }
            else
            {
                i += 2;
            }
        }
    }

    private static void SwitchPairs(List<Vector3> pairs, int pos1, int pos2)
    {
        if (pos1 == pos2) return;

        Vector3 temp1 = pairs[pos1];
        Vector3 temp2 = pairs[pos1 + 1];
        pairs[pos1] = pairs[pos2];
        pairs[pos1 + 1] = pairs[pos2 + 1];
        pairs[pos2] = temp1;
        pairs[pos2 + 1] = temp2;
    }

}
