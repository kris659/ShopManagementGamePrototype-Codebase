using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ProductsData : MonoBehaviour
{
    [System.Serializable]
    public struct ProductsCategory
    {
        public string name;
        public ProductSO[] products;
    }
    [System.Serializable]
    public struct LicenseData
    {
        public string name;
        public bool isUnlockedOnStart;
        public int unlockPrice;
        public ProductSO[] products;
    }

    public static ProductsData instance;

    [SerializeField] public List<ProductsCategory> productsCategories;
    [SerializeField] public List<LicenseData> productsLicenses;
    [SerializeField] public List<bool> unlockedProductsLicenses;
    [SerializeField] private bool shouldSpawnOnSpawn = true;
    [SerializeField] private List<Transform> startingSpawnProductPositions = new List<Transform>();
    [SerializeField] private List<Transform> startingSpawnContainerPositions = new List<Transform>();
    [SerializeField] private List<int> startingSpawnProductTypes = new List<int>();
    [SerializeField] private List<int> startingSpawnContainerTypes = new List<int>();

    public List<Product> productsSpawned = new List<Product>();
    public List<Container> containersSpawned = new List<Container>();
    public List<FurnitureBox> furnitureBoxesSpawned = new List<FurnitureBox>();

    public List<Product> ProductsInShop => placingTriggerAreaParent.products;
    public List<Container> ContainersInShop => placingTriggerAreaParent.containers;

    public LayerMask productsLayerMask;

    public List<Product> productsOnShelves = new List<Product>();
    public int[] productsOnShelvesCount;
    public int differentProductTypesOnShelvesCount;
    public int totalProductsTypeCount;

    PlacingTriggerAreaParent placingTriggerAreaParent;
    private void Awake()
    {
        if (instance != null) {
            Debug.LogError("Multiple ProductsData");
            Destroy(this);
        }
        instance = this;

        placingTriggerAreaParent = GetComponent<PlacingTriggerAreaParent>();

        placingTriggerAreaParent.OnProductTriggerEnterEvent += OnProductPlacedInArea;
        placingTriggerAreaParent.OnProductTriggerExitEvent += OnProductTakenFromArea;

        placingTriggerAreaParent.OnContainerTriggerEnterEvent += OnContainerPlacedInArea;
        placingTriggerAreaParent.OnContainerTriggerExitEvent += OnContainerTakenFromArea;

        totalProductsTypeCount = 0;
        foreach(LicenseData licenseData in productsLicenses) {
            totalProductsTypeCount += licenseData.products.Length;
        }
    }

    public void OnProductPlacedOnShelf(Product product) {
        if (!productsOnShelves.Contains(product)) {
            productsOnShelves.Add(product);
            productsOnShelvesCount[product.productTypeIndex]++;
            if (productsOnShelvesCount[product.productTypeIndex] == 1) {
                differentProductTypesOnShelvesCount++;
                TasksManager.instance.ProgressTasks(TaskType.HaveDifferentProducts, differentProductTypesOnShelvesCount);
            }
        }
    }

    public void OnProductTakenFromShelf(Product product) {
        if (productsOnShelves.Contains(product)) {
            productsOnShelves.Remove(product);
            productsOnShelvesCount[product.productTypeIndex]--;
            if (productsOnShelvesCount[product.productTypeIndex] == 0) {
                differentProductTypesOnShelvesCount--;
                TasksManager.instance.ProgressTasks(TaskType.HaveDifferentProducts, differentProductTypesOnShelvesCount);
            }
        }
    }

    public bool IsProductsUnlocked(int productIndex)
    {
        return IsProductsUnlocked(SOData.productsList[productIndex]);
    }
    public bool IsProductsUnlocked(ProductSO productSO)
    {
        for (int i = 0; i < unlockedProductsLicenses.Count; i++) {
            if (unlockedProductsLicenses[i] && productsLicenses[i].products.Contains(productSO))
                return true;
        }
        return false;
    }

    public int GetRandomProduct()
    {
        int productsCount = 0;
        for(int i = 0; i < unlockedProductsLicenses.Count; i++) {
            if (unlockedProductsLicenses[i])
                productsCount += productsLicenses[i].products.Length;
        }
        int index = Random.Range(0, productsCount);
        ProductSO product = null;
        for (int i = 0; i < unlockedProductsLicenses.Count; i++) {
            if (unlockedProductsLicenses[i]) {
                if (index >= productsLicenses[i].products.Length)
                    index -= productsLicenses[i].products.Length;
                else {
                    product = productsLicenses[i].products[index];
                    break;
                }

            }
        }
        return SOData.GetProductIndex(product);
    }

    public void OnProductPlacedInArea(Product product){  }

    public void OnProductTakenFromArea(Product product){  }

    public void OnContainerPlacedInArea(Container container) {  }

    public void OnContainerTakenFromArea(Container container) {  }

    public IPickable GetPickableByID(int pickableTypeID, int pickableID)
    {       
        if(pickableTypeID >= 0 && pickableTypeID < 999 && productsSpawned.Count > pickableID)
            return productsSpawned[pickableID];
        if (pickableTypeID < 0 && containersSpawned.Count > pickableID)
            return containersSpawned[pickableID];
        if (pickableTypeID == 999 && furnitureBoxesSpawned.Count > pickableID)
            return furnitureBoxesSpawned[pickableID];

        Debug.LogError("Pickable NOT FOUND! TypeID: " + pickableTypeID + " ID: " + pickableID);
        return null;
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
        if (unlockedProductsLicenses[categoryIndex]) {
            return productsLicenses[categoryIndex].products;
        }
        return new ProductSO[0];
    }

    public void GetInTriggerPositions(ProductSO productType, BoxCollider boxCollider, out List<Vector3> positions, bool allowMultipleLevels, float spaceBeetween = 0.003f, bool prioritizeRows = false)
    {
        List<ProductSO> products = Enumerable.Repeat(productType, 100).ToList();
        GetInTriggerPositions(products, boxCollider, out positions, spaceBeetween, prioritizeRows);

        if (!allowMultipleLevels)
            return;

        BoxCollider productCollider = productType.prefab.GetComponentInChildren<BoxCollider>();

        float colliderHeight = productCollider.size.y;
        float currentHeight = colliderHeight;
        float boxHeight = boxCollider.size.y * boxCollider.transform.localScale.y;
        int initialListSize = positions.Count;

        while (currentHeight + colliderHeight < boxHeight) {
            for (int i = 0; i < initialListSize; i++) {
                positions.Add(positions[positions.Count - initialListSize] + new Vector3(0, colliderHeight, 0));
            }
            currentHeight += colliderHeight;
        }
    }

    public void GetInTriggerPositions(List<Product> products, BoxCollider boxCollider, out List<Vector3> positions, float spaceBeetween = 0.003f, bool prioritizeRows = false)
    {
        List<ProductSO> productSOs = new List<ProductSO>();
        foreach(Product product in products) {
            productSOs.Add(product.productType);
        }
        GetInTriggerPositions(productSOs, boxCollider, out positions, spaceBeetween, prioritizeRows);
    }

    public void GetInTriggerPositions(List<ProductSO> products, BoxCollider boxCollider, out List<Vector3> positions, float spaceBeetween = 0.003f, bool prioritizeRows = false)
    {
        Vector3 maxPosition = new Vector3(boxCollider.size.x * boxCollider.transform.localScale.x, 0, boxCollider.size.z * boxCollider.transform.localScale.z);
        if(!prioritizeRows) {
            maxPosition = new Vector3(maxPosition.z, 0, maxPosition.x);
        }
        Vector3 currentPosition = Vector3.zero;
        Vector3 offset = new Vector3(boxCollider.size.x * boxCollider.transform.localScale.x / 2, 0, boxCollider.size.z * boxCollider.transform.localScale.z / 2);
        positions = new List<Vector3>();
        float nextPositionZ = 0;
        for(int i = 0; i < products.Count; i++) {
            BoxCollider productCollider = products[i].prefab.GetComponentInChildren<BoxCollider>();
            float x = productCollider.size.x + spaceBeetween;
            float z = productCollider.size.z + spaceBeetween;
            if(!prioritizeRows) {
                x = productCollider.size.z + spaceBeetween;
                z = productCollider.size.x + spaceBeetween;
            }

            if(currentPosition.x + x > maxPosition.x) { // Next row
                currentPosition.x = 0;
                currentPosition.z += nextPositionZ;
                nextPositionZ = 0;
            }
            if(currentPosition.z + z < maxPosition.z) {                
                if (prioritizeRows)
                    positions.Add(currentPosition + new Vector3(x / 2, 0, z / 2) - offset);
                else
                    positions.Add(new Vector3(currentPosition.z + z / 2, 0, currentPosition.x + x / 2) - offset);
                currentPosition.x += x;
                nextPositionZ = Mathf.Max(nextPositionZ, z);
            }
            else {                
                if(positions.Count == 0) {
                    Debug.Log("To big to spawn");                    
                }
                return;
            }
        }
    }

    public void OnUnlockLicenseButtonPressed(int licenseIndex)
    {
        if (!PlayerData.instance.CanAfford(productsLicenses[licenseIndex].unlockPrice))
            return;
        PlayerData.instance.TakeMoney(productsLicenses[licenseIndex].unlockPrice);
        unlockedProductsLicenses[licenseIndex] = true;
    }

    public ProductSaveData[] GetProductsSaveData()
    {
        ProductSaveData[] productsSaveData = new ProductSaveData[productsSpawned.Count];

        for (int i = 0; i < productsSaveData.Length; i++) {
            if (productsSpawned[i].isTakenByCustomer || productsSpawned[i].isInClosedContainer)
                continue;
            productsSaveData[i] = productsSpawned[i].CreateSaveData();
        }
        return productsSaveData;
    }

    public ContainerSaveData[] GetContainersSaveData()
    {
        ContainerSaveData[] containersSaveData = new ContainerSaveData[containersSpawned.Count];

        for (int i = 0; i < containersSaveData.Length; i++) {
            containersSaveData[i] = containersSpawned[i].CreateSaveData();
        }
        return containersSaveData;
    }

    public FurnitureBoxSaveData[] GetFurnitureBoxSaveData()
    {
        FurnitureBoxSaveData[] furnitureBoxSaveData = new FurnitureBoxSaveData[furnitureBoxesSpawned.Count];

        for (int i = 0; i < furnitureBoxSaveData.Length; i++) {
            furnitureBoxSaveData[i] = furnitureBoxesSpawned[i].CreateSaveData();
        }
        return furnitureBoxSaveData;
    }

    public void LoadFromSaveData(ProductSaveData[] productsSaveData, ContainerSaveData[] containersSaveData, FurnitureBoxSaveData[] furnitureBoxSaveData)
    {
        productsOnShelvesCount = new int[SOData.productsList.Length];
        differentProductTypesOnShelvesCount = 0;
        productsOnShelves.Clear();
        if (furnitureBoxSaveData == null) {
            furnitureBoxSaveData = new FurnitureBoxSaveData[0];
        }
        for (int i = 0; i < furnitureBoxSaveData.Length; i++) {
            new FurnitureBox(furnitureBoxSaveData[i].buildingSaveData, furnitureBoxSaveData[i].isPhysxSpawned, furnitureBoxSaveData[i].position, furnitureBoxSaveData[i].rotation);
        }
        for (int i = 0; i < productsSaveData.Length; i++) {
            new Product(productsSaveData[i].productTypeIndex, productsSaveData[i].isPhysxSpawned, productsSaveData[i].position, productsSaveData[i].rotation);
        }
        for (int i = 0; i < containersSaveData.Length; i++) {
            List<Product> productsInContainer = new List<Product>();
            List<Vector3> productsPositions = new List<Vector3>();
            List<Vector3> productsRotations = new List<Vector3>();

            for (int j = 0; j < containersSaveData[i].productsInContainerIndexes.Length; j++) {
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

    public void LoadSaveUnlockedLicenses(bool[] saveData)
    {
        unlockedProductsLicenses = new List<bool>();
        int i = 0;
        if (saveData != null) {
            for (; i < saveData.Length; i++) {
                unlockedProductsLicenses.Add(saveData[i]);
            }
        }

        for (; i < productsLicenses.Count; i++) {
            unlockedProductsLicenses.Add(productsLicenses[i].isUnlockedOnStart);
        }
    }

    public void DestroyAll()
    {
        for(int i = 0; i < productsSpawned.Count; i++)
            productsSpawned[i].RemoveFromGame(false);
        for (int i = 0; i < containersSpawned.Count; i++)
            containersSpawned[i].RemoveFromGame(false);
        for (int i = 0; i < furnitureBoxesSpawned.Count; i++)
            furnitureBoxesSpawned[i].RemoveFromGame(false);
        productsSpawned.Clear();
        containersSpawned.Clear();
        furnitureBoxesSpawned.Clear();
        ProductsInShop.Clear();
        ContainersInShop.Clear();
        productsOnShelvesCount = new int[SOData.productsList.Length];
        differentProductTypesOnShelvesCount = 0;
        productsOnShelves.Clear();
    }
}
