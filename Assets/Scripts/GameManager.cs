using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    private CoordinateLookup coordinate;

	// Use this for initialization
	void Start () {
        coordinate = new CoordinateLookup();

        Debug.Log("Sphere lookup: " + coordinate.MeshToSphere(coordinate.GetMeshCoordinate(0, 0, 3, 1), 0));
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
