using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NonFunctionalBuilding : MonoBehaviour, IBuildable
{
    public static void Spawn(int shelfTypeIndex, Vector3 position, Quaternion rotation)
    {        
        GameObject shelfGO = Instantiate(SOData.shelvesList[shelfTypeIndex].Prefab, position, rotation);
    }

    public bool CanBuildHere(Vector3 position, Quaternion rotation)
    {
        return true;
    }

    public void Build(int typeIndex, Vector3 position, Quaternion rotation)
    {
        Spawn(typeIndex, position, rotation);
    }
    public void Destroy()
    {
        Destroy(gameObject);
    }
}
