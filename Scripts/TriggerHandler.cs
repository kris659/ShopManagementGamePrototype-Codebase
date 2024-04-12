using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerHandler: MonoBehaviour
{
    public delegate void TriggerEvent(Collider other);

    public TriggerEvent triggerEnter;
    public TriggerEvent triggerExit;


    private void OnTriggerEnter(Collider other)
    {
        triggerEnter?.Invoke(other);
    }
    private void OnTriggerExit(Collider other)
    {
        triggerExit?.Invoke(other);
    }
}
