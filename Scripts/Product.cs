using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Product: IPickable
{
    public ProductGO productGO;
    public ProductSO productType;
    public int productTypeIndex;

    public bool isTakenByCustomer;
    public bool isInClosedContainer;
    public List<PlacingTriggerArea> placingTriggerAreas = new List<PlacingTriggerArea>();

    public int HoldingLimit => productType.holdingLimit;
    public int PickableTypeID => productTypeIndex;
    public int PickableID => ProductsData.instance.productsSpawned.IndexOf(this);

    public bool CanPickableInteract => false;
    public string PickableInteractionText => string.Empty;
    public void OnPickableInteract() { }

    public BoxCollider BoxCollider => productType.prefab.GetComponentInChildren<BoxCollider>();

    public GameObject PreviewGameObject => ProductGO.SpawnVisual(productType);
    public GameObject GameObject => productGO.gameObject;

    public List<IPickable> AdditionalPickables => new();
    public void RemoveLastAdditionalPickable() { }

    private int vehicleAreasCount = 0;
    public Product(int typeIndex)
    {
        this.productTypeIndex = typeIndex;
        this.productType = SOData.productsList[typeIndex];
        ProductsData.instance.productsSpawned.Add(this);
    }

    public Product(int typeIndex, bool isPhysxSpawned, Vector3 position, Quaternion rotation)
    {
        this.productTypeIndex = typeIndex;
        this.productType = SOData.productsList[typeIndex];
        ProductsData.instance.productsSpawned.Add(this);
        if (isPhysxSpawned)
            Place(position, rotation, null, false);
    }    

    private void SpawnVisual(Transform parent)
    {
        productGO = ProductGO.Spawn(true, Vector3.zero, Quaternion.identity, parent, this);
        productGO.transform.localPosition = productType.holdingPosition;
        productGO.transform.localRotation = Quaternion.Euler(productType.holdingRotation);
        productGO.transform.localScale = Vector3.one;
    }

    public void SpawnVisual(Vector3 position, Quaternion rotation, Transform parent)
    {
        DestroyGameObject();
        productGO = ProductGO.Spawn(true, position, rotation, parent, this);
    }

    public void Place(Vector3 position, Quaternion rotation, Transform parent, bool playSound)
    {
        DestroyGameObject();
        productGO = ProductGO.Spawn(false, position, rotation, parent, this);

        if(playSound)
            AudioManager.PlaySound(Sound.ProductDrop, position, productGO.transform);
    }    

    public void OnPlayerTake(bool shoudSpawnVisual, Transform parent)
    {
        DestroyGameObject();
        if (shoudSpawnVisual) {
            SpawnVisual(parent);
        }
        AudioManager.PlaySound(Sound.ProductPickup, parent.position);
    }    

    public ProductSaveData CreateSaveData()
    {
        Vector3 position = Vector3.zero;
        Quaternion rotation = Quaternion.identity;

        if (productGO != null) {
            position = productGO.transform.position;
            rotation = productGO.transform.rotation;
        }
        bool isPhysxSpawned = productGO != null && productGO.isPhysixSpawned;

        return new ProductSaveData(productTypeIndex, isPhysxSpawned, position, rotation);
    }
    public void DestroyGameObject()
    {
        vehicleAreasCount = 0;
        foreach (PlacingTriggerArea triggerArea in placingTriggerAreas) {
            triggerArea.OnProductTakenFromArea(this);
        }
        placingTriggerAreas.Clear();
        if (productGO != null) {
            GameObject.Destroy(productGO.gameObject);
        }
    }

    public void RemoveFromGame(bool shouldRemoveFromProductsList)
    {
        DestroyGameObject();
        if (shouldRemoveFromProductsList && ProductsData.instance.productsSpawned.Contains(this))
            ProductsData.instance.productsSpawned.Remove(this);
    }

    public void OnVehicleAreaEnter()
    {
        if (vehicleAreasCount == 0 && productGO != null && productGO.transform.TryGetComponent(out Rigidbody rb)) {
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
        vehicleAreasCount++;
    }
    public void OnVehicleAreaExit()
    {
        vehicleAreasCount--;
        if(vehicleAreasCount == 0 && productGO != null && productGO.transform.TryGetComponent(out Rigidbody rb)) {
            rb.interpolation = RigidbodyInterpolation.None;
        }
    }    
}
