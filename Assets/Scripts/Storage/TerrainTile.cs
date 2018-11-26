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

    public TerrainTile(int worldX, int worldY)
    {
        GenerateMeshes(worldX, worldY);
        GameObject lower = Generator.GenerateObject(lowerMesh);
        GameObject upper = Generator.GenerateObject(upperMesh);
    }

    private void GenerateMeshes(int worldX, int worldY)
    {
        float width = 1 << recursionDepth;
        float[] noise = new float[(int)((width + 1) * (width + 1))];

        for (int i = 0; i < width + 1; i++)
        {
            for (int j = 0; j < width + 1; j++)
            {
                noise[(int)((i * (width + 1)) + j)] = GameManager.NoiseGenerator.GetPerlinFractal(worldX + (i / width), worldY + (j / width));
                //noise[(int)((i * (width + 1)) + j)] = 0f;
            }
        }

        Vector3[] upperVertices = new Vector3[(int)(3 * width * width)];
        int[] upperTriangles = new int[(int)(3 * width * width)];

        Vector3[] lowerVertices = new Vector3[(int)(3 * width * width)];
        int[] lowerTriangles = new int[(int)(3 * width * width)];

        int lowerCount = 0;
        int upperCount = 0;

        //Debug.Log("Max size: " + upperVertices.Length);
        for (int i = 0; i < width; i++)
        {
            //todo make this an equilateral triangle, not a half square
            //float widthOffset = i * 0.5f;

            // todo: Factor out conditionals by being clever about the loop.
            // for now it's fine, but later this will be a relatively nice optimization.

            // todo: the noise is in the wrong order! (maybe swap the +1 and +width)

            for (int j = 0; j < width; j++)
            {
                //int baseIndex = 6 * (int)((i * width) + j);
                int noiseBase = (int)((i * width) + j);

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


                // on top half (left triangle)
                if (i + j + 2 > width)
                {
                    upperVertices[upperCount + 0] = new Vector3((i + 1) / width, noise[noiseBase + (int)width], j / width);
                    upperVertices[upperCount + 1] = new Vector3(i / width, noise[noiseBase + 1], (j + 1) / width);
                    upperVertices[upperCount + 2] = new Vector3((i + 1) / width, noise[noiseBase + (int)width + 1], (j + 1) / width);

                    upperTriangles[upperCount + 0] = upperCount + 0;
                    upperTriangles[upperCount + 1] = upperCount + 1;
                    upperTriangles[upperCount + 2] = upperCount + 2;

                    upperCount += 3;
                }
                // on bottom half
                else
                {

                    lowerVertices[lowerCount + 0] = new Vector3((i + 1) / width, noise[noiseBase + (int)width], j / width);
                    lowerVertices[lowerCount + 1] = new Vector3(i / width, noise[noiseBase + 1], (j + 1) / width);
                    lowerVertices[lowerCount + 2] = new Vector3((i + 1) / width, noise[noiseBase + (int)width + 1], (j + 1) / width);

                    lowerTriangles[lowerCount + 0] = lowerCount + 0;
                    lowerTriangles[lowerCount + 1] = lowerCount + 1;
                    lowerTriangles[lowerCount + 2] = lowerCount + 2;

                    lowerCount += 3;
                }

                // on top half (right triangle); flip triangle indices
                if (i + j + 1 > width)
                {
                    upperVertices[upperCount + 0] = new Vector3(i / width, noise[noiseBase], j / width);
                    upperVertices[upperCount + 1] = new Vector3((i + 1) / width, noise[noiseBase + 1], j / width);
                    upperVertices[upperCount + 2] = new Vector3(i / width, noise[noiseBase + (int)width], (j + 1) / width);

                    upperTriangles[upperCount + 0] = upperCount + 0;
                    upperTriangles[upperCount + 1] = upperCount + 2;
                    upperTriangles[upperCount + 2] = upperCount + 1;

                    upperCount += 3;
                }
                else
                {
                    lowerVertices[lowerCount + 0] = new Vector3(i / width, noise[noiseBase], j / width);
                    lowerVertices[lowerCount + 1] = new Vector3((i + 1) / width, noise[noiseBase + 1], j / width);
                    lowerVertices[lowerCount + 2] = new Vector3(i / width, noise[noiseBase + (int)width], (j + 1) / width);

                    lowerTriangles[lowerCount + 0] = lowerCount + 0;
                    lowerTriangles[lowerCount + 1] = lowerCount + 2;
                    lowerTriangles[lowerCount + 2] = lowerCount + 1;

                    lowerCount += 3;
                }
            }
        }

        lowerMesh = Generator.GenerateMesh(lowerVertices, lowerTriangles);
        upperMesh = Generator.GenerateMesh(upperVertices, upperTriangles);
    }
}