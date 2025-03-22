using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class House : MonoBehaviour
{
    public Vector3 housePosition => transform.position;
    //public Vector3 houseEntrancePosition => transform.GetChild(0).position;
    public string streetName;
    public int houseNumber;
    ContainerGO containerGO;

    public void Init(string streetName, int houseNumber)
    {
        this.streetName = streetName;
        this.houseNumber = houseNumber;

        TriggerHandler triggerHandler = GetComponentInChildren<TriggerHandler>();
        triggerHandler.triggerEnter += TriggerEnter;
    }

    private void Update()
    {
        containerGO = null;
    }

    private void TriggerEnter(Collider other)
    {
        if (containerGO != null)
            return;
        containerGO = other.GetComponentInParent<ContainerGO>();
        if (containerGO != null) {
            OnlineOrdersManager.instance.CheckOnlineOrderCompletition(this, containerGO);
        }
    }
}
