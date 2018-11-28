using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    // Singleton pattern
    private static GameManager instance;

    public static GameManager GetInstance() {
        return instance;
    }

    /// <summary>
    /// The noise generator for perlin noise.
    /// </summary>
    public static FastNoise NoiseGenerator
    {
        get { return instance.noise; }
    }

    /// <summary>
    /// The render distance.
    /// </summary>
    public static int RenderDistance
    {
        get { return instance.renderRadius; }
        set
        {
            instance.renderRadius = value;
            instance.renderDiameter = (2 * instance.renderRadius) + 1;
        }
    }

    public static CoordinateLookup Coordinate {
        get { return instance.coordinate; }
    }

    #region Instance variables (tied to a GameManager instance)
    private FastNoise noise;

    [SerializeField]
    [Range(0, 30)]
    private int renderRadius = 5;
    private int renderDiameter;

    private CoordinateLookup coordinate;

    /// <summary>
    /// The seed used for the random number generator.
    /// </summary>
    [SerializeField]
    private int seed = 0;

    /// <summary>
    /// How many octaves we use for the perlin noise.
    /// </summary>
    [SerializeField]
    [Range(1, 8)]
    private int octaves = 5;

    public static TerrainBuffer terrainBuffer;

    public static int XChunk { get; set; }
    public static int ZChunk { get; set; }

    #endregion


    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.Log("Extra GameManager found on object.");

            #if UNITY_EDITOR
            UnityEditor.EditorGUIUtility.PingObject(gameObject);
            #endif
        }
    }

    // Use this for initialization
    void Start () {

        noise = new FastNoise(seed);
        noise.SetFrequency(1f);
        noise.SetFractalOctaves(octaves);

        renderDiameter = (2 * renderRadius) + 1;

        coordinate = new CoordinateLookup();
        terrainBuffer = new TerrainBuffer(renderDiameter);

        XChunk = -RenderDistance;
        ZChunk = -RenderDistance;

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

        for (int i = -renderRadius; i <= renderRadius; i++)
        {
            for (int j = -renderRadius; j <= renderRadius; j++)
            {
                terrainBuffer[i + renderRadius, j + renderRadius] = new TerrainTile(i, j, 3);
            }
        }


        //TerrainTile tile = new TerrainTile(0, 0);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
