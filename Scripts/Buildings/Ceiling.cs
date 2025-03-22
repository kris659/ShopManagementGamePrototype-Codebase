using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ceiling : Building
{
    [SerializeField] private LayerMask ceilingBuildingLayer;
    [SerializeField] private float ceilingGridSize;

    public override Vector3 GetBuildingPosition()
    {
        return BuildingManager.instance.GetBuildingPosition(BuildingType.Ceiling, ceilingGridSize);
    }
}
