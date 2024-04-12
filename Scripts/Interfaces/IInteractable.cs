using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    public void OnPlayerInteract();
    public string textToDisplay { get; }
}
