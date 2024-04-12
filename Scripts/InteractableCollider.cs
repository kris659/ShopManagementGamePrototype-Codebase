using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableCollider : MonoBehaviour, IInteractable
{
    IInteractable interactableParent;
    public string textToDisplay { get { return interactableParent.textToDisplay; } }

    public void Init(IInteractable interactableParent)
    {
        this.interactableParent = interactableParent;
    }

    public void OnPlayerInteract()
    {
        if(interactableParent != null) {
            interactableParent.OnPlayerInteract();
        }
        else {
            Debug.LogWarning("Interactable parent not set up!");
        }
    }
}
