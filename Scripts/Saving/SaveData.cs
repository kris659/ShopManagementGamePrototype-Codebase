using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public PlayerSaveData playerData;
    public ShopSaveData shopData;
    public WorkerSaveData[] workersData;
    public TimeSaveData timeData;
    public BuildingSaveData[] buildingsData;
    public ProductSaveData[] productsData;
    public ContainerSaveData[] containersData;
    public VehicleSaveData[] vehiclesData;
    public OnlineOrdersManagerSaveData onlineOrdersData;
    public PricesSaveData priceSaveData;
    public TasksSaveData tasksSaveData;
    public FurnitureBoxSaveData[] furnitureBoxData;


    public int[] storageManagerProducts;

    public SaveData(PlayerSaveData playerData, ShopSaveData shopData, WorkerSaveData[] workersData, TimeSaveData timeData, BuildingSaveData[] buildingsData, ProductSaveData[] productsData, ContainerSaveData[] containersData, FurnitureBoxSaveData[] furnitureBoxData, VehicleSaveData[] vehiclesData, PricesSaveData priceSaveData, TasksSaveData tasksData, OnlineOrdersManagerSaveData onlineOrdersData)
    {
        this.playerData = playerData;
        this.shopData = shopData;
        this.workersData = workersData;
        this.timeData = timeData;
        this.buildingsData = buildingsData;
        this.productsData = productsData;
        this.vehiclesData = vehiclesData;
        this.containersData = containersData;
        this.priceSaveData = priceSaveData;
        this.tasksSaveData = tasksData;
        this.onlineOrdersData = onlineOrdersData;
        this.furnitureBoxData = furnitureBoxData;
    }    
}
[System.Serializable]
public class PlayerSaveData
{
    public Vector3 position;
    public float playerMoney;
    public int vehicleIndex;
    public int[] pickablesTypeID;
    public int[] pickableID;

    public PlayerSaveData(Vector3 position, float playerMoney, int vehicleIndex, int[] pickablesTypeID, int[] pickableID)
    {
        this.position = position;
        this.playerMoney = playerMoney;
        this.vehicleIndex = vehicleIndex;
        this.pickablesTypeID = pickablesTypeID;
        this.pickableID = pickableID;
    }
}

[System.Serializable]
public class ShopSaveData
{
    public string shopName;
    public bool isShopOpen;
    public bool[] unlockedCars;
    public bool[] unlockedLand;
    public bool[] unlockedLicenses;
    public bool[] isLocationUnlocked;
    public float[] shopPopularityValues;
    public int billboardIndex;

    public ShopSaveData(string shopName, bool isShopOpen, bool[] unlockedCars, bool[] unlockedLand, bool[] unlockedLicenses, bool[] isLocationUnlocked, float[] shopPopularityValues, int billboardIndex)
    {
        this.unlockedCars = unlockedCars;
        this.unlockedLand = unlockedLand;
        this.shopName = shopName;
        this.isShopOpen = isShopOpen;
        this.unlockedLicenses = unlockedLicenses;
        this.isLocationUnlocked = isLocationUnlocked;
        this.shopPopularityValues = shopPopularityValues;
        this.billboardIndex = billboardIndex;
    }
}

[System.Serializable]
public class WorkerSaveData
{
    public Vector3 position;

    public string name;
    public int task;
    public int wage;
    public bool isFemale;
    public int containerIndex;

    public WorkerSaveData(Vector3 position, string name, int task, int wage, bool isFemale, int containerIndex)
    {
        this.position = position;
        this.name = name;
        this.task = task;
        this.wage = wage;
        this.isFemale = isFemale;
        this.containerIndex = containerIndex;   
    }
}

[System.Serializable]
public class TimeSaveData
{
    public int day;
    public int hour;
    public int minute;

    public TimeSaveData(int day, int hour, int minute)
    {
        this.day = day;
        this.hour = hour;
        this.minute = minute;
    }
}

[System.Serializable]
public class BuildingSaveData
{
    public int buildingIndex;

    public Vector3 position;
    public Quaternion rotation;

    public int[] additionalBuildingData;

    public BuildingSaveData(Vector3 position, Quaternion rotation, int buildingIndex, int[] additionalData)
    {
        this.position = position;
        this.rotation = rotation;
        this.buildingIndex = buildingIndex;
        this.additionalBuildingData = additionalData;
    }
}

[System.Serializable]
public class ProductSaveData
{
    public int productTypeIndex;
    public bool isPhysxSpawned;
    public Vector3 position;
    public Quaternion rotation;

    public ProductSaveData(int productTypeIndex, bool isPhysxSpawned, Vector3 position, Quaternion rotation)
    {
        this.productTypeIndex = productTypeIndex;
        this.isPhysxSpawned = isPhysxSpawned;
        this.position = position;
        this.rotation = rotation;
    }
}

[System.Serializable]
public class FurnitureBoxSaveData
{
    public BuildingSaveData buildingSaveData;
    public bool isPhysxSpawned;
    public Vector3 position;
    public Quaternion rotation;

