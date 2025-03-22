using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPickable
{
    public void OnPlayerTake(bool shoudSpawnVisual, Transform parent);
    public void Place(Vector3 position, Quaternion rotation, Transform parent, bool playSound);
    public void RemoveFromGame(bool removeFromList);
    public GameObject PreviewGameObject { get; }
    public GameObject GameObject { get; }
    public int HoldingLimit { get; }
    public BoxCollider BoxCollider { get; }
    public int PickableTypeID { get; }
    public int PickableID { get; }
    public List<IPickable> AdditionalPickables { get; }

    public bool CanPickableInteract { get; }
    public string PickableInteractionText { get; }
    public void OnPickableInteract();
    public void RemoveLastAdditionalPickable();

}
