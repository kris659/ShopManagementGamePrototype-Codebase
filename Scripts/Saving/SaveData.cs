using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public PlayerSaveData playerData;
    public ShelfSaveData[] shelvesData;
    public WallSaveData[] wallsData;
    public RegisterSaveData[] registersData;
    public ProductSaveData[] productsData;
    public ContainerSaveData[] containersData;
    public VehicleSaveData[] vehiclesData;
    

    public int[] storageManagerProducts;

    public SaveData(PlayerSaveData playerData,  ShelfSaveData[] shelvesData, WallSaveData[] wallsData, RegisterSaveData[] registersData, ProductSaveData[] productsData, ContainerSaveData[] containersData, VehicleSaveData[] vehiclesData)
    {
        this.playerData = playerData;
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
    public int playerMoney;
    public int vehicleIndex;
    public int hour;
    public int minute;

    public PlayerSaveData(Vector3 position, int playerMoney, int vehicleIndex, int hour, int minute)
    {
        this.position = position;
        this.playerMoney = playerMoney;
        this.vehicleIndex = vehicleIndex;
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

