using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    private Rigidbody body;
    private float aroundRotation = 0f;
    private float upRotation = 90f;

    private float movementScale = 10f;

    private float aroundScale = 5f;
    private float upScale = 5f;

    // Use this for initialization
    void Start () {
        body = GetComponent<Rigidbody>();
	}

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W)) 
        {
            transform.position += transform.rotation * (Time.deltaTime * movementScale * Vector3.forward);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position += transform.rotation * (Time.deltaTime * movementScale * Vector3.forward);
        }
        body.velocity = 0.9f * body.velocity;

        aroundRotation += aroundScale * Input.GetAxis("Mouse X");
        upRotation += upScale * Input.GetAxis("Mouse Y");

        transform.rotation = Quaternion.AngleAxis(aroundRotation, Vector3.up) * Quaternion.AngleAxis(upRotation, Vector3.left);

    }
}
