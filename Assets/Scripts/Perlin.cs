using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Perlin
{

    protected int octaves;
    protected float frequency;
    protected float lacunarity;
    protected float persistence;
    protected float xOffset;
    protected float zOffset;

    public Perlin(float xOffset = 0.0f, float zOffset = 0.0f, int octaves = 8, float frequency = 1.0f, float lacunarity = 2.0f, float persistence = 0.5f)
    {
        this.xOffset = xOffset;
        this.zOffset = zOffset;
        this.octaves = octaves;
        this.frequency = frequency;
        this.lacunarity = lacunarity;
        this.persistence = persistence;
    }

    public float GetValue(float x, float z)
    {
        float value = 0.0f;
        float multiplier = 1.0f;
        x = (x + xOffset) * frequency;
        z = (z + zOffset) * frequency;
        for (int i = 0; i < octaves; i++)
        {
            value += multiplier * Mathf.PerlinNoise(x, z);

            multiplier *= persistence;
            x *= lacunarity;
            z *= lacunarity;
        }
        return value;
    }

    public float GetNormalizedValue(float x, float z)
    {
        return GetValue(x, z) / (2f - Mathf.Pow(2, -octaves + 1));
    }

    #region Converters from skew [generation] coordinates to square [mesh array] coordinates
    public float UFromXZ(float x, float z)
    {
        return x - (Mathf.Sqrt(3) / 3f * z);
    }

    public float VFromXZ(float x, float z)
    {
        return 2f * Mathf.Sqrt(3) / 3f * z;
    }
    #endregion


    #region Converters from square [mesh array] coordinates to skew [generation] coordinates
    public float XFromUV(float u, float v)
    {
        return u + (0.5f * v);
    }

    public float YFromUV(float u, float v)
    {
        return (Mathf.Sqrt(3) / 2f) * v;
    }
    #endregion
}
