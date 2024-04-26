using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProductsData : MonoBehaviour
{
    [SerializeField] private bool shouldSpawnOnSpawn = true;
    [SerializeField] private List<Transform> startingSpawnProductPositions = new List<Transform>();
    [SerializeField] private List<Transform> startingSpawnContainerPositions = new List<Transform>();
    [SerializeField] private List<int> startingSpawnProductTypes = new List<int>();
    [SerializeField] private List<int> startingSpawnContainerTypes = new List<int>();

    public static ProductsData instance;
    public List<Product> products = new List<Product>();
    public List<Container> containers = new List<Container>();
    public LayerMask productsLayerMask;


    private void Start()
    {
        if (instance != null)
            Destroy(this);
        instance = this;
        //if (shouldSpawnOnSpawn){
        //    for (int i = 0; i < startingSpawnProductTypes.Count; i++) {
        //        products.Add(new Product(startingSpawnProductTypes[i], startingSpawnPositions[i].position, startingSpawnPositions[i].rotation));
        //    }
        //}
    }
    public ProductSaveData[] GetProductsSaveData()
    {
        ProductSaveData[] productsSaveData = new ProductSaveData[products.Count];

        for(int i = 0; i < productsSaveData.Length; i++) {
            if (products[i].isTakenByCustomer || products[i].isInClosedContainer) continue;
            productsSaveData[i] = products[i].CreateSaveData();
        }
        return productsSaveData;
    }

    public ContainerSaveData[] GetContainersSaveData()
    {
        ContainerSaveData[] containersSaveData = new ContainerSaveData[containers.Count];

        for (int i = 0; i < containersSaveData.Length; i++) {
            if (products[i].isTakenByCustomer || products[i].isInClosedContainer) continue;
            containersSaveData[i] = containers[i].CreateSaveData();
        }
        return containersSaveData;
    }

    public void LoadFromSaveData(ProductSaveData[] productsSaveData, ContainerSaveData[] containersSaveData)
    {
        for(int i = 0;i < productsSaveData.Length; i++) {
            new Product(productsSaveData[i].productTypeIndex, productsSaveData[i].isPhysxSpawned, productsSaveData[i].position, productsSaveData[i].rotation);
        }
        for(int i = 0; i < containersSaveData.Length; i++) {
            List<Product> productsInContainer = new List<Product>(); 
            List<Vector3> productsPositions = new List<Vector3>(); 
            List<Vector3> productsRotations = new List<Vector3>();

            for(int j = 0; j < containersSaveData[i].productsInContainerIndexes.Length; j++) {
                productsInContainer.Add(products[containersSaveData[i].productsInContainerIndexes[j]]);
                productsPositions.Add(containersSaveData[i].productsInContainerPositions[j]);
                productsRotations.Add(containersSaveData[i].productsInContainerRotations[j].eulerAngles);
            }
            new Container(containersSaveData[i].containerTypeIndex, containersSaveData[i].isPhysxSpawned, containersSaveData[i].position,
                containersSaveData[i].rotation, containersSaveData[i].isOpen, productsInContainer, productsPositions, productsRotations);
        }
        if (shouldSpawnOnSpawn) {
            for (int i = 0; i < startingSpawnProductTypes.Count; i++) {
                new Product(startingSpawnProductTypes[i], true, startingSpawnProductPositions[i].position, startingSpawnProductPositions[i].rotation);
            }
            for (int i = 0; i < startingSpawnContainerTypes.Count; i++) {
                new Container(startingSpawnContainerTypes[i], true, startingSpawnContainerPositions[i].position, startingSpawnContainerPositions[i].rotation, false);
            }
        }
    }

    public void DestroyAll()
    {
        for(int i = 0; i < products.Count; i++)
            products[i].RemoveProductFromGame(false);
        products.Clear();
    }
}
