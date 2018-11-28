using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseGenerator
{
    //private static NoiseGenerator instance;
    //public static NoiseGenerator GetInstance()
    //{
    //    if (instance == null) {
    //        instance = new NoiseGenerator();
    //    }
    //    return instance;
    //}

    private static int temperatureSeed = 1232;
    private static int precipitationSeed = 941;

    public static float GetTemperature(int worldX, int worldZ, float percentX, float percentZ) {

        float u = worldX + percentX;
        float v = worldZ + percentZ;

        float x = CoordinateLookup.XFromUV(u, v);
        float z = CoordinateLookup.ZFromUV(u, v);

        GameManager.NoiseGenerator.SetFractalType(FastNoise.FractalType.FBM);
        GameManager.NoiseGenerator.SetSeed(temperatureSeed);
        GameManager.NoiseGenerator.SetFractalOctaves(2);
        GameManager.NoiseGenerator.SetFrequency(1f / 64f);

        return (GameManager.NoiseGenerator.GetPerlinFractal(x, z) + 0.5f) / 1f;
    }

    public static float GetPrecipitation(int worldX, int worldZ, float percentX, float percentZ)
    {
        float u = worldX + percentX;
        float v = worldZ + percentZ;

        float x = CoordinateLookup.XFromUV(u, v);
        float z = CoordinateLookup.ZFromUV(u, v);

        GameManager.NoiseGenerator.SetFractalType(FastNoise.FractalType.FBM);
        GameManager.NoiseGenerator.SetSeed(precipitationSeed);
        GameManager.NoiseGenerator.SetFractalOctaves(2);
        GameManager.NoiseGenerator.SetFrequency(1f / 64f);

        return (GameManager.NoiseGenerator.GetPerlinFractal(x, z) + 0.5f) / 1f;
    }

    public static float GetLandOcean(int worldX, int worldZ, float percentX, float percentZ)
    {
        float u = worldX + percentX;
        float v = worldZ + percentZ;

        float x = CoordinateLookup.XFromUV(u, v);
        float z = CoordinateLookup.ZFromUV(u, v);

        /*
        // This isn't bad. It's also not awesome.
        GameManager.NoiseGenerator.SetFractalType(FastNoise.FractalType.RigidMulti);
        GameManager.NoiseGenerator.SetSeed(precipitationSeed);
        GameManager.NoiseGenerator.SetFractalOctaves(5);
        GameManager.NoiseGenerator.SetFrequency(1f / 4f);

        float noise = (GameManager.NoiseGenerator.GetPerlinFractal(x, z) + 0.5f) / 1f;
        if (noise < 0.4f)
        {
            return 0f;
        }
        if (noise > 0.6f)
        {
            return 1f;
        }
        // Renormalize to [0,1]
        noise = 5f * (noise - 0.4f);

        return noise;
        */

        GameManager.NoiseGenerator.SetFractalType(FastNoise.FractalType.FBM);
        GameManager.NoiseGenerator.SetFractalOctaves(5);
        GameManager.NoiseGenerator.SetFrequency(1 / 4f);

        // Grab 10 different perlin noise generators. See if at least half agree where land should be.
        int count = 0;
        for (int i = 0; i < 8; i++)
        {
            GameManager.NoiseGenerator.SetSeed(i);

            // Grab some deterministic random offset in the range [0,1]. This reduces artefacting
            float xJitter = GameManager.NoiseGenerator.GetValue(x, z);
            float zJitter = GameManager.NoiseGenerator.GetValue(x + xJitter, z + xJitter);
            float noise = GameManager.NoiseGenerator.GetPerlinFractal(x + xJitter, z + zJitter) + 0.5f;
            if (noise > 0.5f) {
                count++;
            }
        }

        if (count < 5) {
            return 0;
        } else {
            return 1;
        }
        //return count / 10f;

    }

    [System.Runtime.CompilerServices.MethodImpl(256)] // Aggressive inlining
    public static float GetNoise(int worldX, int worldZ, float percentX, float percentZ) {

        float u = worldX + percentX;
        float v = worldZ + percentZ;

        float x = CoordinateLookup.XFromUV(u, v);
        float z = CoordinateLookup.ZFromUV(u, v);

        // Good mountains
        GameManager.NoiseGenerator.SetFractalType(FastNoise.FractalType.RigidMulti);
        GameManager.NoiseGenerator.SetFractalLacunarity(2f);
        GameManager.NoiseGenerator.SetFrequency(1f / 8f);
        //return 4f * GameManager.NoiseGenerator.GetPerlinFractal(x, z);
        return GetLandOcean(worldX, worldZ, percentX, percentZ);

        //return GameManager.NoiseGenerator.GetPerlinFractal(CoordinateLookup.XFromUV(u, v), CoordinateLookup.ZFromUV(u, v));
    }
}
