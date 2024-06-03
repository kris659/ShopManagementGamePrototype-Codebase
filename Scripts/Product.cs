using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Product: IPickable
{
    public ProductGO productGO;
    public ProductSO productType;
    public int productTypeIndex;

    public bool isTakenByCustomer;
    public bool isInClosedContainer;
    public List<IPlacingTriggerArea> placingTriggerAreas = new List<IPlacingTriggerArea>();

    public int HoldingLimit => productType.holdingLimit;
    public int PickableTypeID => productTypeIndex;
    public int PickableID => ProductsData.instance.productsSpawned.IndexOf(this);

    public BoxCollider BoxCollider => productType.prefab.GetComponentInChildren<BoxCollider>();

    public GameObject PreviewGameObject => GameObject.Instantiate(productType.visualPrefab);
    public GameObject GameObject => productGO.gameObject;

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
            Place(position, rotation, null);
    }

    public void Place(Vector3 position, Quaternion rotation, Transform parent)
    {
        DestroyGameObject();
        productGO = ProductGO.Spawn(false, position, rotation, parent, this);

        UpdatePlacingTriggerAreas(position, rotation);        
    }

    public void SpawnVisual(Vector3 position, Quaternion rotation, Transform parent)
    {
        DestroyGameObject();
        productGO = ProductGO.Spawn(true, position, rotation, parent, this);
    }

    public void OnPlayerTake(bool shoudSpawnVisual, Transform parent)
    {
        DestroyGameObject();
        if (shoudSpawnVisual) {
            SpawnVisual(parent);
        }     
    }

    private void SpawnVisual(Transform parent)
    {
        productGO = ProductGO.Spawn(true, Vector3.zero, Quaternion.identity, parent, this);
        productGO.transform.localPosition = productType.offset;
        productGO.transform.localRotation = Quaternion.identity;
        productGO.transform.localScale = Vector3.one;
    }

    private void UpdatePlacingTriggerAreas(Vector3 position, Quaternion rotation)
    {
        BoxCollider collider = productType.prefab.GetComponentInChildren<BoxCollider>();
        Vector3 center = position + collider.center;
        Vector3 halfExtents = collider.size / 2.2f;
        Collider[] hitColliders = Physics.OverlapBox(center, halfExtents, rotation, ShopData.instance.shelfTriggerLayer);
        
        for(int i = 0; i < hitColliders.Length; i++) {
            IPlacingTriggerArea placingTriggerArea = hitColliders[i].GetComponentInParent<IPlacingTriggerArea>();
            if (placingTriggerArea != null && !placingTriggerAreas.Contains(placingTriggerArea)){
                placingTriggerAreas.Add(placingTriggerArea);
            }
            placingTriggerArea = hitColliders[i].GetComponent<IPlacingTriggerArea>();
            if (placingTriggerArea != null && !placingTriggerAreas.Contains(placingTriggerArea)) {
                placingTriggerAreas.Add(placingTriggerArea);
            }
        }
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
        foreach(IPlacingTriggerArea triggerArea in placingTriggerAreas) {
            triggerArea.OnProductTakenFromArea(this);
        }
        placingTriggerAreas.Clear();
        if (productGO != null) {
            GameObject.Destroy(productGO.gameObject);
        }
    }

    public void RemoveProductFromGame(bool shouldRemoveFromProductsList)
    {
        if (productGO != null)
            GameObject.Destroy(productGO.gameObject);
        if (shouldRemoveFromProductsList && ProductsData.instance.productsSpawned.Contains(this))
            ProductsData.instance.productsSpawned.Remove(this);
    }

    public void SpawnVisualSavePickup(Transform parent)
    {
        SpawnVisual(parent);
    }
}
