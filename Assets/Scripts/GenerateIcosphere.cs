using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GenerateIcosphere : MonoBehaviour {

    [SerializeField]
    [Range(0, 1)]
    public float normalizationAmount;

    [SerializeField]
    [Range(0, 6)]
    public int recursionDepth;

    public bool shouldUpdate = false;

    private GameObject obj;

    // Use this for initialization
    void Start () {

    }
	
	// Update is called once per frame
	void Update () {
        if (!shouldUpdate) {
            return;
        }

        float s = (float) System.Math.Sqrt((5.0 - System.Math.Sqrt(5.0)) / 10.0);
        float t = (float) System.Math.Sqrt((5.0 + System.Math.Sqrt(5.0)) / 10.0);

        List<Vector3> vertices = new List<Vector3>();

        vertices.Add(new Vector3(-s,  t, 0));
        vertices.Add(new Vector3( s,  t, 0));
        vertices.Add(new Vector3(-s, -t, 0));
        vertices.Add(new Vector3( s, -t, 0));

        vertices.Add(new Vector3(0, -s,  t));
        vertices.Add(new Vector3(0,  s,  t));
        vertices.Add(new Vector3(0, -s, -t));
        vertices.Add(new Vector3(0,  s, -t));

        vertices.Add(new Vector3( t, 0, -s));
        vertices.Add(new Vector3( t, 0,  s));
        vertices.Add(new Vector3(-t, 0, -s));
        vertices.Add(new Vector3(-t, 0,  s));

        List<int> triangles = new List<int>() {
            0, 11, 5,
            0, 5, 1,
            0, 1, 7,
            0, 7, 10,
            0, 10, 11,

            1, 5, 9,
            5, 11, 4,
            11, 10, 2,
            10, 7, 6,
            7, 1, 8,

            3, 9, 4,
            3, 4, 2,
            3, 2, 6,
            3, 6, 8,
            3, 8, 9,

            4, 9, 5,
            2, 4, 11,
            6, 2, 10,
            8, 6, 7,
            9, 8, 1
        };

        List<Vector2> uvs = new List<Vector2> {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0.5f, 1),
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0.5f, 1),
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0.5f, 1),
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0.5f, 1)
        };

        /*
        List<Vector3> normals = new List<Vector3>(vertices.Count);
        for (int i = 0; i < vertices.Count; i++) 
        {
            normals.Add(vertices[i].normalized);
        }
        */

        if (obj != null) {
            GameObject.DestroyImmediate(obj);
        }
        obj = new GameObject();

        MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
        SphereCollider sphereCollider = obj.AddComponent<SphereCollider>();
        sphereCollider.radius = 1f;

        Mesh mesh = new Mesh
        {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray(),
            uv = uvs.ToArray()
        };

        for (int recur = 0; recur < recursionDepth; recur++) 
        {
            Subdivide(mesh);
        }
        vertices = new List<Vector3>(mesh.vertices);


        // Normalization pass to make it spherical
        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 v = vertices[i];
            Vector3 n = vertices[i].normalized;

            vertices[i] = ((normalizationAmount * n) + ((1 - normalizationAmount) * v));
        }
        mesh.vertices = vertices.ToArray();

        //mesh.normals = normals.ToArray();

        meshFilter.mesh = mesh;
        meshRenderer.material = Resources.Load<Material>("Materials/Grass");
	}

    private static int GetVertex(int i1, int i2, ref List<Vector3> vertices, ref Dictionary<uint, int> newVertices)
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

    private static void Subdivide(Mesh mesh)
    {
        Dictionary<uint, int> newVertices = new Dictionary<uint, int>();
        List<Vector3> vertices = new List<Vector3>(mesh.vertices);

        List<int> newTriangles = new List<int>();
        List<int> triangles = new List<int>(mesh.triangles);

        List<Vector2> uvs = new List<Vector2>();

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

        for (int i = 0; i < vertices.Count; i += 6)
        {
            uvs.AddRange(new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1) });
            uvs.AddRange(new Vector2[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1) });
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        Debug.Log("Vertices: " + vertices.Count);
        mesh.triangles = newTriangles.ToArray();
        Debug.Log("Triangles: " + triangles.Count);
        mesh.uv = uvs.ToArray();
        Debug.Log("UVs: " + uvs.Count);
    }
}
