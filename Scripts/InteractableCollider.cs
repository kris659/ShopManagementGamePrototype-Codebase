using System;
using UnityEngine;

public class InteractableCollider : MonoBehaviour, IInteractable
{
    IInteractable interactableParent;
    public string InteractionText { get { return interactableParent.InteractionText; } }
    public int InteractionTextSize => interactableParent.InteractionTextSize;


    public void Init(IInteractable interactableParent)
    {
        this.interactableParent = interactableParent;
    }

    public void OnPlayerButtonInteract()
    {
        if(interactableParent != null) {
            interactableParent.OnPlayerButtonInteract();
        }
        else {
            Debug.LogError("Interactable parent not set up!");
        }
    }
    public void OnMouseButtoDown()
    {
        if (interactableParent != null) {
            interactableParent.OnMouseButtoDown();
        }
        else {
            Debug.LogError("Interactable parent not set up!");
        }
    }
    public void OnMouseButton()
    {
        if (interactableParent != null) {
            interactableParent.OnMouseButton();
        }
        else {
            Debug.LogError("Interactable parent not set up!");
        }
    }
    public void OnMouseButtonUp()
    {
        if (interactableParent != null) {
            interactableParent.OnMouseButtonUp();
        }
        else {
            Debug.LogError("Interactable parent not set up!");
        }
    }
}
