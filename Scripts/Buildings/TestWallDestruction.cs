using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestWallDestruction : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.impulse.magnitude > 1000) {
            collision.rigidbody.AddForce(-collision.impulse * 0.5f, ForceMode.Impulse);
            Destroy(gameObject);
        }
    }
}
