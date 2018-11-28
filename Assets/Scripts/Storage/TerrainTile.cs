using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;


/// <summary>
/// Storage for a single generation tile in memory. In short, this is a rhombus
/// containing two separate triangles.
/// </summary>
public class TerrainTile
{
    private Mesh lowerMesh;
    private Mesh upperMesh;

    private GameObject lower;
    private GameObject upper;

    //private const int recursionDepth = 7;
    private int recursionDepth;

    public TerrainTile(int worldX, int worldZ, int LOD = 7)
    {
        if (LOD > 7)
        {
            LOD = 7;
        }
        this.recursionDepth = LOD;

        GameManager.GetInstance().StartCoroutine(CreateAsync(worldX, worldZ));
    }

    public void Destroy()
    {
        GameObject.Destroy(lower);
        GameObject.Destroy(upper);
    }

    private IEnumerator CreateAsync(int worldX, int worldZ)
    {
        // Make the objects upfront, so if they're deleted, we can break out early.
        lower = new GameObject();
        upper = new GameObject();

        // Do the generation call to make the meshes
        yield return GenerateAsync(worldX, worldZ);

        // Compute the spawn location of the terrain.
        float genX = 128 * CoordinateLookup.XFromUV(worldX, worldZ);
        float genZ = 128 * CoordinateLookup.ZFromUV(worldX, worldZ);

        // At some point, generate the objects asyncronously. 
        yield return Generator.GenerateObjectAsync(lower, lowerMesh, new Vector3(genX, 0, genZ));
        if (lower != null)
        {
            lower.transform.localScale = 128 * Vector3.one;
            lower.name = "Lower x=" + worldX + " z=" + worldZ;
        }

        yield return Generator.GenerateObjectAsync(upper, upperMesh, new Vector3(genX, 0, genZ));
        if (upper != null)
        {
            upper.name = "Upper x=" + worldX + " z=" + worldZ;
            upper.transform.localScale = 128 * Vector3.one;
        }
    }

