using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Decoration : Building
{
    public int decorationScore;
    public override Vector3 GetBuildingPosition()
    {
        Vector3 position = base.GetBuildingPosition();
        if(buildingType == BuildingType.FloorBuilding && position == BuildingManager.instance.defaultBuildingPosition) {
            position = BuildingManager.instance.GetBuildingPosition(BuildingType.FloorDecorationOnly);
        }
        return position;
    }
}
