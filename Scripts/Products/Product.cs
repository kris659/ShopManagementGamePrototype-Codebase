using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Product
{
    public ProductGO productGO;
    public ProductSO productType;

    public bool isTakenByCustomer;
    public bool isInClosedContainer;
    public bool isContainerClosed;
    public Shelf shelf;
    private BoxCollider collider;

    private List<Product> productsInContainer = new List<Product>();
    private List<Vector3> positionsInContainer = new List<Vector3>();
    private List<Vector3> rotationsInContainer = new List<Vector3>();

    public Product(int typeIndex)
    {
        this.productType = SOData.productsList[typeIndex];
    }

    public Product(int typeIndex, Vector3 position, Quaternion rotation, bool isTakenByCustomer = false, int shelfIndex = -1)
    {
        this.productType = SOData.productsList[typeIndex];
        this.isTakenByCustomer = isTakenByCustomer;


        bool isOnShelf = (shelfIndex != -1);

        if (isTakenByCustomer)
            return;
        if (isOnShelf) {
            shelf = ShopData.instance.shelvesList[shelfIndex];
            shelf.AddProduct(this);
        }
        productGO = ProductGO.Spawn(isOnShelf, position, rotation, null, this, isOnShelf);
    }


    public void Place(Vector3 position, Quaternion rotation, Transform parent)
    {
        isInClosedContainer = false;
        if (productGO != null)
            GameObject.Destroy(productGO.gameObject);
        productGO = ProductGO.Spawn(false, position, rotation, parent, this, true);

        if (productType.canBeSold) {
            shelf = GetShelf(position, rotation);
            if (shelf != null) {
                shelf.AddProduct(this);
            }
        }
        if (productType.isContainer) {
            HandleContainterPlace(position, rotation, parent);
        }
    }

    public void OnPlayerTake(bool shoudSpawnVisual, Transform parent)
    {
        if (productType.isContainer)
            HandleContainerTake(parent);
        GameObject.Destroy(productGO.gameObject);

        if (shoudSpawnVisual) {
            productGO = ProductGO.Spawn(true, Vector3.zero, Quaternion.identity, parent, this);
            productGO.transform.localPosition = productType.offset;
            productGO.transform.localRotation = Quaternion.identity;
            productGO.transform.localScale = Vector3.one;
        }

        if (shelf != null) {
            shelf.TakeProduct(this);
            shelf = null;
        }        
    }

    public void OnCustomerTake()
    {
        isTakenByCustomer = true;
        GameObject.Destroy(productGO.gameObject);
        shelf = null;
    }

    public void CloseInContainer()
    {
        isInClosedContainer = true;
        Destroy(productGO.gameObject);
    }
    public void Destroy(bool shouldRemoveFromProductsList = true)
    {
        if (productGO != null)
            GameObject.Destroy(productGO.gameObject);
        if (shouldRemoveFromProductsList && ProductsData.instance.products.Contains(this))
            ProductsData.instance.products.Remove(this);
    }
    private Shelf GetShelf(Vector3 position, Quaternion rotation)
    {
        Shelf shelf = null;
        collider = productType.prefab.GetComponentInChildren<BoxCollider>();
        Vector3 center = position + collider.center;// + collider.transform.localPosition;
        Vector3 halfExtents = collider.size / 2.2f;
        Collider[] hitColliders = Physics.OverlapBox(center, halfExtents, rotation, ShopData.instance.shelfTriggerLayer);
        //Debug.Log(hitColliders.Length);
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

    public void InitContainer(int productTypeIndexToSpawn, int productAmount, Transform positionsParent)
    {
        for(int i = 0;i < productAmount; i++) {
            Vector3 position = positionsParent.GetChild(i).localPosition;
            Vector3 rotation = positionsParent.GetChild(i).localEulerAngles;
            productsInContainer.Add(new Product(productTypeIndexToSpawn));
            positionsInContainer.Add(position);
            rotationsInContainer.Add(rotation);
        }
        productGO.container.Init(false, productsInContainer, positionsInContainer, rotationsInContainer);
    }

    private void HandleContainerTake(Transform parent)
    {
        isContainerClosed = !productGO.container.isOpen;
        productGO.GetProductsInContainerData(out productsInContainer, out positionsInContainer, out rotationsInContainer);
        Debug.Log("Products in container count: " + productsInContainer.Count);
        if (!isContainerClosed && productGO.container.isOpen) {
            for (int i = 0; i < productsInContainer.Count; i++) {
                productsInContainer[i].OnPlayerTake(true, parent);
            }
        }
    }
    private void HandleContainterPlace(Vector3 containerPosition, Quaternion containerRotation, Transform parent)
    {
        productGO.container.Init(!isContainerClosed, productsInContainer, positionsInContainer, rotationsInContainer);
    }

    public ProductsSaveData CreateSaveData()
    {
        Vector3 position = Vector3.zero;
        Quaternion rotation = Quaternion.identity;
        int productTypeIndex = SOData.GetProductIndex(productType);
        int shelfIndex = -1;
        if (!isTakenByCustomer) { // Currently impossible becouse of ProductData
            position = productGO.transform.position;
            rotation = productGO.transform.rotation;
        }
        if (shelf != null) {
            shelfIndex = shelf.shelfIndex;
        }

        productGO.GetProductsInContainerData(out List<Product> productsInContainer, out List<Vector3> productsPositions, out List<Vector3> productsRotations);
        ProductsSaveData[] productsInContainerSaveData = new ProductsSaveData[productsInContainer.Count];
        for(int i = 0; i < productsInContainer.Count; i++) {
            productsInContainerSaveData[i] = productsInContainer[i].CreateSaveData();
        }

        return new ProductsSaveData(position, rotation, productTypeIndex, isTakenByCustomer, shelfIndex,
            productsInContainerSaveData, productsPositions.ToArray(), productsRotations.ToArray());
    }
}
