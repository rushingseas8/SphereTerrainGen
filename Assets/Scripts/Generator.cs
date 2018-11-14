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
        meshRenderer.material = new Material(Shader.Find("Diffuse"));
        //meshRenderer.material = Resources.Load<Material>("Materials/Grass");
        meshCollider.sharedMesh = mesh;

        obj.transform.position = position;
        obj.transform.rotation = rotation;

        return obj;
    }
}
