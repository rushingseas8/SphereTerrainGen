using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GenerateCube : MonoBehaviour {

    [SerializeField]
    [Range(0,1)]
    public float normalizationAmount;

    [SerializeField]
    [Range(0, 5)]
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
        Debug.Log("Update called");

        float halfUnit = 0.5f;
        List<Vector3> vertices = new List<Vector3>
        {
            // +x
            new Vector3(halfUnit, -halfUnit,  halfUnit),
            new Vector3(halfUnit, -halfUnit, -halfUnit),
            new Vector3(halfUnit,  halfUnit,  halfUnit),
            new Vector3(halfUnit,  halfUnit, -halfUnit),

            // -x
            new Vector3(-halfUnit, -halfUnit, -halfUnit),
            new Vector3(-halfUnit, -halfUnit,  halfUnit),
            new Vector3(-halfUnit,  halfUnit, -halfUnit),
            new Vector3(-halfUnit,  halfUnit,  halfUnit),

            // +y
            new Vector3(-halfUnit, halfUnit, -halfUnit),
            new Vector3(-halfUnit, halfUnit,  halfUnit),
            new Vector3(halfUnit,  halfUnit, -halfUnit),
            new Vector3(halfUnit,  halfUnit,  halfUnit),

            // -y
            new Vector3(-halfUnit, -halfUnit,  halfUnit),
            new Vector3(-halfUnit, -halfUnit, -halfUnit),
            new Vector3(halfUnit,  -halfUnit,  halfUnit),
            new Vector3(halfUnit,  -halfUnit, -halfUnit),

            // +z
            new Vector3(-halfUnit, -halfUnit, halfUnit),
            new Vector3( halfUnit, -halfUnit, halfUnit),
            new Vector3(-halfUnit,  halfUnit, halfUnit),
            new Vector3( halfUnit,  halfUnit, halfUnit),

            // -z
            new Vector3( halfUnit, -halfUnit, -halfUnit),
            new Vector3(-halfUnit, -halfUnit, -halfUnit),
            new Vector3( halfUnit,  halfUnit, -halfUnit),
            new Vector3(-halfUnit,  halfUnit, -halfUnit)
        };

        List<int> triangles = new List<int>();

        for (int face = 0; face < 6; face++) 
        {
            int vertexBase = face * 4;
            triangles.Add(vertexBase + 0);
            triangles.Add(vertexBase + 1);
            triangles.Add(vertexBase + 2);

            triangles.Add(vertexBase + 3);
            triangles.Add(vertexBase + 2);
            triangles.Add(vertexBase + 1);
        }

        // Make a mesh from the initial vertices and triangles
        Mesh mesh = new Mesh
        {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray()
        };

        // Subdivide the mesh as needed
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
        mesh.RecalculateNormals();
        //Debug.Log("Normalized " + vertices.Count )

        // Render the mesh into a GameObject
        if (obj != null)
        {
            GameObject.DestroyImmediate(obj);
        }

        obj = new GameObject();

        MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
        MeshCollider meshCollider = obj.AddComponent<MeshCollider>();

        meshFilter.mesh = mesh;
        //meshRenderer.material = new Material(Shader.Find("Diffuse"));
        meshRenderer.material = Resources.Load<Material>("Materials/Grass");
        meshCollider.sharedMesh = mesh;
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
            newTriangles.AddRange(new int[] {  a, b, c });

            //uvs.AddRange(new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1) });
        }

        for (int i = 0; i < vertices.Count; i += 6) {
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
