using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingSurface : MonoBehaviour
{
    //[EnumFlags] 
    [SerializeField] public BuildingType allowedBuildingType;

    private void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("BuildingSurface");
    }
}
