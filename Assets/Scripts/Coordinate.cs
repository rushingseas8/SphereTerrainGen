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
    /// <summary>
    /// Converts a mesh coordinate to its respective spherical coordinate.
    /// 
    /// The "mesh coordinate" is defined as the (x, y) position within the mesh, 
    /// when iterating over the triangles of the mesh (rather than the rhombuses).
    /// 
    /// The "sphere coordinate" is where the vertex of the mesh would be moved to
    /// when mapped to a sphere. This requires finding out which face of the icosahedron
    /// the mesh coordinate is in, which makes this a nontrivial computation.
    /// 
    /// </summary>
    /// <returns>The sphere coordinate.</returns>
    /// <param name="lobe">Which lobe of the icosahedron we're in. Valid values are integers in [0, 4].</param>
    /// <param name="meshX">The mesh X coordinate, i.e., width.</param>
    /// <param name="meshY">The mesh Y coordinate, i.e., length.</param>
    /// <param name="triangleIndex">Which triangle vertex we're in. Valid values are in {0, 1, 2}.</param>
    /// <param name="recursionDepth">Recursion depth. TODO pull this from global parameter (GameManager?).</param>
    public Vector3 MeshToSphere(int lobe, int meshX, int meshY, int triangleIndex, int recursionDepth) 
    {
        Profiler.BeginSample("MeshToSphere");

        Profiler.BeginSample("MeshToSphere.Setup");
        int width = (int)Mathf.Pow(2, recursionDepth);
        int length = 4 * width;

        // We preserve the parity bit. This is needed because the top and bottom triangle in the mesh respond differently
        // to the "triangleIndex" value. 
        int parity = meshY % 2;

        // Divide the mesh Y by 2. This essentially converts from triangle indices -> rhombus indicies in the mesh.
        meshY /= 2;
        Profiler.EndSample();

        Profiler.BeginSample("MeshToSphere.Asserts");
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
        Profiler.EndSample();

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

        Profiler.BeginSample("MeshToSphere.FaceComputation");
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
        Profiler.EndSample();

        // Compute the offset due to the parity and triangle index.
        // The top-left corner of a rectangle is the zero point. "triangleIndex" defines
        // an offset relative to this zero point. The reason why we can't just pass in the raw
        // meshX and meshY values is because those would otherwise evaluate to being on the wrong face.
        // Thus, by specifying the base and offset like this, we can compute the correct face first.
        Profiler.BeginSample("MeshToSphere.TriangleOffset");
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
        Profiler.EndSample();

        Profiler.BeginSample("MeshToSphere.VertexComputation");
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

        Profiler.EndSample();

        // Using the relative mesh coordinates, we compute the offset within this
        // face of the icosahedron. Because the faces alternate from up and down facing
        // triangles, we reverse the relative coordinates for every odd face (1 or 3).
        Profiler.BeginSample("MeshToSphere.IcosahedronVector");
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
        Profiler.EndSample();

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

    #region Converters from skew [generation] coordinates to square [mesh array] coordinates

    /// <summary>
    /// Converts Cartesian coordinates to Einstein coordinates.
    /// </summary>
    /// <returns>The resulting U value (maps to Einstein X).</returns>
    /// <param name="x">X.</param>
    /// <param name="z">Z.</param>
    public static float UFromXZ(float x, float z)
    {
        return x - (Mathf.Sqrt(3) / 3f * z);
    }

    /// <summary>
    /// Converts Cartesian coordinates to Einstein coordinates.
    /// </summary>
    /// <returns>The resulting V value (maps to Einstein Z).</returns>
    /// <param name="x">X.</param>
    /// <param name="z">Z.</param>
    public static float VFromXZ(float x, float z)
    {
        return 2f * Mathf.Sqrt(3) / 3f * z;
    }
    #endregion


    #region Converters from square [mesh array] coordinates to skew [generation] coordinates

    /// <summary>
    /// Converts Einstein coordinates to Cartesian coordinates.
    /// </summary>
    /// <returns>The resulting X value.</returns>
    /// <param name="u">U.</param>
    /// <param name="v">V.</param>
    public static float XFromUV(float u, float v)
    {
        return u + (0.5f * v);
    }

    /// <summary>
    /// Converts Einstein coordinates to Cartesian coordinates.
    /// </summary>
    /// <returns>The resulting Y value.</returns>
    /// <param name="u">U.</param>
    /// <param name="v">V.</param>
    public static float ZFromUV(float u, float v)
    {
        return (Mathf.Sqrt(3) / 2f) * v;
    }
    #endregion
}