    private IEnumerator GenerateAsync(int worldX, int worldZ)
    {
        #region Perlin noise generation
        float width = 1 << recursionDepth;
        float[] noise = new float[(int)((width + 1) * (width + 1))];

        for (int z = 0; z < width + 1; z++)
        {
            //Profiler.BeginSample("Perlin noise generation");
            for (int x = 0; x < width + 1; x++)
            {
                // Use the noise generator to get a height value.
                noise[(int)((z * (width + 1)) + x)] = NoiseGenerator.GetNoise(worldX, worldZ, x / width, z / width);
                //noise[(int)((i * (width + 1)) + j)] = 0f;
            }

            // If a mesh is high resolution, it's worth breaking up generation
            // into smaller pieces. Else, just do it all at once.
            if (recursionDepth > 2)
            {
                //Profiler.EndSample();

                // If the GameObject is deleted, stop loading it in.
                if (lower == null || upper == null)
                {
                    Debug.Log("Stopped generation between noise loops.");
                    yield break;
                }

                yield return null;
            }
        }


        // If the GameObject is deleted, stop loading it in.
        if (lower == null || upper == null)
        {
            Debug.Log("Stopped generation after noise generation.");
            yield break;
        }
        //Profiler.EndSample();
        yield return null;
        #endregion

        #region Vertex and triangle generation

        // Arrays for storing vertices and triangles for the upper and lower mesh.
        Vector3[] upperVertices = new Vector3[(int)(3 * width * width)];
        int[] upperTriangles = new int[(int)(3 * width * width)];

        Vector3[] lowerVertices = new Vector3[(int)(3 * width * width)];
        int[] lowerTriangles = new int[(int)(3 * width * width)];

        // Counters for where we are in each mesh.
        int lowerCount = 0;
        int upperCount = 0;

        float heightScale = Mathf.Sqrt(3) / 2f;
        for (int z = 0; z < width; z++)
        {
            //Profiler.BeginSample("Vertex and triangle generation");
            float offset = z / width * 0.5f;
            float offsetAndHalf = (z + 1) / width * 0.5f;

            // todo: Factor out conditionals by being clever about the loop.
            // for now it's fine, but later this might make the code cleaner.

            for (int x = 0; x < width; x++)
            {
                int noiseBase = (int)((z * (width + 1)) + x);

                // bottom half, left triangle
                if (x + z < width)
                {
                    lowerVertices[lowerCount + 0] = new Vector3(offset + ((x + 0) / width), noise[noiseBase], heightScale * (z + 0) / width);
                    lowerVertices[lowerCount + 1] = new Vector3(offsetAndHalf + ((x + 0) / width), noise[noiseBase + (int)(width + 1)], heightScale * (z + 1) / width);
                    lowerVertices[lowerCount + 2] = new Vector3(offset + ((x + 1) / width), noise[noiseBase + 1], heightScale * (z + 0) / width);

                    lowerTriangles[lowerCount + 0] = lowerCount + 0;
                    lowerTriangles[lowerCount + 1] = lowerCount + 1;
                    lowerTriangles[lowerCount + 2] = lowerCount + 2;

                    lowerCount += 3;
                }
                // top half, left triangle
                else
                {
                    upperVertices[upperCount + 0] = new Vector3(offset + ((x + 0) / width), noise[noiseBase], heightScale * (z + 0) / width);
                    upperVertices[upperCount + 1] = new Vector3(offsetAndHalf + ((x + 0) / width), noise[noiseBase + (int)(width + 1)], heightScale * (z + 1) / width);
                    upperVertices[upperCount + 2] = new Vector3(offset + ((x + 1) / width), noise[noiseBase + 1], heightScale * (z + 0) / width);

                    upperTriangles[upperCount + 0] = upperCount + 0;
                    upperTriangles[upperCount + 1] = upperCount + 1;
                    upperTriangles[upperCount + 2] = upperCount + 2;

                    upperCount += 3;
                }

                // bottom half, right triangle. This triangle has flipped indices.
                if (x + z < width - 1)
                {

                    lowerVertices[lowerCount + 0] = new Vector3(offsetAndHalf + ((x + 0) / width), noise[noiseBase + (int)(width + 1)], heightScale * (z + 1) / width);
                    lowerVertices[lowerCount + 1] = new Vector3(offset + ((x + 1) / width), noise[noiseBase + 1], heightScale * (z + 0) / width);
                    lowerVertices[lowerCount + 2] = new Vector3(offsetAndHalf + ((x + 1) / width), noise[noiseBase + (int)width + 2], heightScale * (z + 1) / width);

                    lowerTriangles[lowerCount + 0] = lowerCount + 0;
                    lowerTriangles[lowerCount + 1] = lowerCount + 2;
                    lowerTriangles[lowerCount + 2] = lowerCount + 1;

                    lowerCount += 3;
                }
                // top half, right triangle. This triangle has flipped indices.
                else
                {
                    upperVertices[upperCount + 0] = new Vector3(offsetAndHalf + ((x + 0) / width), noise[noiseBase + (int)(width + 1)], heightScale * (z + 1) / width);
                    upperVertices[upperCount + 1] = new Vector3(offset + ((x + 1) / width), noise[noiseBase + 1], heightScale * (z + 0) / width);
                    upperVertices[upperCount + 2] = new Vector3(offsetAndHalf + ((x + 1) / width), noise[noiseBase + (int)width + 2], heightScale * (z + 1) / width);

                    upperTriangles[upperCount + 0] = upperCount + 0;
                    upperTriangles[upperCount + 1] = upperCount + 2;
                    upperTriangles[upperCount + 2] = upperCount + 1;

                    upperCount += 3;
                }
            }
            //Profiler.EndSample();
            // This is a fairly quick operation, so we don't really need to yield return between loop iterations.
            yield return null;
        }

        // If the GameObject is deleted, stop loading it in.
        if (lower == null || upper == null)
        {
            Debug.Log("Stopped generation after vertices/triangles.");
            yield break;
        }

        yield return null;
        #endregion


        #region Mesh generation
        //Profiler.BeginSample("Mesh generation");
        //lowerMesh = Generator.GenerateMesh(lowerVertices, lowerTriangles);
        //upperMesh = Generator.GenerateMesh(upperVertices, upperTriangles);

        lowerMesh = new Mesh();
        upperMesh = new Mesh();

        yield return Generator.GenerateMeshAsync(lowerMesh, lowerVertices, lowerTriangles);
        yield return Generator.GenerateMeshAsync(upperMesh, upperVertices, upperTriangles);

        //Profiler.EndSample();
        #endregion
    }

