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

        //int width = (int)Mathf.Pow(2, recursionDepth);
        int width = 1;
        int length = 4 * (int)Mathf.Pow(2, recursionDepth);

        int scaleFactor = (int)Mathf.Pow(2, recursionDepth);

        Vector3[] vertices = new Vector3[3 * width * length];
        for (int w = 0; w < width; w++) 
        {
            for (int l = 0; l < length / 2; l++) 
            {
                //int baseIndex = 6 * ((w * length) + l);
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

        /*
        for (int i = 0; i < vertices.Length; i += 6) 
        {
            //float angle = Mathf.PI * ((float)i / vertices.Length) / 2.0f;

            Vector3 baseVector = vertices[i];
            for (int j = 0; j < 6; j++)
            {
                Vector3 input = vertices[i + j];
                Vector3 difference = input - baseVector;


                float angle = Mathf.PI * ((float)(i + j) / vertices.Length);

                //vertices[i + j] = new Vector3(0, Mathf.Cos(angle), Mathf.Sin(angle));
                vertices[i + j] = (new Vector3(0, 1, 0) + input).normalized;
            }
        }
        */

        /*
        for (int i = 0; i < vertices.Length; i++)
        {
            float zDistance = Mathf.PI * vertices[i].z / 2.5f;
            vertices[i] = new Vector3(Mathf.Cos(Mathf.PI * ((float)i / vertices.Length) * (Mathf.Deg2Rad * 9f)), Mathf.Cos(zDistance), Mathf.Sin(zDistance));
        }
        */

        /*
         * Algorithm for computing positions of triangles.
         * (non-subdivided)
         * Position first triangle correctly.
         * for i from 1 to n - 1:
         *   Grab the edge between i and i-1
         *   Rotate using dihedral angle (138.19 degrees) [likely 180 - 138.19 for 
         *      this approach] about the edge
         * 
         * (subdivided)
         * Position initial row of triangles correctly
         * As above, but new dihedral angle is generated (compute?)
         */

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
