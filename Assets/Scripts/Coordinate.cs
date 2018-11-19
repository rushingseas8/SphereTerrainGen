using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class CoordinateLookup
{

    public static Vector3[] icosahedronVertices;
    public static int[] icosahedronTriangles;

    public CoordinateLookup()
    {
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
            4, 5, 11,
            5, 4, 9,
            3, 9, 4,

            0, 10, 11,
            2, 11, 10,
            11, 2, 4,
            3, 4, 2,

            0, 7, 10,
            6, 10, 7,
            10, 6, 2,
            3, 2, 6,

            0, 1, 7,
            8, 7, 1,
            7, 8, 6,
            3, 6, 8
        };
    }

    public Vector2Int GetMeshCoordinate(int lobe, int width, int length, int triangleIndex)
    {
        Profiler.BeginSample("GetMeshCoordinate");
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

        Profiler.EndSample();
        return toReturn;
    }

    /*
     * top 3 bits of mesh.x is the lobe (5 values)
     * rest of mesh.x is the width
     * top 2 bits of mesh.y is triangle vertex (0, 1, or 2)
     * rest of mesh.y is the length
     * 
     * Grab the correct stripe of the mesh using lobe info. 
     * Grab the face of the stripe using (mesh.y / totalLength).
     * Within the face, find exact triangle using mesh coordinates.
     * Find exact triangle coordinates using face and coordinates (coordinates give relative position)
     * Normalize triangle coordinates to obtain spherical coordinate. 
     */
    public Vector3 MeshToSphere(Vector2Int meshCoordinate, int recursionDepth) 
    {
        Profiler.BeginSample("MeshToSphere");
        int width = (int)Mathf.Pow(2, recursionDepth);
        int length = 4 * width;

        int meshX = meshCoordinate.x; // width
        int meshY = meshCoordinate.y; // length

        int lobe = 7 & meshX >> 29; // Top 3 bits of mesh X is lobe. The "7 &" part removes the sign bit.
        int triangleIndex = 3 & (meshY >> 30); // Top 2 bits of mesh Y is triangle index. The "3 &" part removes the sign bit.

        meshX = (meshX << 3) >> 3; // Clear out the top 3 bits of mesh X
        meshY = (meshY << 2) >> 2; // Clear out the top 2 bits of mesh Y

        // We preserve the parity bit. This is needed because the top and bottom triangle in the mesh respond differently
        // to the "triangleIndex" value. 
        int parity = meshY % 2;

        // Divide the mesh Y by 2. This essentially converts from triangle indices -> rhombus indicies in the mesh.
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

        /*
         * Figure out which face we're on within this given lobe.
         * 
         * If you look at the lobe as a rectangular array, with "meshX" being the height,
         * and "meshY" being the length, the algorithm below does the following.
         * 
         * 1. Determine if we're on the left or right side, by checking if the length
         * is more or less than the midway point. Normally, in triangle indicies, this
         * would be "length / 2"; because we shifted to rhombus indices, this becomes 
         * "length / 4" instead.
         * 
         * 2. Within each side, determine which face we're in. The dividing line is
         * "meshY == meshX", with the parity bit being a tiebreaker. Another way to think
         * of it is by treating every triangle you move towards the dividing line as incrementing
         * a counter up by one. Since any movement along the x axis, y axis, or between parity bits
         * counts this up, we can take the sum "meshX + meshY + parity" to figure out the value.
         * 
         * Note that on the right side, since we know meshY > length / 4, we shift our line up by
         * the value "length / 4" by subtracting that value from meshY.
         * _____________
         * |    /|    /|
         * |   / |   / |
         * |  /  |  /  |
         * | /   | /   |
         * |/____|/____|
         * 
         */
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
            // Normalize the mesh Y to be in the range [0, length / 4] again.
            meshY -= (length / 4);

            // We're on the first half; face 2
            if (meshX + meshY + parity < length / 4f)
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
        // The top-left corner of a rectangle is the zero point. "triangleIndex" defines
        // an offset relative to this zero point. The reason why we can't just pass in the raw
        // meshX and meshY values is because those would otherwise evaluate to being on the wrong face.
        // Thus, by specifying the base and offset like this, we can compute the correct face first.
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

        // Grab the base index of the triangle array, based on the lobe and face.
        int triangleBaseIndex = 3 * ((lobe * 4) + face);

        // Grab the three vertices that make up this face of the icosahedron
        Vector3 v1 = icosahedronVertices[icosahedronTriangles[triangleBaseIndex + 0]];
        Vector3 v2 = icosahedronVertices[icosahedronTriangles[triangleBaseIndex + 1]];
        Vector3 v3 = icosahedronVertices[icosahedronTriangles[triangleBaseIndex + 2]];

        // Here, we have to get relative mesh coordinates (convert them to the range [0,1])
        // This is based on the parity of the face variable (flipped if face is odd)
        float relativeX = (float)meshX / width;
        float relativeY = (float)meshY / width;

        // Using the relative mesh coordinates, we compute the offset within this
        // face of the icosahedron. Because the faces alternate from up and down facing
        // triangles, we reverse the relative coordinates for every odd face (1 or 3).
        Vector3 icosahedronVector;
        if (face % 2 == 0)
        {
            Vector3 basis = v1;

            Vector3 baseX = relativeX * (v2 - v1);
            Vector3 baseY = relativeY * (v3 - v1);

            icosahedronVector = basis + baseX + baseY;
        }
        else {

            Vector3 basis = v1;

            Vector3 baseX = (1 - relativeX) * (v2 - v1);
            Vector3 baseY = (1 - relativeY) * (v3 - v1);

            icosahedronVector = basis + baseX + baseY;
        }

        // Finally, we normalize the vector to project it to the sphere.
        Profiler.EndSample();
        return icosahedronVector.normalized;
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
