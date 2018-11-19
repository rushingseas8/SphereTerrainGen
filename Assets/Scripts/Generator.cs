﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Generator {

    public static Mesh GenerateMesh(Vector3[] vertices, int[] triangles, Vector2[] uvs = null, Vector3[] normals = null) 
    {

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        if (uvs != null)
        {
            mesh.uv = uvs;
        }

        if (normals != null)
        {
            mesh.normals = normals;
        }
        else
        {
            mesh.RecalculateNormals();
        }

        return mesh;
    }

    public static GameObject GenerateObject(Mesh mesh, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion)) 
    {
        GameObject obj = new GameObject();

        MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
        MeshCollider meshCollider = obj.AddComponent<MeshCollider>();

        meshFilter.mesh = mesh;
        //meshRenderer.material = new Material(Shader.Find("Diffuse"));
        //meshRenderer.material = Resources.Load<Material>("Materials/Debug");
        meshRenderer.material = Resources.Load<Material>("Materials/Grass");
        //meshRenderer.material = Resources.Load<Material>("Materials/GrassSeamless");
        meshCollider.sharedMesh = mesh;

        obj.transform.position = position;
        obj.transform.rotation = rotation;

        return obj;
    }

    public static int GetVertex(int i1, int i2, ref List<Vector3> vertices, ref Dictionary<uint, int> newVertices)
    {
        // We have to test both directions since the edge
        // could be reversed in another triangle
        uint t1 = ((uint)i1 << 16) | (uint)i2;
        uint t2 = ((uint)i2 << 16) | (uint)i1;

        if (newVertices.ContainsKey(t2))
        {
            return newVertices[t2];
        }

        if (newVertices.ContainsKey(t1))
        {
            return newVertices[t1];
        }

        // Generate vertex and keep track of it in the dictionary.
        int newIndex = vertices.Count;
        newVertices.Add(t1, newIndex);

        // Calculate new vertex, and add it to the original reference.
        vertices.Add((vertices[i1] + vertices[i2]) * 0.5f);

        return newIndex;
    }

    public static void Subdivide(Mesh mesh)
    {
        Dictionary<uint, int> newVertices = new Dictionary<uint, int>();
        List<Vector3> vertices = new List<Vector3>(mesh.vertices);

        List<int> newTriangles = new List<int>();
        List<int> triangles = new List<int>(mesh.triangles);

        //List<Vector2> uvs = new List<Vector2>();

        for (int i = 0; i < triangles.Count; i += 3)
        {
            int i1 = triangles[i + 0];
            int i2 = triangles[i + 1];
            int i3 = triangles[i + 2];

            int a = GetVertex(i1, i2, ref vertices, ref newVertices);
            int b = GetVertex(i2, i3, ref vertices, ref newVertices);
            int c = GetVertex(i3, i1, ref vertices, ref newVertices);

            newTriangles.AddRange(new int[] { i1, a, c });
            newTriangles.AddRange(new int[] { i2, b, a });
            newTriangles.AddRange(new int[] { i3, c, b });
            newTriangles.AddRange(new int[] { a, b, c });

            //uvs.AddRange(new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1) });
        }

        //for (int i = 0; i < vertices.Count; i += 6)
        //{
        //    uvs.AddRange(new Vector2[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1) });
        //    uvs.AddRange(new Vector2[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1) });
        //}

        //mesh.Clear();
        mesh.vertices = vertices.ToArray();
        Debug.Log("Vertices: " + vertices.Count);
        mesh.triangles = newTriangles.ToArray();
        Debug.Log("Triangles: " + triangles.Count);
        //mesh.uv = uvs.ToArray();
        //Debug.Log("UVs: " + uvs.Count);
    }

    public static void SubdivideNotShared(Mesh mesh)
    {
        List<Vector3> newVertices = new List<Vector3>();
        List<Vector3> vertices = new List<Vector3>(mesh.vertices);

        List<int> newTriangles = new List<int>();
        List<int> triangles = new List<int>(mesh.triangles);

        int triangleCount = 0;
        for (int i = 0; i < triangles.Count; i += 3)
        {
            int i1 = triangles[i + 0];
            int i2 = triangles[i + 1];
            int i3 = triangles[i + 2];

            Vector3 v1 = vertices[i1];
            Vector3 v2 = vertices[i2];
            Vector3 v3 = vertices[i3];

            Vector3 a = (v1 + v2) * 0.5f;
            Vector3 b = (v2 + v3) * 0.5f;
            Vector3 c = (v3 + v1) * 0.5f;

            newVertices.AddRange(new Vector3[] { v1, a, c });
            newVertices.AddRange(new Vector3[] { v2, b, a });
            newVertices.AddRange(new Vector3[] { v3, c, b });
            newVertices.AddRange(new Vector3[] { a , b, c });

            for (int count = 0; count < 12; count++)
            {
                newTriangles.Add(triangleCount++);
            }
        }

        mesh.vertices = newVertices.ToArray();
        mesh.triangles = newTriangles.ToArray();

        Debug.Log("Vertices: " + newVertices.Count);
        Debug.Log("Triangles: " + newTriangles.Count);
    }

    // Like SubdivideNotSharedOrdered, but knows that an icosahedron has five
    // rhombus-like meshes inside of it, and subdivides properly.
    public static void SubdivideIcosahedron(Mesh mesh, int recursionDepth)
    {
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        Vector3[] newVertices = new Vector3[mesh.vertices.Length * 4];
        int[] newTriangles = new int[mesh.triangles.Length * 4];

        SubdivisionHelper(ref vertices, ref triangles, ref newVertices, ref newTriangles, recursionDepth, true);

        mesh.vertices = newVertices;
        mesh.triangles = newTriangles;
    }

    /// <summary>
    /// Subdivides the given mesh, being very careful to preserve the order of the
    /// vertices and tirangles. As such, it takes an already ordered mesh (with the
    /// vertices in essentially a rectangular array, and the triangles as described
    /// within the algorithm), and returns a mesh with 4x as many triangles and vertices,
    /// maintaining the same ordering.
    /// </summary>
    /// <param name="mesh">Mesh.</param>
    public static void SubdivideNotSharedOrdered(Mesh mesh, int recursionDepth) 
    {
        Debug.Log("Input mesh: " + mesh.vertices.Length + " vertices and " + mesh.triangles.Length + " triangles.");
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        Vector3[] newVertices = new Vector3[mesh.vertices.Length * 4];
        int[] newTriangles = new int[mesh.triangles.Length * 4];

        SubdivisionHelper(ref vertices, ref triangles, ref newVertices, ref newTriangles, recursionDepth);

        // Assign the new vertices and triangles
        mesh.vertices = newVertices;
        mesh.triangles = newTriangles;

        Debug.Log("Output mesh vertices: " + newVertices.Length);
        Debug.Log("Output mesh triangles: " + newTriangles.Length);
    }

    private static void SubdivisionHelper(ref Vector3[] vertices, ref int[] triangles, ref Vector3[] newVertices, ref int[] newTriangles, int recursionDepth, bool icosahedron=false)
    {
        int lobes = icosahedron ? 5 : 1;

        for (int lobe = 0; lobe < lobes; lobe++)
        {
            int lobeOffset = (int)((float)lobe / lobes * vertices.Length);

            int splits = (int)Mathf.Pow(2, recursionDepth);

            for (int split = 0; split < splits; split++)
            {
                int startIndex = lobeOffset + (int)(((float)split / splits) * (vertices.Length / lobes));
                int endIndex = lobeOffset + (int)((split + 1f) / splits * vertices.Length / lobes);

                int bottomRowIndex = 4 * startIndex;
                int topRowIndex = 4 * (startIndex + ((endIndex - startIndex) / 2));

                for (int i = startIndex; i < endIndex; i += 6)
                {
                    /*
                     * 
                     * The original 2 triangles we are splitting are shown here:
                     * 
                     *     v2/v4         v6
                     *        ____________
                     *       /\          /
                     *      /  \        /
                     *     /    \      /
                     *    /      \    /
                     *   /        \  /
                     *  /__________\/
                     * v1        v3/v5
                     * 
                     * The first triangle will always add 3 new triangles to the bottom
                     * row, and 1 to the top row. The second will do the opposite.
                     * 
                     * This is shown below:
                     * 
                     *     v2/v4    f    v6
                     *        ____________
                     *       /\    /\    /
                     *      /  \ b/  \  /
                     *   a /____\/____\/ e
                     *    /\    /\d   / 
                     *   /  \  /  \  /
                     *  /____\/____\/
                     * v1     c   v3/v5
                     * 
                     * This algorithm splits 2 triangles at a time to consistently add
                     * 4 sets of vertices and triangles to the array at once. 
                     * 
                     * We have to be very careful to make sure the ordering of the vertices 
                     * is the same as it was when we started, to keep the pattern recursively
                     * similar.
                     */

                    #region First triangle
                    // Grab the indices of the first triangle
                    int i1 = triangles[i + 0];
                    int i2 = triangles[i + 1];
                    int i3 = triangles[i + 2];

                    // Grab the vertices of the first triangle
                    Vector3 v1 = vertices[i1];
                    Vector3 v2 = vertices[i2];
                    Vector3 v3 = vertices[i3];

                    // Compute the midpoints (vertices of the subdivided triangles)
                    Vector3 a = (v1 + v2) * 0.5f;
                    Vector3 b = (v2 + v3) * 0.5f;
                    Vector3 c = (v3 + v1) * 0.5f;

                    // Add the vertices
                    newVertices[bottomRowIndex + 0] = v1;
                    newVertices[bottomRowIndex + 1] = a;
                    newVertices[bottomRowIndex + 2] = c;

                    newVertices[bottomRowIndex + 3] = a;
                    newVertices[bottomRowIndex + 4] = c;
                    newVertices[bottomRowIndex + 5] = b;

                    newVertices[bottomRowIndex + 6] = c;
                    newVertices[bottomRowIndex + 7] = b;
                    newVertices[bottomRowIndex + 8] = v3;

                    newVertices[topRowIndex + 0] = a;
                    newVertices[topRowIndex + 1] = v2;
                    newVertices[topRowIndex + 2] = b;
                    #endregion

                    #region Second triangle
                    // Grab the indices of the second triangle (note we swap to match triangle order)
                    int i4 = triangles[i + 3];
                    int i5 = triangles[i + 5];
                    int i6 = triangles[i + 4];

                    // Grab the vertices of the second triangle
                    Vector3 v4 = vertices[i4];
                    Vector3 v5 = vertices[i5];
                    Vector3 v6 = vertices[i6];

                    // Compute the midpoints
                    Vector3 d = (v4 + v5) * 0.5f;
                    Vector3 e = (v5 + v6) * 0.5f;
                    Vector3 f = (v6 + v4) * 0.5f;

                    // Add the vertices
                    newVertices[topRowIndex + 3] = v4;
                    newVertices[topRowIndex + 4] = d;
                    newVertices[topRowIndex + 5] = f;

                    newVertices[topRowIndex + 6] = d;
                    newVertices[topRowIndex + 7] = f;
                    newVertices[topRowIndex + 8] = e;

                    newVertices[topRowIndex + 9] = f;
                    newVertices[topRowIndex + 10] = e;
                    newVertices[topRowIndex + 11] = v6;

                    newVertices[bottomRowIndex + 9] = d;
                    newVertices[bottomRowIndex + 10] = v5;
                    newVertices[bottomRowIndex + 11] = e;
                    #endregion


                    // Add the triangles. Same as vertices, but in the order 0,1,2,3,5,4,
                    // since the last triangle needs to be flipped to face the right way.
                    for (int k = 0; k < 2; k++)
                    {
                        newTriangles[bottomRowIndex + (6 * k) + 0] = bottomRowIndex + (6 * k) + 0;
                        newTriangles[bottomRowIndex + (6 * k) + 1] = bottomRowIndex + (6 * k) + 1;
                        newTriangles[bottomRowIndex + (6 * k) + 2] = bottomRowIndex + (6 * k) + 2;

                        newTriangles[bottomRowIndex + (6 * k) + 3] = bottomRowIndex + (6 * k) + 3;
                        newTriangles[bottomRowIndex + (6 * k) + 4] = bottomRowIndex + (6 * k) + 5; // These are
                        newTriangles[bottomRowIndex + (6 * k) + 5] = bottomRowIndex + (6 * k) + 4; // swapped

                        newTriangles[topRowIndex + (6 * k) + 0] = topRowIndex + (6 * k) + 0;
                        newTriangles[topRowIndex + (6 * k) + 1] = topRowIndex + (6 * k) + 1;
                        newTriangles[topRowIndex + (6 * k) + 2] = topRowIndex + (6 * k) + 2;

                        newTriangles[topRowIndex + (6 * k) + 3] = topRowIndex + (6 * k) + 3;
                        newTriangles[topRowIndex + (6 * k) + 4] = topRowIndex + (6 * k) + 5; // These are
                        newTriangles[topRowIndex + (6 * k) + 5] = topRowIndex + (6 * k) + 4; // swapped
                    }

                    // Finally, we increment the indices of the top and bottom row by 12 each,
                    // since we added 12 vertices to both of them. Total is 24 vertices, which is
                    // exactly 4 times the 6 vertices we used for input.
                    bottomRowIndex += 12;
                    topRowIndex += 12;
                }
            }
        }
    }

    public static void SortByBottomLeft(ref Vector3[] vertices, ref int[] triangles)
    {
        Vector3[] toReturn = new Vector3[vertices.Length];

        Debug.Assert(vertices.Length == triangles.Length);

        FaceStorage[] faces = new FaceStorage[vertices.Length / 3];
        for (int i = 0; i < vertices.Length; i += 3)
        {
            FaceStorage face = new FaceStorage();

            face.vertices = new Vector3[] {
                vertices[i + 0], vertices[i + 1], vertices[i + 2]
            };
            face.triangles = new int[] {
                triangles[i + 0], triangles[i + 1], triangles[i + 2]
            };

            faces[i / 3] = face;
        }

        System.Array.Sort(faces);

        for (int i = 0; i < faces.Length; i++)
        {
            FaceStorage face = faces[i];

            vertices[(3 * i) + 0] = face.vertices[0];
            vertices[(3 * i) + 1] = face.vertices[1];
            vertices[(3 * i) + 2] = face.vertices[2];

            triangles[(3 * i) + 0] = face.triangles[0];
            triangles[(3 * i) + 1] = face.triangles[1];
            triangles[(3 * i) + 2] = face.triangles[2];
        }
    }

    private class FaceStorage : System.IComparable<FaceStorage>
    {
        public Vector3[] vertices;
        public int[] triangles;

        public int CompareTo(FaceStorage other)
        {
            // Check y first
            if (other.vertices[0].y < vertices[0].y)
            {
                return 1;
            }

            if (other.vertices[0].y > vertices[0].y)
            {
                return -1;
            }

            // Check x if y values are equal
            if (other.vertices[0].x < vertices[0].x)
            {
                return 1;
            }

            if (other.vertices[0].x > vertices[0].x)
            {
                return 1;
            }

            return 0;
        }
    }

}
