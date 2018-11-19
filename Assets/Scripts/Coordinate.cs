using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoordinateLookup
{

    public static Vector3[] icosahedronVertices;
    public static int[] icosahedronTriangles;

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
            //icosahedronVertices[i] = icosahedronVertices[i].normalized;
        }

        // The triangles of the base icosahedron, grouped in order of lobe stripes.
        // Each group of 4 is a lobe, and each triangle within that is a face.
        // The triangles are ordered in such a way that we can easily map mesh coordinates to their location in the icosahedron.
        icosahedronTriangles = new int[] {
            0, 5, 1,
            9, 1, 5,
            1, 9, 8,
            3, 8, 9,

            0, 11, 5,
            //5, 11, 4,
            4, 5, 11,
            //4, 9, 5,
            5, 4, 9,
            3, 9, 4,

            0, 10, 11,
            //11, 10, 2,
            2, 11, 10,
            //2, 4, 11,
            11, 2, 4,
            3, 4, 2,

            0, 7, 10,
            //10, 7, 6,
            6, 10, 7,
            //6, 2, 10,
            10, 6, 2,
            3, 2, 6,

            0, 1, 7,
            //7, 1, 8,
            8, 7, 1,
            //8, 6, 7,
            7, 8, 6,
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

    public Vector2Int GetMeshCoordinate(int lobe, int width, int length, int triangleIndex)
    {
        Vector2Int toReturn = new Vector2Int((lobe << 29) | width, (triangleIndex << 30) | length);

        int meshX = toReturn.x; // width
        int meshY = toReturn.y; // length

        int _lobe = 7 & meshX >> 29; // Top 3 bits of mesh X is lobe. The "7 &" part removes the sign bit.
        int _triangleIndex = 3 & (meshY >> 30); // Top 2 bits of mesh Y is triangle index. The "3 &" part removes the sign bit.

        meshX = (meshX << 3) >> 3; // Clear out the top 3 bits of mesh X
        meshY = (meshY << 2) >> 2; // Clear out the top 2 bits of mesh Y

        Debug.Assert(lobe == _lobe);
        Debug.Assert(triangleIndex == _triangleIndex);
        Debug.Assert(width == meshX);
        Debug.Assert(length == meshY);

        return toReturn;
    }

    /*
     * top 3 bits of mesh.x is the lobe (5 values)
     * rest of mesh.x is the width
     * top bit of mesh.y is triangle (false is lower, true is upper)
     * rest of mesh.y is the length
     * 
     * Grab the correct stripe of the mesh using lobe info. 
     * Grab the face of the stripe using (mesh.y / totalLength).
     * Within the face, find exact triangle using mesh coordinates.
     * Find exact triangle coordinates using face and coordinates (coordinates give relative position)
     * normalize triangle coordinates to obtain spherical coordinate. 
     * 
     * For now, x/y are mesh coordinate and z is lobe info.
     */
    public Vector3 MeshToSphere(Vector2Int meshCoordinate, int recursionDepth) 
    {
        int width = (int)Mathf.Pow(2, recursionDepth);
        int length = 4 * width;

        int meshX = meshCoordinate.x; // width
        int meshY = meshCoordinate.y; // length

        int lobe = 7 & meshX >> 29; // Top 3 bits of mesh X is lobe. The "7 &" part removes the sign bit.
        int triangleIndex = 3 & (meshY >> 30); // Top 2 bits of mesh Y is triangle index. The "3 &" part removes the sign bit.

        meshX = (meshX << 3) >> 3; // Clear out the top 3 bits of mesh X
        meshY = (meshY << 2) >> 2; // Clear out the top 2 bits of mesh Y

        int parity = meshY % 2;
        meshY /= 2;

        // Make some assertions here
        if (meshX > width)
        {
            Debug.LogError("Invalid mesh X value in MeshToSphere: " + meshX + " when width is " + width);
            return Vector3.zero;
        }

        if (meshY > length)
        {
            Debug.LogError("Invalid mesh Y value in MeshToSphere: " + meshY + " when length is " + length);
            return Vector3.zero;
        }

        if (lobe >= 5)
        {
            Debug.LogError("Invalid lobe value in MeshToSphere: " + lobe + " when lobe should be in range [0, 4].");
            return Vector3.zero;
        }

        if (triangleIndex > 3) {
            Debug.LogError("Invalid triangle index in MeshToSphere: " + triangleIndex + " when index should be in range [0, 2].");
        }

        //Debug.Log("Mesh x: " + meshX + " mesh y: " + meshY + " lobe: " + lobe + " triangle index: " + triangleIndex + " Length: " + length);

        int face;

        // Face 0 or 1
        if (meshY < (length / 4f))
        {
            // We're on the first half; face 0
            if (meshX + meshY + parity < length / 4f)
            {
                face = 0;
            }
            // Second half; face 1
            else
            {
                face = 1;
            }
        }
        // Face 2 or 3
        else
        {
            // We're on the first half; face 2
            if (meshX + meshY + parity < length / 2f)
            {
                face = 2;
            }
            // Second half; face 3
            else
            {
                face = 3;
            }
        }

        // Compute the offset due to the parity and triangle index.
        if (parity == 0) {
            if (triangleIndex == 1)
            {
                meshX += 1;
            }
            if (triangleIndex == 2)
            {
                meshY += 1;
            }
        } else {
            if (triangleIndex == 0) {
                meshX += 1;
            }
            if (triangleIndex == 1) {
                meshY += 1;
            }
            if (triangleIndex == 2) {
                meshX += 1;
                meshY += 1;
            }
        }

        if (face >= 2) {
            meshY -= (length / 4);
        }

        //face = 0;
        //Debug.Log("Face: " + face);

        //int moduloFace = (face / 2) * 2;
        //if (face == 1) { face = 0; }
        //if (face == 2 || face == 3) { face = 1; }

        int triangleBaseIndex = 3 * ((lobe * 4) + face);
        //Debug.Log("Triangle base index: " + triangleBaseIndex);

        Vector3 v1 = icosahedronVertices[icosahedronTriangles[triangleBaseIndex + 0]];
        Vector3 v2 = icosahedronVertices[icosahedronTriangles[triangleBaseIndex + 1]];
        Vector3 v3 = icosahedronVertices[icosahedronTriangles[triangleBaseIndex + 2]];

        // Here, we have to get relative mesh coordinates (convert them to the range [0,1])
        // This is based on the parity of the face variable (flipped if face is odd)
        float relativeX = (float)meshX / width;
        float relativeY = (float)meshY / width;

        //Debug.Log("Returning: " + (Vector3.Lerp(v1, v2, relativeX) + Vector3.Lerp(v1, v3, relativeY)).normalized);
        //Debug.Log("Relative x: " + relativeX + " and relative y: " + relativeY);

        //Debug.Assert(!(face == 2 || face == 3));

        Debug.Log("Face: " + face + " V1: " + v1 + " V2: " + v2 + " V3: " + v3);


        if (face % 2 == 0)
        {
            Vector3 basis = v1;

            Vector3 baseX = relativeX * (v2 - v1);
            Vector3 baseY = relativeY * (v3 - v1);

            return basis + baseX + baseY;
            //return (Vector3.Lerp(v1, v2, relativeX) + Vector3.Lerp(v1, v3, relativeY)) / 2f;
            //return Vector3.Lerp(v1, v2, relativeX) + relativeY * Vector3.right;
        }
        else {

            Vector3 basis = v1;

            Vector3 baseX = (1 - relativeX) * (v2 - v1);
            Vector3 baseY = (1 - relativeY) * (v3 - v1);

            return basis + baseX + baseY;

            //return (Vector3.Lerp(v1, v3, relativeX) + Vector3.Lerp(v2, v3, relativeY));
            //return (Vector3.Lerp(v1, v3, relativeX) + Vector3.Lerp(v2, v3, relativeY)) / 2f;

            // Need to normalize relative X and Y to be 0 at their respective 0 points.
            //return (Vector3.Lerp(v1, v2, relativeX) + Vector3.Lerp(v1, v3, relativeY));
        }

        //return (Vector3.Lerp(v1, v2, relativeX) + Vector3.Lerp(v1, v3, relativeY));

        //return (Vector3.Lerp(v1, v3, relativeX) + Vector3.Lerp(v2, v3, relativeY));
        //return (Vector3.Lerp(v1, v3, relativeX) + Vector3.Lerp(v2, v3, relativeY));
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
