using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GenerateIcosphere2 : MonoBehaviour {

    [SerializeField]
    [Range(0, 8)]
    public int recursionDepth;

    [SerializeField]
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

        int width = (int)Mathf.Pow(2, recursionDepth);
        int length = 4 * (int)Mathf.Pow(2, recursionDepth);

        int scaleFactor = (int)Mathf.Pow(2, recursionDepth);

        /*
         * Four cases, based on the initial one of the four faces we're in.
         * 
         * Condition 1: l < length / 2
         *  Condition 2: w + l + parity < length / 2
         *      -> Face 0
         *  else
         *      -> Face 1
         * else
         *  Condition 2: w + (l - (length / 2)) + parity < length / 2
         *      -> Face 2
         *  else
         *      -> Face 3
         * 
         * TODO: it would be better to iterate using this algorithm, but it requires
         * a lot of tricky any annoying math.
         */

        Vector3[] vertices = new Vector3[3 * width * length];
        // We iterate over the rhombuses, not the triangles.
        for (int w = 0; w < width; w++) 
        {
            for (int l = 0; l < length / 2; l++) 
            {
                //int baseIndex = 6 * ((w * length) + l);
                //int baseIndex = (w * 3 * 4 * width) + (6 * l);
                int rhombusBaseIndex = 6 * (w * length) + l;

                // Parity bit.
                for (int p = 0; p < 2; p++)
                {
                    int vertexBaseIndex = 3 * ((w * length) + l + p);

                    if (l < length / 2)
                    {
                        if (w + l + p < length / 2) {
                            Vector3 v1 = CoordinateLookup.icosahedronVertices[0];
                            Vector3 v2 = CoordinateLookup.icosahedronVertices[1];
                            Vector3 v3 = CoordinateLookup.icosahedronVertices[2];

                            float widthScale = (float)w / width;
                            float lengthScale = (float)l / (length / 2);

                            vertices[vertexBaseIndex + 0] = Vector3.Lerp(v1, v2, widthScale);
                        }
                    }
                }
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


        Debug.Log("Vertices length: " + vertices.Length);
        Debug.Log("Triangles length: " + triangles.Length);
        if (obj != null) 
        {
            DestroyImmediate(obj);
        }

        obj = Generator.GenerateObject(Generator.GenerateMesh(vertices, triangles));
	}
}
