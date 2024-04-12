using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFPSCamera : MonoBehaviour
{
    public GameObject cameraBrain;
    public Rigidbody playerRigidbody;
    public GameObject followPoint;
    public Vector3 offset;

    private void Update()
    {
        //playerRigidbody.position
        transform.position = playerRigidbody.transform.position + offset;
        if (playerRigidbody.gameObject.activeSelf){
            playerRigidbody.MoveRotation(Quaternion.Euler(new Vector3(0, cameraBrain.transform.eulerAngles.y)));
        }
    }
}
