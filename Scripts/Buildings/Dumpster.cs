using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dumpster : Building, IInteractable
{
    public string InteractionText => "F - Throw away";
    public int InteractionTextSize => 60;

    public void OnMouseButtoDown() { }
    public void OnMouseButton() { }
    public void OnMouseButtonUp() { }

    public Transform workersDestination;

    public override void Build()
    {
        base.Build();
        gameObject.GetComponentInChildren<InteractableCollider>().Init(this);
        ShopData.instance.AddDumpster(this);
    }

    public void OnPlayerButtonInteract()
    {
        //Debug.Log("Tried to throw away");
        PlayerData.instance.OnDumpsterUsed();
    }

    private void OnDestroy()
    {
        ShopData.instance.RemoveDumpster(this);
    }
}
