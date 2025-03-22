using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlacingTriggerAreaParent))]
public class Shelf : Building
{
    public List<Product> ProductsOnShelf => placingTriggerAreaParent.products;
    public int shelfTypeIndex;
    public int shelfIndex;

    public Transform customerDestination;
    [HideInInspector]
    public PlacingTriggerArea[] shelfTriggers;

    PlacingTriggerAreaParent placingTriggerAreaParent;
    private void Awake()
    {
        shelfTriggers = transform.GetComponentsInChildren<PlacingTriggerArea>();
        placingTriggerAreaParent = GetComponent<PlacingTriggerAreaParent>();

        placingTriggerAreaParent.OnProductTriggerEnterEvent += OnProductPlacedInArea;
        placingTriggerAreaParent.OnProductTriggerExitEvent += OnProductTakenFromArea;
        placingTriggerAreaParent.OnContainerTriggerEnterEvent += OnContainerPlacedInArea;
        placingTriggerAreaParent.OnContainerTriggerExitEvent += OnContainerTakenFromArea;
    }

    public override void Build()
    {
        base.Build();
        shelfIndex = ShopData.instance.AddShelf(this);
    }

    public void OnProductPlacedInArea(Product product)
    {
        ShopData.instance.UpdateShelfStatus(this);
        ProductsData.instance.OnProductPlacedOnShelf(product);
    }

    public void OnProductTakenFromArea(Product product)
    {
        ShopData.instance.UpdateShelfStatus(this);      
        ProductsData.instance.OnProductTakenFromShelf(product);
    }

    public void OnContainerPlacedInArea(Container container) {
        container.isContainerOnShelf = true;
    }
    public void OnContainerTakenFromArea(Container container) {
        container.isContainerOnShelf = false;
    }

    public Product GetRandomProduct()
    {
        if(ProductsOnShelf.Count > 0) {
            int index = Random.Range(0, ProductsOnShelf.Count);
            Product product = ProductsOnShelf[index];
            return product;
        }
        return null;
    }

    public override int[] GetAdditionalSaveData()
    {
        int[] shelfTriggersSaveData = new int[shelfTriggers.Length];
        for(int i = 0; i < shelfTriggersSaveData.Length; i++) {
            shelfTriggersSaveData[i] = SOData.GetProductIndex(shelfTriggers[i].currentProduct);
        }
        return shelfTriggersSaveData;
    }

    public override void LoadAdditionalSaveData(int[] additionalSaveData)
    {
        for (int i = 0; i < additionalSaveData.Length; i++) {
            if (additionalSaveData[i] >= 0) {
                shelfTriggers[i].currentProduct = SOData.productsList[additionalSaveData[i]];
                shelfTriggers[i].UpdateCurrentProduct();
            }
        }
    }

    public void OnDestroy()
    {
        ShopData.instance.RemoveShelf(this);
        //ProductsOnShelf.Clear();
    }
}
