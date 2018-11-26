using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Rendering;

public class PerlinGPU
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

    public PerlinGPU(float xOffset = 0.0f, float zOffset = 0.0f, int octaves = 8, float frequency = 1.0f, float lacunarity = 2.0f, float persistence = 0.5f)
    {
        this.xOffset = xOffset;
        this.zOffset = zOffset;
        this.octaves = octaves;
        this.frequency = frequency;
        this.lacunarity = lacunarity;
        this.persistence = persistence;
    }

    private AsyncGPUReadbackRequest request;
    private static bool isExecuting = false;

    /// <summary>
    /// Uses a GPU compute shader to compute perlin noise.
    /// The problem with this script is that it needs to run in a coroutine; any
    /// script that calls it needs to respect the asyncronous nature of the call.
    /// 
    /// In addition, early tests showed that the swap time from GPU to CPU memory
    /// is huge; thus, this cannot be called every frame. It might be better if 
    /// this method is called infrequently for batch computation jobs.
    /// </summary>
    /// <returns>The computed values.</returns>
    /// <param name="input">An array of input positions to the 3D Perlin noise function.</param>
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
            // TODO: this needs to request data using a Coroutine.
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
}
