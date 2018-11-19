using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    private CoordinateLookup coordinate;

	// Use this for initialization
	void Start () {
        coordinate = new CoordinateLookup();

        //Debug.LogError("Sphere lookup: " + coordinate.MeshToSphere(coordinate.GetMeshCoordinate(0, 0, 8, 0), 2));

        //Perlin perlin = new Perlin();
        //Debug.Log("GPU Perlin: " + perlin.GetValueShader(new Vector3(0, 0, 0)));
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
