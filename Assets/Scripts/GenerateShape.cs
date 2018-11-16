//using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GenerateShape : MonoBehaviour {

    [SerializeField]
    [Tooltip("Which geometric shape to generate.")]
    private Shape shape = Shape.ICOSPHERE;

    [SerializeField]
    [Tooltip("How big this object will be. Measured in base 10. -2 corresponds to 0.01 units, and 4 is 10,000 units.")]
    [Range(-2f, 4f)]
    private float logScale = 0f;

    [SerializeField]
    [Tooltip("How spherical this mesh will be. 0 is the original mesh, 1 is the mesh mapped to a sphere. Doesn't work for Rhombus.")]
    [Range(0, 1)]
    private float normalizationAmount = 1f;

    [SerializeField]
    [Tooltip("How many subdivisions we perform on the mesh.")]
    [Range(0, 6)]
    private int recursionDepth = 1;

    [SerializeField]
    [Tooltip("If true, try to use the seamless texture. Currently only works for Rhombus.")]
    private bool seamless = false;

    [SerializeField]
    [Tooltip("If true, the mesh will share vertices between triangles. This breaks UVs, but allows for bigger meshes.")]
    private bool connectedTriangles = false;

    [SerializeField]
    [Tooltip("If true, render the mesh using a debug UV texture. Increasing X is red, increasing Y is green.")]
    private bool debugUvs = false;

    [SerializeField]
    [Tooltip("If true, any editor action will update the mesh.")]
    private bool shouldUpdate = false;

    [SerializeField]
    [Range(0f, 256f)]
    private float terrainGenerationScaleX = 1f;

    [SerializeField]
    [Range(0f, 256f)]
    private float terrainGenerationScaleZ = 1f;

    [SerializeField]
    [Range(1, 8)]
    private int octaves = 1;

    private GameObject obj;

    public enum Shape
    {
        CUBE,
        ICOSPHERE,
        RHOMBUS,
        TERRAIN
    }
	
	void Update () {
        if (!shouldUpdate)
        {
            return;
        }

        Mesh mesh = new Mesh();
        switch (shape)
        {
            case Shape.CUBE: GenerateCube(mesh); break;
            case Shape.ICOSPHERE: GenerateIcosphere(mesh); break;
            case Shape.RHOMBUS: GenerateRhombus(mesh); break;
            case Shape.TERRAIN: GenerateTerrain(mesh); break;
            default: GenerateIcosphere(mesh); break;
        }

        if (obj != null) 
        {
            DestroyImmediate(obj);
        }

        obj = Generator.GenerateObject(mesh);
        obj.transform.localScale = Mathf.Pow(10f, logScale) * Vector3.one;

        if (debugUvs)
        {
            obj.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Debug");
        }
        else
        {
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

    private void GenerateCube(Mesh mesh)
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
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        // Subdivide the mesh as needed
        for (int recur = 0; recur < recursionDepth; recur++)
        {
            if (connectedTriangles)
            {
                Generator.Subdivide(mesh);
            }
            else
            {
                Generator.SubdivideNotSharedOrdered(mesh);
            }
        }
        vertices = new List<Vector3>(mesh.vertices);
        triangles = new List<int>(mesh.triangles);

        // UV pass
        Vector2[] uvs = new Vector2[vertices.Count];
        for (int i = 0; i < triangles.Count; i += 6) {
            uvs[triangles[i + 0]] = new Vector2(0, 1);
            uvs[triangles[i + 1]] = new Vector2(1, 1);
            uvs[triangles[i + 2]] = new Vector2(0, 0);

            uvs[triangles[i + 3]] = new Vector2(1, 0);
            uvs[triangles[i + 4]] = new Vector2(0, 0);
            uvs[triangles[i + 5]] = new Vector2(1, 1);
        }
        mesh.uv = uvs;

        // Normalization pass to make it spherical
        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 v = vertices[i];
            Vector3 n = vertices[i].normalized;

            vertices[i] = ((normalizationAmount * n) + ((1 - normalizationAmount) * v));
        }
        mesh.vertices = vertices.ToArray();
        mesh.RecalculateNormals();
    }

    private void GenerateIcosphere(Mesh mesh)
    {
        float s = (float)System.Math.Sqrt((5.0 - System.Math.Sqrt(5.0)) / 10.0);
        float t = (float)System.Math.Sqrt((5.0 + System.Math.Sqrt(5.0)) / 10.0);

        // Generate the non-shared vertices of the icosahedron. 
        // We're using the 4 perpendicular quads method to enumerate the vertices, and generate them
        // via "lobes": a stripe starting at the top vertex, and moving through the faces in a 
        // down-left-down-left pattern. This matches how we store the faces in memory.
        Vector3[] vertices = {
            #region Lobe 1: towards +x, +z
            new Vector3(-s, t, 0),
            new Vector3(0, s, t),
            new Vector3(s, t, 0),

            new Vector3(s, t, 0),
            new Vector3(t, 0, s), // Swap
            new Vector3(0, s, t), // Swap

            new Vector3(t, 0, s),
            new Vector3(t, 0, -s),
            new Vector3(s, t, 0),

            new Vector3(s, -t, 0),
            new Vector3(t, 0, s),  // Swap
            new Vector3(t, 0, -s), // Swap
            #endregion

            /*
            #region Lobe 2: towards +z (slightly towards -x)
            new Vector3(-s, t, 0),
            new Vector3(-t, 0, s),
            new Vector3(0, s, t),

            new Vector3(0, s, t),
            new Vector3(0, -s, t), // Swap
            new Vector3(-t, 0, s), // Swap

            new Vector3(s, -t, 0),
            new Vector3(t, 0, s),
            new Vector3(0, -s, t),

            new Vector3(0, -s, t),
            new Vector3(0, s, t), // Swap
            new Vector3(t, 0, s), // Swap
            #endregion

            #region Lobe 3: towards -x
            new Vector3(-s, t, 0),
            new Vector3(-t, 0, -s),
            new Vector3(-t, 0, s),

            new Vector3(-t, 0, s),
            new Vector3(-s, -t, 0),
            new Vector3(-t, 0, -s),

            new Vector3(-s, -t, 0),
            new Vector3(0, -s, t),
            new Vector3(-t, 0, s),

            new Vector3(s, -t, 0),
            new Vector3(-s, -t, 0),
            new Vector3(0, -s, t),
            #endregion

            #region Lobe 4: towards -z (slightly towards -x)
            new Vector3(-s, t, 0),
            new Vector3(0, s, -t),
            new Vector3(-t, 0, -s),

            new Vector3(-t, 0, -s),
            new Vector3(0, -s, -t), // Swapped
            new Vector3(0, s, -t),  // Swapped

            new Vector3(0, -s, -t),
            new Vector3(-s, -t, 0),
            new Vector3(-t, 0, -s),

            new Vector3(s, -t, 0),
            new Vector3(0, -s, -t), // Swapped
            new Vector3(-s, -t, 0), // Swapped
            #endregion

            #region Lobe 5: towards +x, -z
            new Vector3(-s, t, 0),
            new Vector3(s, t, 0),
            new Vector3(0, s, -t),

            new Vector3(0, s, -t),
            new Vector3(t, 0, -s), // Swapped
            new Vector3(s, t, 0),  // Swapped

            new Vector3(t, 0, -s),
            new Vector3(0, -s, -t),
            new Vector3(0, s, -t),

            new Vector3(s, -t, 0),
            new Vector3(t, 0, -s),  // Swapped
            new Vector3(0, -s, -t), // Swapped

            #endregion
            */
        };

        float angle = Mathf.Rad2Deg * Mathf.Acos(1.0f / (2.0f * Mathf.Sin(2f * Mathf.PI / 5f)));
        angle = (180 - angle - angle) / 2f;

        // Rotate the whole icosahedron so the point is upwards. 
        // 
        // The above definition for vertices is convenient to write the points out for, z
        // but generates a mesh that has its vertices offset from center.
        for (int i = 0; i < vertices.Length; i++)
        {
            //Quaternion.AngleAxis(-90, Vector3.forward) * Quaternion.AngleAxis(angle, Vector3.forward) *
            vertices[i] = Quaternion.AngleAxis(-angle, Vector3.forward) * vertices[i];
        }

        // Triangles are generated with the last two vertices flipped, so every face faces outwards.
        int[] triangles = new int[12];
        for (int i = 0; i < triangles.Length; i += 6)
        {
            triangles[i + 0] = i + 0;
            triangles[i + 1] = i + 1;
            triangles[i + 2] = i + 2;

            triangles[i + 3] = i + 3;
            triangles[i + 4] = i + 5;
            triangles[i + 5] = i + 4;
        }

        // Assign the vertices and triangles to the mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        // Recursively subdivide the mesh
        for (int recur = 0; recur < recursionDepth; recur++)
        {
            if (connectedTriangles)
            {
                Generator.Subdivide(mesh);
            }
            else
            {
                Generator.SubdivideNotSharedOrdered(mesh);
            }
        }
        // Update the vertices and triangles, since we subdivided
        vertices = mesh.vertices;
        triangles = mesh.triangles;

        // UV pass
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < triangles.Length; i += 6)
        {
            uvs[triangles[i + 0]] = new Vector2(0, 1);
            uvs[triangles[i + 1]] = new Vector2(1, 1);
            uvs[triangles[i + 2]] = new Vector2(0, 0);

            uvs[triangles[i + 3]] = new Vector2(1, 0);
            uvs[triangles[i + 4]] = new Vector2(0, 0);
            uvs[triangles[i + 5]] = new Vector2(1, 1);
        }
        mesh.uv = uvs;

        // Normalization pass to make it spherical
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 v = vertices[i];
            Vector3 n = vertices[i].normalized;

            vertices[i] = ((normalizationAmount * n) + ((1 - normalizationAmount) * v));
        }
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }

    private void GenerateRhombus(Mesh mesh)
    {
        Vector3[] vertices = {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(0.5f, 0, 1),

            new Vector3(0.5f, 0, 1),
            new Vector3(1.5f, 0, 1),
            new Vector3(1, 0, 0)
        };

        int[] triangles = {
            0, 2, 1, 3, 4, 5
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        // Subdivide
        for (int i = 0; i < recursionDepth; i++)
        {
            if (connectedTriangles)
            {
                Generator.Subdivide(mesh);
            }
            else
            {
                Generator.SubdivideNotSharedOrdered(mesh);
            }
        }
        vertices = mesh.vertices;
        triangles = mesh.triangles;

        // Generate UVs (changes depending on if we're using the seamless or not)
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

        mesh.uv = uvs;

        mesh.RecalculateNormals();
    }

    private void GenerateTerrain(Mesh mesh)
    {
        int width = (int)Mathf.Pow(2, recursionDepth);
        int length = 4 * (int)Mathf.Pow(2, recursionDepth);

        int scaleFactor = (int)Mathf.Pow(2, recursionDepth);

        Vector3[] vertices = new Vector3[3 * width * length];
        for (int w = 0; w < width; w++)
        {
            for (int l = 0; l < length / 2; l++)
            {
                int baseIndex = (w * 3 * 4 * width) + (6 * l);

                /*
                 *   2/3       4
                 *     ________
                 *    /\      /
                 *   /  \    / 
                 *  /    \  /
                 * /______\/
                 * 0      1/5
                 * 
                 * Note that each triangle is equilateral with side length 1.
                 */

                float rowOffset = (0.5f * w);
                //float rowOffset = 0f;

                vertices[baseIndex + 0] = new Vector3(w, 0, rowOffset + l) / scaleFactor;
                vertices[baseIndex + 1] = new Vector3(w, 0, rowOffset + l + 1f) / scaleFactor;
                vertices[baseIndex + 2] = new Vector3(w + 1f, 0, rowOffset + l + 0.5f) / scaleFactor;

                vertices[baseIndex + 3] = new Vector3(w + 1f, 0, rowOffset + l + 0.5f) / scaleFactor;
                vertices[baseIndex + 4] = new Vector3(w + 1f, 0, rowOffset + l + 1.5f) / scaleFactor;
                vertices[baseIndex + 5] = new Vector3(w, 0, rowOffset + l + 1f) / scaleFactor;
            }
        }

        int[] triangles = new int[3 * width * length];
        for (int w = 0; w < width; w++)
        {
            for (int l = 0; l < length / 2; l++)
            {
                int baseIndex = (w * 3 * 4 * width) + (6 * l);

                triangles[baseIndex + 0] = baseIndex + 0;
                triangles[baseIndex + 1] = baseIndex + 1;
                triangles[baseIndex + 2] = baseIndex + 2;

                triangles[baseIndex + 3] = baseIndex + 3;
                triangles[baseIndex + 4] = baseIndex + 5;
                triangles[baseIndex + 5] = baseIndex + 4;
            }
        }

        // Generate UVs (changes depending on if we're using the seamless or not)
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

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        // Do a noise pass, adding Perlin stuff
        Perlin perlin = new Perlin(291, 842, octaves);
        //int width = (int)Mathf.Pow(2, recursionDepth + 1) + 1;

        int wp1 = width + 1;
        float[] noise = new float[(width + 1) * ((width * 2) + 1)];
        float count = 0;
        for (int i = 0; i < width + 1; i++)
        {
            for (int j = 0; j < (width * 2) + 1; j++)
            {
                int baseIndex = (i * ((width * 2) + 1)) + j;
                //int baseIndex = (i * wp1) + j;

                float u = i;
                float v = j;

                float x = perlin.XFromUV(u, v);
                float y = perlin.YFromUV(u, v);

                noise[baseIndex] = perlin.GetNormalizedValue((x / terrainGenerationScaleX), (y / terrainGenerationScaleZ));
            }
        }

        // Assign noise to each vertex's height value
        for (int w = 0; w < width; w++)
        {
            for (int l = 0; l < length / 2; l++)
            {
                // Base position for each vertex
                int i = (w * 3 * 4 * width) + (6 * l);

                // Base position for corresponding square in the noise array
                int baseIndex = (w * ((width * 2) + 1)) + l;

                // Next row in the noise array
                int nextRow = (width * 2) + 1;

                vertices[i + 0] = vertices[i + 0] + new Vector3(0, noise[baseIndex + 0], 0);
                vertices[i + 1] = vertices[i + 1] + new Vector3(0, noise[baseIndex + 1], 0);
                vertices[i + 2] = vertices[i + 2] + new Vector3(0, noise[baseIndex + nextRow], 0);

                //vertices[i + 3] = vertices[i + 3] + new Vector3(0, noise[baseIndex + wp1], 0);
                //vertices[i + 4] = vertices[i + 4] + new Vector3(0, noise[baseIndex + 1 + wp1], 0);
                //vertices[i + 5] = vertices[i + 5] + new Vector3(0, noise[baseIndex + 1], 0);

                vertices[i + 3] = vertices[i + 3] + new Vector3(0, noise[baseIndex + nextRow], 0);
                vertices[i + 4] = vertices[i + 4] + new Vector3(0, noise[baseIndex + 1 + nextRow], 0);
                vertices[i + 5] = vertices[i + 5] + new Vector3(0, noise[baseIndex + 1], 0);
            }
        }

        mesh.vertices = vertices;
        Debug.Log("Vertices: " + vertices.Length);
        Debug.Log("Triangles: " + triangles.Length / 3);

        mesh.RecalculateNormals();
    }
}
