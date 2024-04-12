using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProductsData : MonoBehaviour
{
    [SerializeField] private bool shouldSpawnOnSpawn = true;
    [SerializeField] private List<Transform> startingSpawnPositions = new List<Transform>();
    [SerializeField] private List<int> startingSpawnProductTypes = new List<int>();

    public static ProductsData instance;
    public List<Product> products = new List<Product>();
    public LayerMask productsLayerMask;


    private void Start()
    {
        if (instance != null)
            Destroy(this);
        instance = this;
        if (shouldSpawnOnSpawn){
            for (int i = 0; i < startingSpawnPositions.Count; i++) {
                products.Add(new Product(startingSpawnProductTypes[i], startingSpawnPositions[i].position, startingSpawnPositions[i].rotation));
            }
        }
    }

    public ProductsSaveData[] GetProductsSaveData()
    {
        ProductsSaveData[] productsSaveData = new ProductsSaveData[products.Count];

        for(int i = 0; i < productsSaveData.Length; i++) {
            if (products[i].isTakenByCustomer || products[i].isInClosedContainer) continue;
            productsSaveData[i] = products[i].CreateSaveData();
        }
        return productsSaveData;
    }

    public void LoadFromSaveData(ProductsSaveData[] productsSaveData)
    {
        for(int i = 0;i < productsSaveData.Length; i++) {
            products.Add(new Product(productsSaveData[i].productTypeIndex, productsSaveData[i].position, productsSaveData[i].rotation, 
                productsSaveData[i].isTakenByCustomer, productsSaveData[i].shelfIndex));
        }
    }

    public void DestroyAll()
    {
        for(int i =0;i<products.Count;i++)
            products[i].Destroy(false);
        products.Clear();
    }
}
