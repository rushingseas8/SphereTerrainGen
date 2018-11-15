using System.Collections;
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

}