    public FurnitureBoxSaveData(BuildingSaveData buildingSaveData, bool isPhysxSpawned, Vector3 position, Quaternion rotation)
    {
        this.buildingSaveData = buildingSaveData;
        this.isPhysxSpawned = isPhysxSpawned;
        this.position = position;
        this.rotation = rotation;
    }
}

[System.Serializable]
public class ContainerSaveData
{
    public int containerTypeIndex;
    public bool isPhysxSpawned;
    public bool isOpen;
    public Vector3 position;
    public Quaternion rotation;

    public int[] productsInContainerIndexes;
    public Vector3[] productsInContainerPositions;
    public Quaternion[] productsInContainerRotations;

    public ContainerSaveData(int containerTypeIndex, bool isPhysxSpawned, bool isOpen, Vector3 position, Quaternion rotation, int[] productsInContainerIndexes, Vector3[] productsInContainerPositions, Quaternion[] productsInContainerRotations)
    {
        this.containerTypeIndex = containerTypeIndex;
        this.isPhysxSpawned = isPhysxSpawned;
        this.isOpen = isOpen;
        this.position = position;
        this.rotation = rotation;
        this.productsInContainerIndexes = productsInContainerIndexes;
        this.productsInContainerPositions = productsInContainerPositions;
        this.productsInContainerRotations = productsInContainerRotations;
    }
}

[System.Serializable]
public class VehicleSaveData
{
    public int prefabIndex;
    public Vector3 position;
    public Quaternion rotation;
    public bool isUnlocked;

    public VehicleSaveData(Vector3 position, Quaternion rotation, int prefabIndex, bool isUnlocked)
    {
        this.prefabIndex = prefabIndex;
        this.position = position;
        this.rotation = rotation;
        this.isUnlocked = isUnlocked;
    }
}

[System.Serializable]
public class PricesSaveData
{
    public float[] productSellingPrices;
    public int[] wholesalePriceChanges;
    public int[] marketPriceChanges;

    public int[] wholesaleLowestChanges;
    public int[] marketLowestChanges;

    public int[] wholesaleHighestChanges;
    public int[] marketHighestChanges;

    public int[] currentChangedPricesIndexes;
    public int[] currentChangedMarketPricesValues;
    public int[] currentChangedWholesalePricesValues;
    public int[] currentChangedLowestHighest;

    public PricesSaveData(float[] productSellingPrices, int[] wholesalePriceChanges, int[] marketPriceChanges, int[] wholesaleLowestChanges, int[] marketLowestChanges, int[] wholesaleHighestChanges, int[] marketHighestChanges, int[] currentChangedPricesIndexes, int[] currentChangedMarketPricesValues, int[] currentChangedWholesalePricesValues, int[] currentChangedLowestHighest)
    {
        this.productSellingPrices = productSellingPrices;
        this.wholesalePriceChanges = wholesalePriceChanges;
        this.marketPriceChanges = marketPriceChanges;
        this.wholesaleLowestChanges = wholesaleLowestChanges;
        this.marketLowestChanges = marketLowestChanges;
        this.wholesaleHighestChanges = wholesaleHighestChanges;
        this.marketHighestChanges = marketHighestChanges;
        this.currentChangedPricesIndexes = currentChangedPricesIndexes;
        this.currentChangedMarketPricesValues = currentChangedMarketPricesValues;
        this.currentChangedWholesalePricesValues = currentChangedWholesalePricesValues;
        this.currentChangedLowestHighest = currentChangedLowestHighest;
    }    
}

[System.Serializable]
public class TasksSaveData
{
    public Vector2Int[] tasksAlreadyUsed;
    public TaskSaveData[] currentTasksList;

    public TasksSaveData(Vector2Int[] tasksUsed, TaskSaveData[] currentTasksList)
    {
        this.tasksAlreadyUsed = tasksUsed;
        this.currentTasksList = currentTasksList;
    }
}

[System.Serializable]
public class TaskSaveData
{
    public int tier;
    public TaskType taskType;
    public int progress;
    public int[] additionalInfo;
    public bool wasRewardTaken;

    public TaskSaveData(int tier, TaskType taskType, int progress, int[] additionalInfo, bool wasRewardTaken)
    {
        this.tier = tier;
        this.taskType = taskType;
        this.progress = progress;
        this.additionalInfo = additionalInfo;
        this.wasRewardTaken = wasRewardTaken;
    }
}

[System.Serializable]
public class OnlineOrdersManagerSaveData
{
    public int selectedDeliveryType;
    public OnlineOrderSaveData[] onlineOrdersData;

    public OnlineOrdersManagerSaveData(int selectedDeliveryType, OnlineOrderSaveData[] onlineOrdersData)
    {
        this.selectedDeliveryType = selectedDeliveryType;
        this.onlineOrdersData = onlineOrdersData;
    }
}



[System.Serializable]
public class OnlineOrderSaveData
{
    public int houseIndex;
    public int productType;
    public int productsAmount;
    public int price;
    public int timeLeft;

    public OnlineOrderSaveData(int houseIndex, int productType, int productsAmount, int price, int timeLeft)
    {
        this.houseIndex = houseIndex;
        this.productType = productType;
        this.productsAmount = productsAmount;
        this.price = price;
        this.timeLeft = timeLeft;
    }
}

