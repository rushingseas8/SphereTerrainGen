using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GenerateShape : MonoBehaviour {

    [SerializeField]
    private Shape shape;

    [SerializeField]
    [Range(0.01f, 10000f)]
    private float scale = 1f;

    [SerializeField]
    [Range(0, 1)]
    private float normalizationAmount;

    [SerializeField]
    [Range(0, 6)]
    private int recursionDepth;

    [SerializeField]
    private bool seamless = false;

    [SerializeField]
    private bool shouldUpdate = false;

    private GameObject obj;

    public enum Shape
    {
        CUBE,
        ICOSPHERE,
        RHOMBUS
    }
	
    // TODO standardize all the various methods to use the above parameters
	void Update () {
        if (!shouldUpdate)
        {
            return;
        }

        switch (shape)
        {
            case Shape.CUBE: GenerateCube(); break;
            case Shape.ICOSPHERE: GenerateIcosphere(); break;
            case Shape.RHOMBUS: GenerateRhombus(); break;
            default: GenerateIcosphere(); break;
        }
	}

    private void GenerateCube()
    {
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
            Generator.Subdivide(mesh);
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

    private void GenerateIcosphere()
    {
        float s = (float)System.Math.Sqrt((5.0 - System.Math.Sqrt(5.0)) / 10.0);
        float t = (float)System.Math.Sqrt((5.0 + System.Math.Sqrt(5.0)) / 10.0);

        List<Vector3> vertices = new List<Vector3>();

        vertices.Add(new Vector3(-s, t, 0));
        vertices.Add(new Vector3(s, t, 0));
        vertices.Add(new Vector3(-s, -t, 0));
        vertices.Add(new Vector3(s, -t, 0));

        vertices.Add(new Vector3(0, -s, t));
        vertices.Add(new Vector3(0, s, t));
        vertices.Add(new Vector3(0, -s, -t));
        vertices.Add(new Vector3(0, s, -t));

        vertices.Add(new Vector3(t, 0, -s));
        vertices.Add(new Vector3(t, 0, s));
        vertices.Add(new Vector3(-t, 0, -s));
        vertices.Add(new Vector3(-t, 0, s));

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

        if (obj != null)
        {
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
            Generator.SubdivideNotShared(mesh);
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

    private void GenerateRhombus()
    {
        Vector3[] vertices = {
            new Vector3(0, 0, 0) * scale,
            new Vector3(1, 0, 0) * scale,
            new Vector3(0.5f, 0, 1) * scale,

            new Vector3(0.5f, 0, 1) * scale,
            new Vector3(1.5f, 0, 1) * scale,
            new Vector3(1, 0, 0) * scale
        };

        int[] triangles = {
            0, 2, 1, 3, 4, 5
        };

        if (obj != null)
        {
            DestroyImmediate(obj);
        }

        Mesh rawMesh = Generator.GenerateMesh(vertices, triangles);
        for (int i = 0; i < recursionDepth; i++)
        {
            Generator.SubdivideNotShared(rawMesh);
        }
        vertices = rawMesh.vertices;
        triangles = rawMesh.triangles;

        Vector2[] uvs = new Vector2[vertices.Length];

        if (seamless)
        {
            // UVs for the seamless texture (original + modified side-by-side)
            for (int i = 0; i < triangles.Length; i += 6)
            {
                uvs[triangles[i]] = new Vector2(0, 0);
                uvs[triangles[i + 1]] = new Vector2(0.5f, 0);
                uvs[triangles[i + 2]] = new Vector2(0.25f, 1);


                uvs[triangles[i + 3]] = new Vector2(1f, 0);
                uvs[triangles[i + 4]] = new Vector2(0.5f, 0);
                uvs[triangles[i + 5]] = new Vector2(0.75f, 1);
            }

        }
        else
        {
            // UVs for basic texture (has hexagonal repeating pattern)
            for (int i = 0; i < triangles.Length; i += 3)
            {
                uvs[triangles[i]] = new Vector2(0, 0);
                uvs[triangles[i + 1]] = new Vector2(1, 0);
                uvs[triangles[i + 2]] = new Vector2(0.5f, 1);
            }
        }

        rawMesh.uv = uvs;
        rawMesh.RecalculateNormals();

        obj = Generator.GenerateObject(rawMesh, Vector3.up);

        if (seamless)
        {
            obj.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/GrassSeamless");
        }
        else
        {
            obj.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Grass");
        }
    }
}
