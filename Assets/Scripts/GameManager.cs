using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    // Singleton pattern
    private static GameManager instance;

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    /// <summary>
    /// The noise generator for perlin noise.
    /// </summary>
    private static FastNoise _noise;
    public static FastNoise NoiseGenerator
    {
        get
        {
            if (_noise == null)
            {
                _noise = new FastNoise(instance.seed);
                _noise.SetFrequency(1f);
                _noise.SetFractalOctaves(instance.octaves);
            }
            return _noise;
        }
    }

    /// <summary>
    /// The render distance.
    /// </summary>
    [SerializeField]
    [Range(1, 30)]
    private int renderDistance = 5;
    public static int RenderDistance
    {
        get
        {
            return instance.renderDistance;
        }

        set
        {
            instance.renderDistance = value;
        }
    }

    private CoordinateLookup coordinate;
    public static CoordinateLookup Coordinate {
        get {
            return instance.coordinate;
        }
    }

    // Instance variables (tied to a GameManager instance)

    /// <summary>
    /// The seed used for the random number generator.
    /// </summary>
    [SerializeField]
    private int seed;

    /// <summary>
    /// How many octaves we use for the perlin noise.
    /// </summary>
    [SerializeField]
    [Range(1, 8)]
    private int octaves;

    private SquareBuffer<TerrainTile> terrainBuffer;


	// Use this for initialization
	void Start () {
        coordinate = new CoordinateLookup();
        //terrainBuffer = new SquareBuffer<TerrainTile>(renderDistance);

        //Debug.LogError("Sphere lookup: " + coordinate.MeshToSphere(coordinate.GetMeshCoordinate(0, 0, 8, 0), 2));

        //Perlin perlin = new Perlin();
        //Debug.Log("GPU Perlin: " + perlin.GetValueShader(new Vector3(0, 0, 0)));

        //// Basic LOD test
        //for (int i = -renderDistance; i <= renderDistance; i++)
        //{
        //    for (int j = -renderDistance; j <= renderDistance; j++)
        //    {
        //        if (Mathf.Abs(i) <= 1 && Mathf.Abs(j) <= 1) {
        //            TerrainTile tile = new TerrainTile(i, j, 7);
        //        }

        //        else if (Mathf.Abs(i) <= 3 && Mathf.Abs(j) <= 3)
        //        {
        //            TerrainTile tile = new TerrainTile(i, j, 6);
        //        }

        //        else if (Mathf.Abs(i) <= 6 && Mathf.Abs(j) <= 6)
        //        {
        //            TerrainTile tile = new TerrainTile(i, j, 5);
        //        }

        //        else if (Mathf.Abs(i) <= 10 && Mathf.Abs(j) <= 10) 
        //        {
        //            TerrainTile tile = new TerrainTile(i, j, 4);
        //        }

        //        //else if (Mathf.Abs(i) <= 15 && Mathf.Abs(j) <= 15)
        //        //{
        //        //    TerrainTile tile = new TerrainTile(i, j, 3);
        //        //}

        //        //else if (Mathf.Abs(i) <= 21 && Mathf.Abs(j) <= 21)
        //        //{
        //        //    TerrainTile tile = new TerrainTile(i, j, 2);
        //        //}

        //        // Below resolution level 3, it becomes obvious
        //        else
        //        {
        //            TerrainTile tile = new TerrainTile(i, j, 3);
        //        }
        //    }
        //}


        for (int i = -renderDistance; i <= renderDistance; i++)
        {
            for (int j = -renderDistance; j <= renderDistance; j++)
            {
                TerrainTile tile = new TerrainTile(i, j, 3);
            }
        }


        //TerrainTile tile = new TerrainTile(0, 0);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
