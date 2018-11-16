using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coordinate
{

    private static float s = (float)System.Math.Sqrt((5.0 - System.Math.Sqrt(5.0)) / 10.0);
    private static float t = (float)System.Math.Sqrt((5.0 + System.Math.Sqrt(5.0)) / 10.0);

    private static Vector3[] vertices = {
        new Vector3(-s, t, 0),
        new Vector3(s, t, 0),
        new Vector3(-s, -t, 0),
        new Vector3(s, -t, 0),

        new Vector3(0, -s, t),
        new Vector3(0, s, t),
        new Vector3(0, -s, -t),
        new Vector3(0, s, -t),

        new Vector3(t, 0, -s),
        new Vector3(t, 0, s),
        new Vector3(-t, 0, -s),
        new Vector3(-t, 0, s)
    };

    private static int[] triangles = {
        0, 5, 1,
        1, 5, 9,
        9, 8, 1,
        3, 8, 9,





        0, 11, 5,
        0, 1, 7,
        0, 7, 10,
        0, 10, 11,

        5, 11, 4,
        11, 10, 2,
        10, 7, 6,
        7, 1, 8,

        3, 9, 4,
        3, 4, 2,
        3, 2, 6,
        3, 6, 8,

        4, 9, 5,
        2, 4, 11,
        6, 2, 10,
        8, 6, 7,
    };


    // Reference to the triangles and which face/lobe they describe
    //0, 11, 5, // 1 (face 2)
    //0, 5, 1, // 1 (face 1)
    //0, 1, 7, // 1 (face 5)
    //0, 7, 10, // 1 (face 4)
    //0, 10, 11, // 1 (face 3)

    //1, 5, 9, // 2 (face 1)
    //5, 11, 4, // 2 (face 2)
    //11, 10, 2, // 2 (face 3)
    //10, 7, 6, // 2 (face 4)
    //7, 1, 8, // 2 (face 5)

    //4, 9, 5, // 3 (face 2)
    //2, 4, 11, // 3 (face 3)
    //6, 2, 10, // 3 (face 4)
    //8, 6, 7, // 3 (face 5)
    //9, 8, 1 // 3 (face 1)

    //3, 9, 4, // 4 (face 2)
    //3, 4, 2, // 4 (face 3)
    //3, 2, 6, // 4 (face 4)
    //3, 6, 8, // 4 (face 5)
    //3, 8, 9, // 4 (face 1)




    /*
     * top 3 bits of mesh.x is the lobe (5 values)
     * rest of mesh.x is the width
     * mesh.y is the length.
     * 
     * Grab the correct stripe of the mesh using lobe info. 
     * Grab the face of the stripe using (mesh.y / totalLength).
     * Within the face, find exact triangle using mesh coordinates.
     * Find exact triangle coordinates using face and coordinates (coordinates give relative position)
     * normalize triangle coordinates to obtain spherical coordinate. 
     * 
     * For now, x/y are mesh coordinate and z is lobe info.
     */
    public Vector3 MeshToSphere(Vector3 meshCoordinate, int recursionDepth) 
    {
        int width = (int)Mathf.Pow(2, recursionDepth);
        int length = 4 * width;





        return Vector3.zero;
    }

    /*
     * Spherical coordinate definition:
     * x: angle around (+x is 0, increasing counterclockwise)
     * y: angle up (north pole is 0, increasing clockwise; south pole is 180)
     * z: distance out (distance)
     */
    public Vector2 SphereToMesh(Vector3 sphereCoordiante) 
    {
        return Vector2.zero;
    }
}
