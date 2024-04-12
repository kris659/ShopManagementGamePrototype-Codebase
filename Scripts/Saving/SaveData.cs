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
    public ProductsSaveData[] productsData;
    public VehiclesSaveData[] vehiclesData;
    public int hour;
    public int minute;

    public int[] storageManagerProducts;

    public SaveData(PlayerSaveData playerData,  ShelfSaveData[] shelvesData, WallSaveData[] wallsData, RegisterSaveData[] registersData, ProductsSaveData[] productsData, VehiclesSaveData[] vehiclesData)
    {
        this.playerData = playerData;
        this.shelvesData = shelvesData;
        this.wallsData = wallsData;
        this.registersData = registersData;
        this.productsData = productsData;
        this.vehiclesData = vehiclesData;
        hour = TimeManager.instance.Hour;
        minute = TimeManager.instance.Minute;
    }
    
}

[System.Serializable]
public class PlayerSaveData
{
    public Vector3 position;
    public int playerMoney;
    public int vehicleIndex;

    public PlayerSaveData(Vector3 position, int playerMoney, int vehicleIndex)
    {
        this.position = position;
        this.playerMoney = playerMoney;
        this.vehicleIndex = vehicleIndex;
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
public class ProductsSaveData
{
    public int productTypeIndex;
    public Vector3 position;
    public Quaternion rotation;

    public bool isTakenByCustomer;
    public int shelfIndex;

    public ProductsSaveData[] productsInContainer;
    public Vector3[] productsInContainerPositions;
    public Vector3[] productsInContainerRotations;

    public ProductsSaveData(Vector3 position, Quaternion rotation, int productTypeIndex, bool isTakenByCustomer, int shelfIndex,
        ProductsSaveData[] productsInContainer, Vector3[] productsInContainerPositions, Vector3[] productsInContainerRotations)
    {
        this.productTypeIndex = productTypeIndex;
        this.position = position;
        this.rotation = rotation;
        this.isTakenByCustomer = isTakenByCustomer;
        this.shelfIndex = shelfIndex;
        this.productsInContainer = productsInContainer;
        this.productsInContainerPositions = productsInContainerPositions;
        this.productsInContainerRotations = productsInContainerRotations;
    }
}

[System.Serializable]
public class VehiclesSaveData
{
    public int prefabIndex;
    public Vector3 position;
    public Quaternion rotation;

    public VehiclesSaveData(Vector3 position, Quaternion rotation, int prefabIndex)
    {
        this.prefabIndex = prefabIndex;
        this.position = position;
        this.rotation = rotation;
    }
}

