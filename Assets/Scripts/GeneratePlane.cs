using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratePlane : MonoBehaviour {

    private int width = 5;

    [SerializeField]
    public Texture2D[] textures;

    // Use this for initialization
    void Start()
    {
        GameObject obj = new GameObject();
        MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
        MeshCollider meshCollider = obj.AddComponent<MeshCollider>();

        Vector3[] vertices = new Vector3[4 * width * width];
        for (int i = 0; i < width; i++) 
        {
            for (int j = 0; j < width; j++)
            {
                int baseIndex = ((i * width) + j) * 4;
                vertices[baseIndex + 0] = new Vector3(i, 0, j);
                vertices[baseIndex + 1] = new Vector3(i + 1, 0, j);
                vertices[baseIndex + 2] = new Vector3(i, 0, j + 1);
                vertices[baseIndex + 3] = new Vector3(i + 1, 0, j + 1);
            }
        }

        int[] triangles = new int[6 * width * width];
        for (int i = 0; i < width; i++) 
        {
            for (int j = 0; j < width; j++) 
            {
                int baseIndex = ((i * width) + j) * 6;
                int vertexBase = ((i * width) + j) * 4;
                triangles[baseIndex + 0] = vertexBase + 0;
                triangles[baseIndex + 1] = vertexBase + 2;
                triangles[baseIndex + 2] = vertexBase + 1;
                triangles[baseIndex + 3] = vertexBase + 1;
                triangles[baseIndex + 4] = vertexBase + 2;
                triangles[baseIndex + 5] = vertexBase + 3;
            }
        }

        float[] heightMap = new float[(width + 1) * (width + 1)];
        float scale = 1f;
        for (int i = 0; i < width + 1; i++) {
            for (int j = 0; j < width + 1; j++) {
                int baseIndex = ((i * width) + j);
                heightMap[baseIndex] = Mathf.PerlinNoise(i * (scale / width), j * (scale / width));
            }
        }

        Vector3[] uvs = new Vector3[4 * width * width];
        for (int i = 0; i < width; i++) 
        {
            for (int j = 0; j < width; j++)
            {
                //float uvLayer = (i + j) / 2f / width;
                //float uvLayer = Random.value;
                int baseIndex = ((i * width) + j) * 4;
                uvs[baseIndex + 0] = new Vector3((float)i / width, (float)j / width, heightMap[(i * width) + j]);
                uvs[baseIndex + 1] = new Vector3((float)(i + 1) / width, (float)j / width, heightMap[((i + 1) * width) + j]);
                uvs[baseIndex + 2] = new Vector3((float)i / width, (float)(j + 1) / width, heightMap[(i * width) + j + 1]);
                uvs[baseIndex + 3] = new Vector3((float)(i + 1) / width, (float)(j + 1) / width, heightMap[((i + 1) * width) + j + 1]);
            }
        }

        //Vector3[] vertices = {
        //    new Vector3(0, 0, 0),
        //    new Vector3(1, 0, 0),
        //    new Vector3(0, 0, 1),
        //    new Vector3(1, 0, 1)
        //};

        //int[] triangles = {
        //    0, 2, 1, 1, 2, 3
        //};

        //Vector3[] uvs = {
        //    new Vector3(0, 0, 0),
        //    new Vector3(1, 0, 0),
        //    new Vector3(0, 1, 0),
        //    new Vector3(1, 1, 1)
        //};



        Texture2DArray array = new Texture2DArray(textures[0].width, textures[0].height, textures.Length, TextureFormat.RGBA32, true, false);

        array.filterMode = FilterMode.Bilinear;
        array.wrapMode = TextureWrapMode.Repeat;

        for (int i = 0; i < textures.Length; i++) {
            array.SetPixels(textures[i].GetPixels(0), i, 0);
        }


        array.Apply();

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.SetUVs(0, new List<Vector3>(uvs));
        //mesh.uv = uvs;
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        //meshRenderer.material = new Material(Shader.Find("Diffuse"));

        meshRenderer.material = Resources.Load<Material>("Materials/TerrainMaterial");
        //meshRenderer.material = Resources.Load<Material>("Materials/Debug");
        meshRenderer.material.SetTexture("_MyArr", array);

        //meshRenderer.material.SetTexture("_MyArr", Resources.Load<Material>("Materials/Grass").mainTexture);
        //meshRenderer.material.SetTexture("_MainTex", Resources.Load<Material>("Materials/Grass").mainTexture);

        meshCollider.sharedMesh = mesh;

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
