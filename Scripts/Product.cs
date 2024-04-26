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
    public Shelf shelf;

    public int HoldingLimit => productType.holdingLimit;
    public int HoldingLimitID => productTypeIndex;
    public BoxCollider BoxCollider => productType.prefab.GetComponentInChildren<BoxCollider>();

    public GameObject PreviewGameObject => GameObject.Instantiate(productType.visualPrefab);

    public Product(int typeIndex)
    {
        this.productTypeIndex = typeIndex;
        this.productType = SOData.productsList[typeIndex];
        ProductsData.instance.products.Add(this);
    }

    public Product(int typeIndex, bool isPhysxSpawned, Vector3 position, Quaternion rotation)
    {
        this.productTypeIndex = typeIndex;
        this.productType = SOData.productsList[typeIndex];
        ProductsData.instance.products.Add(this);
        if (isPhysxSpawned)
            Place(position, rotation, null);
    }

    public void Place(Vector3 position, Quaternion rotation, Transform parent)
    {
        DestroyGameObject();
        productGO = ProductGO.Spawn(false, position, rotation, parent, this);

        if (productType.canBeSold) {
            shelf = TryToGetShelf(position, rotation);
            if (shelf != null) {
                shelf.AddProduct(this);
            }
        }
    }

    public void OnPlayerTake(bool shoudSpawnVisual, Transform parent)
    {
        DestroyGameObject();
        if (shoudSpawnVisual) {
            productGO = ProductGO.Spawn(true, Vector3.zero, Quaternion.identity, parent, this);
            productGO.transform.localPosition = productType.offset;
            productGO.transform.localRotation = Quaternion.identity;
            productGO.transform.localScale = Vector3.one;
        }     
    }

    private Shelf TryToGetShelf(Vector3 position, Quaternion rotation)
    {
        Shelf shelf = null;
        BoxCollider collider = productType.prefab.GetComponentInChildren<BoxCollider>();
        Vector3 center = position + collider.center;
        Vector3 halfExtents = collider.size / 2.2f;
        Collider[] hitColliders = Physics.OverlapBox(center, halfExtents, rotation, ShopData.instance.shelfTriggerLayer);
        Collider closestCollider = GetClosestCollider(hitColliders, position);
        if (closestCollider != null)
            shelf = closestCollider.GetComponentInParent<Shelf>();
        return shelf;
    }

    private Collider GetClosestCollider(Collider[] hitColliders, Vector3 spawnPosition)
    {
        Collider collider = null;
        float distance = 100;
        for (int i = 0; i < hitColliders.Length; i++) {
            if (Vector3.Distance(hitColliders[i].transform.parent.position, spawnPosition) < distance) {
                collider = hitColliders[i];
                distance = Vector3.Distance(hitColliders[i].transform.parent.position, spawnPosition);
            }
        }
        return collider;
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
        if (shelf != null) {
            shelf.TakeProduct(this);
            shelf = null;
        }
        if (productGO != null) {
            GameObject.Destroy(productGO.gameObject);
        }
    }

    public void RemoveProductFromGame(bool shouldRemoveFromProductsList)
    {
        if (productGO != null)
            GameObject.Destroy(productGO.gameObject);
        if (shouldRemoveFromProductsList && ProductsData.instance.products.Contains(this))
            ProductsData.instance.products.Remove(this);
    }
}
