using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoordinateLookup
{

    private static Vector3[] icosahedronVertices;
    private static int[] icosahedronTriangles;

    public CoordinateLookup() {
        float s = (float)System.Math.Sqrt((5.0 - System.Math.Sqrt(5.0)) / 10.0);
        float t = (float)System.Math.Sqrt((5.0 + System.Math.Sqrt(5.0)) / 10.0);

        icosahedronVertices = new Vector3[] {
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

        float angle = Mathf.Rad2Deg * Mathf.Acos(1.0f / (2.0f * Mathf.Sin(2f * Mathf.PI / 5f)));
        angle = (180 - angle - angle) / 2f;

        // Rotate the whole icosahedron so the point is upwards. 
        // 
        // The above definition for vertices is convenient to write the points out for, z
        // but generates a mesh that has its vertices offset from center.
        for (int i = 0; i < icosahedronVertices.Length; i++)
        {
            icosahedronVertices[i] = Quaternion.AngleAxis(-angle, Vector3.forward) * icosahedronVertices[i];
        }

        // The triangles of the base icosahedron, grouped in order of lobe stripes.
        // Each group of 4 is a lobe, and each triangle within that is a face.
        icosahedronTriangles = new int[] {
            0, 5, 1,
            1, 5, 9,
            9, 8, 1,
            3, 8, 9,

            0, 11, 5,
            5, 11, 4,
            4, 9, 5,
            3, 9, 4,

            0, 10, 11,
            11, 10, 2,
            2, 4, 11,
            3, 4, 2,

            0, 7, 10,
            10, 7, 6,
            6, 2, 10,
            3, 2, 6,

            0, 1, 7,
            7, 1, 8,
            8, 6, 7,
            3, 6, 8
        };
    }


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

        int meshX = (int)meshCoordinate.x; // width
        int meshY = (int)meshCoordinate.y; // length
        int lobe = (int)meshCoordinate.z;

        int face = (int)((float)meshY / length);

        int triangleBaseIndex = icosahedronTriangles[(lobe * 4) + face];
        Debug.Log("Triangle base index: " + triangleBaseIndex);

        // TODO need "side" info; are we asking for the top or bottom triangle in the square mesh coordinate?


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
