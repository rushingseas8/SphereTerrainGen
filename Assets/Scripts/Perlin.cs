using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Rendering;

public class Perlin
{

    protected int octaves;
    protected float frequency;
    protected float lacunarity;
    protected float persistence;
    protected float xOffset;
    protected float zOffset;

    private ComputeShader compute;
    private int kernelIndex = -1;


    private ComputeBuffer inputBuffer;
    private ComputeBuffer outputBuffer;

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

    private AsyncGPUReadbackRequest request;
    private static bool isExecuting = false;

    public float[] GetValueShader(Vector3[] input)
    {
        if (!isExecuting)
        {
            if (compute == null)
            {
                compute = Resources.Load<ComputeShader>("Shaders/Perlin3D");

                kernelIndex = compute.FindKernel("Perlin3D");

                inputBuffer = new ComputeBuffer(input.Length, 12);
                outputBuffer = new ComputeBuffer(input.Length, 4);

                compute.SetBuffer(kernelIndex, "Input", inputBuffer);
                compute.SetBuffer(kernelIndex, "Result", outputBuffer);
            }

            float[] outputArray = new float[input.Length];

            int numFinished = 0;

            inputBuffer.SetData(input);

            int parallel = 128;
            while (numFinished < input.Length)
            {
                //Debug.Log("Finished " + numFinished);

                // Load in the input Vector3s
                //inputBuffer.SetData(input, numFinished, numFinished, parallel);
                compute.Dispatch(kernelIndex, (input.Length / parallel), 1, 1);

                //Vector3[] result = new Vector3[1024];
                numFinished += parallel;
            }


            //outputBuffer.GetData(outputArray);
            request = AsyncGPUReadback.Request(outputBuffer);
            isExecuting = true;
            return null;
        }
        else
        {
            // oi m8 hurry up
            request.Update();

            if (request.done)
            {
                if (request.hasError)
                {
                    Debug.LogError("Failed to transfer data from GPU to CPU!");
                    return null;
                } else {

                    float[] toReturn = request.GetData<float>().ToArray();

                    inputBuffer.Release();
                    outputBuffer.Release();

                    return toReturn;
                }
            } else {
                // Not done yet!
                return null;
            }
        }

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
