using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProductsData : MonoBehaviour, IPlacingTriggerArea
{
    [System.Serializable]
    public struct ProductsCategory
    {
        public string name;
        public ProductSO[] products;
    }
    [SerializeField] private List<ProductsCategory> productsCategories;
    [SerializeField] private bool shouldSpawnOnSpawn = true;
    [SerializeField] private List<Transform> startingSpawnProductPositions = new List<Transform>();
    [SerializeField] private List<Transform> startingSpawnContainerPositions = new List<Transform>();
    [SerializeField] private List<int> startingSpawnProductTypes = new List<int>();
    [SerializeField] private List<int> startingSpawnContainerTypes = new List<int>();

    public static ProductsData instance;
    public List<Product> productsSpawned = new List<Product>();
    public List<Container> containersSpawned = new List<Container>();

    public List<Product> productsInShop = new List<Product>();
    public List<Container> containersInShop = new List<Container>();

    public LayerMask productsLayerMask;

    private void Awake()
    {
        if (instance != null) {
            Debug.LogError("Multiple ProductsData");
            Destroy(this);
        }
        instance = this;

        TriggerHandler triggerHandler = GetComponentInChildren<TriggerHandler>();
        triggerHandler.triggerEnter += EnterShopArea;
        triggerHandler.triggerExit += LeaveShopArea;
    }

    public Container GetContainerForRestocker()
    {
        foreach(Container container in containersInShop) {
            if(!container.isContainerOnShelf)
                return container;
        }
        return null;
    }

    public void OnProductPlacedInArea(Product product){ }

    public void OnProductTakenFromArea(Product product){ }

    public void OnContainerPlacedInArea(Container container) {
        if (!containersInShop.Contains(container)) {
            Debug.Log("Container placed in area");
            containersInShop.Add(container);
        }
    }

    public void OnContainerTakenFromArea(Container container) {
        if (containersInShop.Contains(container)) {
            Debug.Log("Container taken from area");
            containersInShop.Remove(container);
        }
    }

    private void EnterShopArea(Collider other)
    {
        if (other.gameObject.name != "ContainerTrigger")
            return;
        if (other.transform.parent != null && other.transform.parent.TryGetComponent(out ContainerGO containerGO)) {
            if (!containerGO.container.placingTriggerAreas.Contains(this))
                containerGO.container.placingTriggerAreas.Add(this);
            OnContainerPlacedInArea(containerGO.container);
        }
    }
    private void LeaveShopArea(Collider other)
    {
        if (other.gameObject.name != "ContainerTrigger")
            return;
        if (other.transform.parent != null && other.transform.parent.TryGetComponent(out ContainerGO containerGO)) {
            if (containerGO.container.placingTriggerAreas.Contains(this))
                containerGO.container.placingTriggerAreas.Remove(this);
            else {
                Debug.LogError("Sus");
            }
            OnContainerTakenFromArea(containerGO.container);
        }
    }

    public ProductSaveData[] GetProductsSaveData()
    {
        ProductSaveData[] productsSaveData = new ProductSaveData[productsSpawned.Count];

        for(int i = 0; i < productsSaveData.Length; i++) {
            if (productsSpawned[i].isTakenByCustomer || productsSpawned[i].isInClosedContainer) continue;
            productsSaveData[i] = productsSpawned[i].CreateSaveData();
        }
        return productsSaveData;
    }

    public ContainerSaveData[] GetContainersSaveData()
    {
        ContainerSaveData[] containersSaveData = new ContainerSaveData[containersSpawned.Count];

        for (int i = 0; i < containersSaveData.Length; i++) {
            if (productsSpawned[i].isTakenByCustomer || productsSpawned[i].isInClosedContainer) 
                continue;
            containersSaveData[i] = containersSpawned[i].CreateSaveData();
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
                productsInContainer.Add(productsSpawned[containersSaveData[i].productsInContainerIndexes[j]]);
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

    public string[] GetCategoryNames()
    {
        string[] categoryNames = new string[productsCategories.Count];

        for(int i = 0; i < categoryNames.Length; i++) {
            categoryNames[i] = productsCategories[i].name;
        }
        return categoryNames;
    }

    public ProductSO[] GetCategoryProducts(int categoryIndex)
    {
        return productsCategories[categoryIndex].products;
    }

    public void GetInTriggerPositions(List<Product> products, BoxCollider boxCollider, out List<Vector3> positions, bool prioritizeRows)
    {
        List<ProductSO> productSOs = new List<ProductSO>();
        foreach(Product product in products) {
            productSOs.Add(product.productType);
        }
        GetInTriggerPositions(productSOs, boxCollider, out positions, prioritizeRows);
    }

    public void GetInTriggerPositions(List<ProductSO> products, BoxCollider boxCollider, out List<Vector3> positions, bool prioritizeRows)
    {
        Vector3 maxPosition = new Vector3(boxCollider.size.x * boxCollider.transform.localScale.x, 0, boxCollider.size.z * boxCollider.transform.localScale.z);
        if(!prioritizeRows) {
            maxPosition = new Vector3(maxPosition.z, 0, maxPosition.x);
        }
        Vector3 currentPosition = Vector3.zero;
        positions = new List<Vector3>();
        float nextPositionZ = 0;
        for(int i = 0; i < products.Count; i++) {
            BoxCollider productCollider = products[i].prefab.GetComponentInChildren<BoxCollider>();
            float x = productCollider.size.x;
            float z = productCollider.size.z;
            if(!prioritizeRows ) {
                x = productCollider.size.z;
                z = productCollider.size.x;
            }

            if(currentPosition.x + x > maxPosition.x) { // Next row
                currentPosition.x = 0;
                currentPosition.z += nextPositionZ;
                nextPositionZ = 0;
            }
            if(currentPosition.z + z < maxPosition.z) {                
                if (prioritizeRows)
                    positions.Add(currentPosition + new Vector3(x / 2, 0, z / 2));
                else
                    positions.Add(new Vector3(currentPosition.z + z / 2, 0, currentPosition.x + z / 2));
                currentPosition.x += x;
                nextPositionZ = Mathf.Max(nextPositionZ, z);
            }
            else {                
                if(positions.Count == 0) {
                    Debug.Log("To big to spawn");                    
                    //positions.Add(new Vector3(0, 0, 0));
                }
                return;
            }
        }
    }

    public void DestroyAll()
    {
        for(int i = 0; i < productsSpawned.Count; i++)
            productsSpawned[i].RemoveProductFromGame(false);
        for (int i = 0; i < containersSpawned.Count; i++)
            containersSpawned[i].RemoveProductFromGame(false);
        productsSpawned.Clear();
        containersSpawned.Clear();
        productsInShop.Clear();
        containersInShop.Clear();
    }
}
