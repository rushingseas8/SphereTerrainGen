using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

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
    [Range(1, 12)]
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

    private CoordinateLookup coordinate;
    private SquareBuffer<TerrainTile> terrainBuffer;


	// Use this for initialization
	void Start () {
        coordinate = new CoordinateLookup();
        //terrainBuffer = new SquareBuffer<TerrainTile>(renderDistance);

        //Debug.LogError("Sphere lookup: " + coordinate.MeshToSphere(coordinate.GetMeshCoordinate(0, 0, 8, 0), 2));

        //Perlin perlin = new Perlin();
        //Debug.Log("GPU Perlin: " + perlin.GetValueShader(new Vector3(0, 0, 0)));

        TerrainTile tile = new TerrainTile(0, 0);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
