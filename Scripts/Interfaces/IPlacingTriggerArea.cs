using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlacingTriggerArea
{
    public void OnProductPlacedInArea(Product product);
    public void OnProductTakenFromArea(Product product);
    public void OnContainerPlacedInArea(Container product);
    public void OnContainerTakenFromArea(Container product);
}
