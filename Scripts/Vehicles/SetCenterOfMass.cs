using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetCenterOfMass : MonoBehaviour
{
    public Transform center;
    void Awake()
    {
        GetComponent<Rigidbody>().centerOfMass = center.localPosition;
    }
}
