using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A basic first person player controller.
/// Allows for flight mode and walking around on a surface.
/// </summary>
public class Controller : MonoBehaviour {

    public KeyCode[] forward { get; set; }
    public KeyCode[] backward { get; set; }
    public KeyCode[] left { get; set; }
    public KeyCode[] right { get; set; }
    public KeyCode[] up { get; set; }
    public KeyCode[] down { get; set; }

    public Camera mainCamera;

    private float thirdPersonDistance = 0;

    public bool flyingMode = true;

    private static float movementScale = 3f;
    private static float rotationScale = 5f;

    // Use this for initialization
    void Start () {
        forward    = new KeyCode[]{ KeyCode.W, KeyCode.UpArrow };
        backward = new KeyCode[]{ KeyCode.S, KeyCode.DownArrow };
        left = new KeyCode[]{ KeyCode.A, KeyCode.LeftArrow };
        right = new KeyCode[]{ KeyCode.D, KeyCode.RightArrow };
        up = new KeyCode[]{ KeyCode.LeftShift, KeyCode.Space };
        down = new KeyCode[]{ KeyCode.LeftControl, KeyCode.LeftAlt };

        //mainCamera = Camera.main;

        //Cursor.lockState = CursorLockMode.Locked;

        /*
        if (flyingMode)
            movementScale = 2.5f;
        else
            movementScale = 0.2f;*/
    }

    bool flag = true;

    bool keycodePressed(KeyCode[] arr) {
        for(int i = 0; i < arr.Length; i++) {
            if(Input.GetKey(arr[i])) {
                return true;
            }
        }
        return false;
    }

    bool keycodeDown(KeyCode[] arr) {
        for(int i = 0; i < arr.Length; i++) {
            if(Input.GetKeyDown(arr[i])) {
                return true;
            }
        }
        return false;
    }

    // Update is called once per frame
    void Update () {
        Vector3 oldPosition = this.gameObject.transform.position;
        Quaternion oldRotation = mainCamera.transform.rotation;

        Vector3 newPosition = oldPosition;
        Quaternion newRotation = oldRotation;

        if (Input.GetKeyDown (KeyCode.Escape)) {
            Cursor.lockState = CursorLockMode.None;
        }

        float planeRotation = mainCamera.transform.rotation.eulerAngles.y;
        Quaternion quat = Quaternion.Euler (new Vector3 (0, planeRotation, 0));

        Quaternion ang = Quaternion.identity;

        bool onGround = false;

        /*
         * Raycast down and if we hit terrain, ensure movement is constrained to be normal to
         * the terrain. This ensures we only move on the ground, instead of clipping in/floating.
         */
        RaycastHit hit;
        if (Physics.Raycast (this.gameObject.transform.position, Vector3.down, out hit)) {
            if (hit.distance <= 1.5f) {
                onGround = true;

                Vector3 fbn = new Vector3 (0, hit.normal.y, hit.normal.z);
                float xAngle = 90 - Vector3.Angle (fbn, Vector3.forward);

                Vector3 lrn = new Vector3 (hit.normal.x, hit.normal.y, 0);
                float zAngle = 90 - Vector3.Angle (lrn, Vector3.left);

                ang = Quaternion.Euler (xAngle, 0, zAngle);
            }
        }

        if (keycodePressed (forward)) {
            newPosition += (ang * quat * Vector3.forward * movementScale);
        }
        if (keycodePressed (backward)) {
            newPosition += (ang * quat * Vector3.back * movementScale);
        }
        if (keycodePressed (left)) {
            newPosition += (ang * quat * Vector3.left * movementScale);
        }
        if (keycodePressed (right)) {
            newPosition += (ang * quat * Vector3.right * movementScale);
        }

        if (flyingMode) {
            if (keycodePressed (up)) {
                newPosition += (Vector3.up * movementScale);
            }
        } else {
            //Only jump if we're on the ground (or very close)
            if (keycodeDown (up)) {    
                if (onGround) {
                    this.gameObject.GetComponent<Rigidbody> ().velocity = new Vector3 (0, 5, 0);
                }
            }
            this.gameObject.GetComponent<Rigidbody>().AddForce(-transform.position.normalized * 9.81f, ForceMode.Acceleration);
        }
            
        if (flyingMode) {
            if (keycodePressed (down)) {
                newPosition += (Vector3.down * movementScale);
            }
        }

        float mouseX = Input.GetAxis ("Mouse X");
        float mouseY = -Input.GetAxis ("Mouse Y");
        if (mouseX != 0 || mouseY != 0) {
            Vector3 rot = oldRotation.eulerAngles;

            float xRot = rot.x;

            float tent = rot.x + (rotationScale * mouseY);

            if (xRot > 270 && tent < 270) {
                xRot = 270;
            } else if (xRot < 90 && tent > 90) {
                xRot = 90;
            } else {
                xRot += rotationScale * mouseY;
            }

            newRotation = Quaternion.Euler (
                new Vector3 (
                    xRot,
                    rot.y + (rotationScale * mouseX),
                    0));
        }

        thirdPersonDistance -= Input.GetAxis ("Mouse ScrollWheel");
        if (thirdPersonDistance < 0)
            thirdPersonDistance = 0;

        this.gameObject.transform.position = newPosition;
        this.gameObject.transform.rotation = ang;

        mainCamera.transform.position = newPosition + (newRotation * new Vector3 (0, 0, -thirdPersonDistance));
        mainCamera.transform.rotation = newRotation;

        // Terrain stuff
        // TODO make the chunks calculate using einstein bonudaries, not cartesian.
        // TODO fix the initial chunk offset. Right now it thinks 0 is the bottom-left; we spawn in chunk 11,11 or something.

        // Because the chunks are using x and z normally, we flip them here.
        int newXChunk = (int)((transform.position.x) / 128) - GameManager.RenderDistance;
        int newZChunk = (int)((transform.position.z) / 128) - GameManager.RenderDistance;

        Direction movementDir = Direction.NONE;

        if (newXChunk < GameManager.XChunk)
        {
            GameManager.XChunk = newXChunk;
            movementDir = Direction.LEFT;
        }
        else if (newXChunk > GameManager.XChunk)
        {
            GameManager.XChunk = newXChunk;
            movementDir = Direction.RIGHT;
        }
        else if (newZChunk < GameManager.ZChunk)
        {
            GameManager.ZChunk = newZChunk;
            movementDir = Direction.BACK;
        }
        else if (newZChunk > GameManager.ZChunk)
        {
            GameManager.ZChunk = newZChunk;
            movementDir = Direction.FRONT;
        }

        if (movementDir != Direction.NONE)
        {
            GameManager.terrainBuffer.Shift(movementDir);
        }
    }
}
