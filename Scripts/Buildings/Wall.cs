using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall: MonoBehaviour, IBuildable
{
    [HideInInspector]
    public int typeIndex;
    public static void Spawn(int wallTypeIndex, Vector3 position, Quaternion rotation)
    {      
        GameObject wallGO = Instantiate(SOData.wallsList[wallTypeIndex].Prefab, position, rotation);
        Wall wall = wallGO.GetComponent<Wall>();
        wall.typeIndex = wallTypeIndex;
        ShopData.instance.AddWall(wall);
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
        ShopData.instance.RemoveWall(this);
        Destroy(gameObject);
    }
}