    private void GenerateMeshes(int worldX, int worldZ)
    {
        Profiler.BeginSample("Perlin noise generation");
        float width = 1 << recursionDepth;
        float[] noise = new float[(int)((width + 1) * (width + 1))];

        for (int z = 0; z < width + 1; z++)
        {
            for (int x = 0; x < width + 1; x++)
            {
                // Use the noise generator to get a height value.
                noise[(int)((z * (width + 1)) + x)] = NoiseGenerator.GetNoise(worldX, worldZ, x / width, z / width);
                //noise[(int)((i * (width + 1)) + j)] = 0f;
            }
        }
        Profiler.EndSample();

        Profiler.BeginSample("Vertex and triangle generation");
        Vector3[] upperVertices = new Vector3[(int)(3 * width * width)];
        int[] upperTriangles = new int[(int)(3 * width * width)];

        Vector3[] lowerVertices = new Vector3[(int)(3 * width * width)];
        int[] lowerTriangles = new int[(int)(3 * width * width)];

        int lowerCount = 0;
        int upperCount = 0;

        //Debug.Log("Max size: " + upperVertices.Length);
        // 
        float heightScale = Mathf.Sqrt(3) / 2f;
        for (int z = 0; z < width; z++)
        {
            float offset = z / width * 0.5f;
            float offsetAndHalf = (z + 1) / width * 0.5f;

            // todo: Factor out conditionals by being clever about the loop.
            // for now it's fine, but later this might make the code cleaner.

            for (int x = 0; x < width; x++)
            {
                int noiseBase = (int)((z * (width + 1)) + x);

                // bottom half, left triangle
                if (x + z < width)
                {
                    lowerVertices[lowerCount + 0] = new Vector3(offset +        ((x + 0) / width), noise[noiseBase],                    heightScale * (z + 0) / width);
                    lowerVertices[lowerCount + 1] = new Vector3(offsetAndHalf + ((x + 0) / width), noise[noiseBase + (int)(width + 1)], heightScale * (z + 1) / width);
                    lowerVertices[lowerCount + 2] = new Vector3(offset +        ((x + 1) / width), noise[noiseBase + 1],                heightScale * (z + 0) / width);

                    lowerTriangles[lowerCount + 0] = lowerCount + 0;
                    lowerTriangles[lowerCount + 1] = lowerCount + 1;
                    lowerTriangles[lowerCount + 2] = lowerCount + 2;

                    lowerCount += 3;
                }
                // top half, left triangle
                else
                {
                    upperVertices[upperCount + 0] = new Vector3(offset +        ((x + 0) / width), noise[noiseBase],                    heightScale * (z + 0) / width);
                    upperVertices[upperCount + 1] = new Vector3(offsetAndHalf + ((x + 0) / width), noise[noiseBase + (int)(width + 1)], heightScale * (z + 1) / width);
                    upperVertices[upperCount + 2] = new Vector3(offset +        ((x + 1) / width), noise[noiseBase + 1],                heightScale * (z + 0) / width);

                    upperTriangles[upperCount + 0] = upperCount + 0;
                    upperTriangles[upperCount + 1] = upperCount + 1;
                    upperTriangles[upperCount + 2] = upperCount + 2;

                    upperCount += 3;
                }

                // bottom half, right triangle. This triangle has flipped indices.
                if (x + z < width - 1)
                {

                    lowerVertices[lowerCount + 0] = new Vector3(offsetAndHalf + ((x + 0) / width), noise[noiseBase + (int)(width + 1)], heightScale * (z + 1) / width);
                    lowerVertices[lowerCount + 1] = new Vector3(offset +        ((x + 1) / width), noise[noiseBase + 1],                heightScale * (z + 0) / width);
                    lowerVertices[lowerCount + 2] = new Vector3(offsetAndHalf + ((x + 1) / width), noise[noiseBase + (int)width + 2],   heightScale * (z + 1) / width);

                    lowerTriangles[lowerCount + 0] = lowerCount + 0;
                    lowerTriangles[lowerCount + 1] = lowerCount + 2;
                    lowerTriangles[lowerCount + 2] = lowerCount + 1;

                    lowerCount += 3;
                }
                // top half, right triangle. This triangle has flipped indices.
                else
                {
                    upperVertices[upperCount + 0] = new Vector3(offsetAndHalf + ((x + 0) / width), noise[noiseBase + (int)(width + 1)], heightScale * (z + 1) / width);
                    upperVertices[upperCount + 1] = new Vector3(offset +        ((x + 1) / width), noise[noiseBase + 1],                heightScale * (z + 0) / width);
                    upperVertices[upperCount + 2] = new Vector3(offsetAndHalf + ((x + 1) / width), noise[noiseBase + (int)width + 2],   heightScale * (z + 1) / width);

                    upperTriangles[upperCount + 0] = upperCount + 0;
                    upperTriangles[upperCount + 1] = upperCount + 2;
                    upperTriangles[upperCount + 2] = upperCount + 1;

                    upperCount += 3;
                }
            }
        }
        Profiler.EndSample();

        Profiler.BeginSample("Mesh generation");
        lowerMesh = Generator.GenerateMesh(lowerVertices, lowerTriangles);
        upperMesh = Generator.GenerateMesh(upperVertices, upperTriangles);
        Profiler.EndSample();
    }

    private Vector3 GetPosition(int worldX, int worldZ, float xOffset, float zOffset, float noise) {
        Vector3 v1 = GameManager.Coordinate.MeshToSphereUnnormalized(0, worldX, worldZ, 0, 2);
        Vector3 v2 = GameManager.Coordinate.MeshToSphereUnnormalized(0, worldX, worldZ, 1, 2);
        Vector3 v3 = GameManager.Coordinate.MeshToSphereUnnormalized(0, worldX, worldZ, 2, 2);

        Vector3 baseX = xOffset * (v2 - v1);
        Vector3 baseY = zOffset * (v3 - v1);

        Vector3 toReturn = v1 + baseX + baseY;

        toReturn = toReturn.normalized;
        //toReturn = toReturn + ((noise - 0.5f) / 4f * toReturn);

        //Debug.Log(toReturn);
        return toReturn;
    }
}