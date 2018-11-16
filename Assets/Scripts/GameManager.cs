using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    private CoordinateLookup coordinate;

	// Use this for initialization
	void Start () {
        coordinate = new CoordinateLookup();

        Debug.Log("Sphere lookup: " + coordinate.MeshToSphere(new Vector3(0, 2, 0), 0));
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
