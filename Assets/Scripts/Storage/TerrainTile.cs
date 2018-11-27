using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Storage for a single generation tile in memory. In short, this is a rhombus
/// containing two separate triangles.
/// </summary>
public class TerrainTile
{
    private Mesh lowerMesh;
    private Mesh upperMesh;

    private const int recursionDepth = 7;

    public TerrainTile(float worldX, float worldZ)
    {
        float genX = 128 * CoordinateLookup.XFromUV(worldX, worldZ);
        float genZ = 128 * CoordinateLookup.ZFromUV(worldX, worldZ);

        GenerateMeshes(worldX, worldZ);
        GameObject lower = Generator.GenerateObject(lowerMesh, new Vector3(genX, 0, genZ));
        lower.transform.localScale = 128 * Vector3.one;
        lower.name = "Lower x=" + worldX + " z=" + worldZ;
        GameObject upper = Generator.GenerateObject(upperMesh, new Vector3(genX, 0, genZ));
        upper.name = "Upper x=" + worldX + " z=" + worldZ;
        upper.transform.localScale = 128 * Vector3.one;
    }

    private void GenerateMeshes(float worldX, float worldZ)
    {
        float width = 1 << recursionDepth;
        float[] noise = new float[(int)((width + 1) * (width + 1))];

        for (int i = 0; i < width + 1; i++)
        {
            for (int j = 0; j < width + 1; j++)
            {
                float u = worldX + (i / width);
                float v = worldZ + (j / width);
                noise[(int)((j * (width + 1)) + i)] = GameManager.NoiseGenerator.GetPerlinFractal(CoordinateLookup.XFromUV(u, v), CoordinateLookup.ZFromUV(u, v));
                //noise[(int)((i * (width + 1)) + j)] = 0f;
                //noise[(int)((i * (width + 1)) + j)] = i / width;
                //noise[(int)((i * (width + 1)) + j)] = j / width;
            }
        }

        Vector3[] upperVertices = new Vector3[(int)(3 * width * width)];
        int[] upperTriangles = new int[(int)(3 * width * width)];

        Vector3[] lowerVertices = new Vector3[(int)(3 * width * width)];
        int[] lowerTriangles = new int[(int)(3 * width * width)];

        int lowerCount = 0;
        int upperCount = 0;

        //Debug.Log("Max size: " + upperVertices.Length);
        // 
        for (int z = 0; z < width; z++)
        {
            //todo make this an equilateral triangle, not a half square
            float offset = z / width * 0.5f;
            float offsetAndHalf = (z + 1) / width * 0.5f;

            // todo: Factor out conditionals by being clever about the loop.
            // for now it's fine, but later this will be a relatively nice optimization.

            // todo: why is there artefacting on the x=z line? 
            // looks like we read the noise from (x/2) to x, and 0 to (x/2)
            // instead of going from 0 to x. maybe the bottom/top half logic is wrong.

            for (int x = 0; x < width; x++)
            {
                float heightScale = Mathf.Sqrt(3) / 2f;
                //int baseIndex = 6 * (int)((i * width) + j);
                int noiseBase = (int)((z * (width + 1)) + x);

                //int baseIndex = count;
                //Debug.Log(baseIndex);
                //Debug.Log(lowerCount + ", upper: " + upperCount + " sum: " + (lowerCount + upperCount));

                // on boundary between meshes; special case
                //if (i + (width - j) >= width)
                //{
                //continue;
                //}

                //vertices[(4 * baseIndex) + 0] = new Vector3(i / width, noise[baseIndex], j / width);
                //vertices[(4 * baseIndex) + 1] = new Vector3((i+1) / width, noise[baseIndex + (int)width], j / width);
                //vertices[(4 * baseIndex) + 2] = new Vector3(i / width, noise[baseIndex + 1], (j+1) / width);
                //vertices[(4 * baseIndex) + 3] = new Vector3((i+1) / width, noise[baseIndex + (int)width + 1], (j+1) / width);

                // bottom half, left triangle
                if (x + z + 1 <= width)
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
                if (x + z + 2 <= width)
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
        }

        lowerMesh = Generator.GenerateMesh(lowerVertices, lowerTriangles);
        upperMesh = Generator.GenerateMesh(upperVertices, upperTriangles);
    }
}