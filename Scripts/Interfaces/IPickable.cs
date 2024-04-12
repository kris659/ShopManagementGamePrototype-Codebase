using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPickable
{
    public void OnPlayerTake(bool shoudSpawnVisual, Transform parent);
    public void Place(Vector3 position, Quaternion rotation, Transform parent);
}
