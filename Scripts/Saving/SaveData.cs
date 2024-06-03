using UnityEngine;

[System.Serializable]
public class SaveData
{
    public PlayerSaveData playerData;
    public ShopSaveData shopData;
    public WorkerSaveData[] workersData;
    public TimeSaveData timeData;
    public ShelfSaveData[] shelvesData;
    public WallSaveData[] wallsData;
    public RegisterSaveData[] registersData;
    public ProductSaveData[] productsData;
    public ContainerSaveData[] containersData;
    public VehicleSaveData[] vehiclesData;   

    public int[] storageManagerProducts;

    public SaveData(PlayerSaveData playerData, ShopSaveData shopData, WorkerSaveData[] workersData, TimeSaveData timeData, ShelfSaveData[] shelvesData, WallSaveData[] wallsData, RegisterSaveData[] registersData, ProductSaveData[] productsData, ContainerSaveData[] containersData, VehicleSaveData[] vehiclesData)
    {
        this.playerData = playerData;
        this.shopData = shopData;
        this.workersData = workersData;
        this.timeData = timeData;
        this.shelvesData = shelvesData;
        this.wallsData = wallsData;
        this.registersData = registersData;
        this.productsData = productsData;
        this.vehiclesData = vehiclesData;
        this.containersData = containersData;
    }    
}
[System.Serializable]
public class PlayerSaveData
{
    public Vector3 position;
    public float playerMoney;
    public int vehicleIndex;
    public int[] pickedupProducts;
    public int[] pickedupContainers;

    public PlayerSaveData(Vector3 position, float playerMoney, int vehicleIndex, int[] pickedupProducts, int[] pickedupContainers)
    {
        this.position = position;
        this.playerMoney = playerMoney;
        this.vehicleIndex = vehicleIndex;
        this.pickedupProducts = pickedupProducts;
        this.pickedupContainers = pickedupContainers;
    }
}

[System.Serializable]
public class ShopSaveData
{
    public bool[] unlockedCars;
    public bool[] unlockedLand;

    public ShopSaveData(bool[] unlockedCars, bool[] unlockedLand)
    {
        this.unlockedCars = unlockedCars;
        this.unlockedLand = unlockedLand;
    }
}

[System.Serializable]
public class WorkerSaveData
{
    public string name;
    public int task;
    public int wage;
    public WorkerSaveData(string name, int task, int wage)
    {
        this.name = name;
        this.task = task;
        this.wage = wage;
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
public class ShelfSaveData
{
    public Vector3 position;
    public Quaternion rotation;
    public int shelfTypeIndex;

    public ShelfSaveData(Vector3 position, Quaternion rotation, int shelfTypeIndex)
    {
        this.position = position;
        this.rotation = rotation;
        this.shelfTypeIndex = shelfTypeIndex;
    }
}

[System.Serializable]
public class WallSaveData
{
    public Vector3 position;
    public Quaternion rotation;
    public int wallTypeIndex;

    public WallSaveData(Vector3 position, Quaternion rotation, int wallTypeIndex)
    {
        this.position = position;
        this.rotation = rotation;
        this.wallTypeIndex = wallTypeIndex;
    }
}
[System.Serializable]
public class RegisterSaveData
{
    public int registerTypeIndex;
    public Vector3 position;
    public Quaternion rotation;

    public RegisterSaveData(Vector3 position, Quaternion rotation, int registerTypeIndex)
    {
        this.registerTypeIndex = registerTypeIndex;
        this.position = position;
        this.rotation = rotation;
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

    public VehicleSaveData(Vector3 position, Quaternion rotation, int prefabIndex)
    {
        this.prefabIndex = prefabIndex;
        this.position = position;
        this.rotation = rotation;
    }
}

