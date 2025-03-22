using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlacingTriggerAreaParent))]
public class WarehouseShelf : Building
{
    [SerializeField] private List<Transform> boxPositions;
    [SerializeField] private Transform workerDestination;
    [HideInInspector]
    public PlacingTriggerArea[] shelfTriggers;

    PlacingTriggerAreaParent placingTriggerAreaParent;

    public override void Build()
    {
        base.Build();
        ShopData.instance.AddWarehouseShelf(this);
        shelfTriggers = buildingModel.transform.GetComponentsInChildren<PlacingTriggerArea>();

        placingTriggerAreaParent = GetComponent<PlacingTriggerAreaParent>();

        placingTriggerAreaParent.OnProductTriggerEnterEvent += OnProductPlacedInArea;
        placingTriggerAreaParent.OnProductTriggerExitEvent += OnProductTakenFromArea;

        placingTriggerAreaParent.OnContainerTriggerEnterEvent += OnContainerPlacedInArea;
        placingTriggerAreaParent.OnContainerTriggerExitEvent += OnContainerTakenFromArea;
    }

    public void OnContainerPlacedInArea(Container container)
    {
        container.isContainerOnStorageShelf = true;
        container.GetProductsInContainerData(out List<Product> products, out _, out _);
        StatsManager.instance.productsOnWarehouseShelves += products.Count;
        TasksManager.instance.ProgressTasks(TaskType.HaveProductsInWarehouse, products.Count);
    }
    public void OnContainerTakenFromArea(Container container)
    {
        container.isContainerOnStorageShelf = false;
        container.GetProductsInContainerData(out List<Product> products, out _, out _);
        StatsManager.instance.productsOnWarehouseShelves -= products.Count;
        TasksManager.instance.ProgressTasks(TaskType.HaveProductsInWarehouse, -products.Count);
    }

    public void OnProductPlacedInArea(Product product) { }

    public void OnProductTakenFromArea(Product product) { }

    private void OnDestroy()
    {
        ShopData.instance.RemoveWarehouseShelf(this);
        if(placingTriggerAreaParent != null) {
            foreach (Container container in placingTriggerAreaParent.containers) {
                container.isContainerOnStorageShelf = false;
            }
        }        
    }
}
