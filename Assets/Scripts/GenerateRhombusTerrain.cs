using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GenerateRhombusTerrain : MonoBehaviour {

    public float scale = 1f;

    public bool shouldUpdate = false;

    public bool seamless = false;

    [Range(0, 8)]
    public int recursionDepth;

    private GameObject obj;

    // Use this for initialization
    void Start () {

	}
	
	// Update is called once per frame
	void Update () {
        if (!shouldUpdate)
        {
            return;
        }
        Debug.Log("Update");

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
