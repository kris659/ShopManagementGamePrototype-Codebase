using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Garbage : MonoBehaviour, IInteractable
{
    public string InteractionText => "F - Clean";
    public int InteractionTextSize => 60;

    public bool isTaken = false;

    public void OnPlayerButtonInteract()
    {
        Clean();
    }

    public void Clean()
    {
        CustomerManager.instance.SpawnedGarbage.Remove(gameObject);
        Destroy(gameObject);
    }


    public void OnMouseButtoDown() { }
    public void OnMouseButton() { }
    public void OnMouseButtonUp() { }
}
