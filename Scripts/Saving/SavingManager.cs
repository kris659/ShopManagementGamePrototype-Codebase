using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class SavingManager : MonoBehaviour
{
    [SerializeField] private bool shouldLoadOnStart = false;
    public string startingSaveName;
    [SerializeField] private int startingMoney;
    [SerializeField] private List<bool> startingUnlockedCars;
    [SerializeField] private List<bool> startingUnlockedLand;
    [SerializeField] private NavMeshSurface navMeshSurface;
    SavingUI savingUI;

    private void Awake()
    {
        SceneLoader.OnUISceneLoaded += () => {
            savingUI = UIManager.savingUI;
            savingUI.Init(this);
        };
        SceneLoader.OnWorldSceneLoaded += () => {
            StartCoroutine(OnWorldSceneLoaded());
        };
    }

    IEnumerator OnWorldSceneLoaded()
    {
        yield return new WaitForEndOfFrame();
        if (shouldLoadOnStart)
            Load(startingSaveName, true);
    }

    public void Save(string saveName, bool isStartingSave = false)
    {
        if (isStartingSave)
            SaveFilesManager.CreateStartingSave(saveName, CreateSaveData(isStartingSave));        
        else
            SaveFilesManager.Save(saveName, CreateSaveData(isStartingSave));
    }
    public void Load(string saveName, bool isStartingSave = false)
    {
        ClearScene();
        SaveData saveData;
        if (isStartingSave)
            saveData = SaveFilesManager.LoadStartingSave(saveName);
        else
            saveData = SaveFilesManager.Load(saveName);

        try { ShopData.instance.LoadFromSaveData(saveData); }
        catch (Exception e) { Debug.LogError("Failed loading ShopData " + e.Message); }
        try { WorkersManager.instance.LoadFromSaveData(saveData.workersData); }
        catch (Exception e) { Debug.LogError("Failed loading ShopData " + e.Message); }
        //ProductsData.instance.LoadFromSaveData(saveData.productsData, saveData.containersData);
        try { ProductsData.instance.LoadFromSaveData(saveData.productsData, saveData.containersData); }
        catch (Exception e) { Debug.LogError("Failed loading ProductsData " + e.Message); }

        try { VehicleManager.instance.LoadFromSaveData(saveData.vehiclesData);}
        catch (Exception e) { Debug.LogError("Failed loading VehiclesData " + e.Message); }
        PlayerData.instance.LoadFromSaveData(saveData.playerData);
        //try { PlayerData.instance.LoadFromSaveData(saveData.playerData); }
        //catch (Exception e) { Debug.LogError("Failed loading PlayerData " + e.Message); }

        try { TimeManager.instance.LoadFromSaveData(saveData.timeData); }
        catch (Exception e) { Debug.LogError("Failed loading TimeData " + e.Message); }

        VehicleManager.instance.vehiclesUnlocked = startingUnlockedCars;
        navMeshSurface.BuildNavMesh();
        StartCoroutine(BuildNavMesh());
    }

    IEnumerator BuildNavMesh()
    {
        yield return new WaitForSeconds(0.2f);
        navMeshSurface.BuildNavMesh();
    }


    private SaveData CreateSaveData(bool isStartingSave)
    {
        PlayerSaveData playerSaveData;
        ShopSaveData shopSaveData;
        WorkerSaveData[] workersSaveData;
        TimeSaveData timeSaveData;
        ShelfSaveData[] shelfSaveData;
        RegisterSaveData[] registerSaveData;
        WallSaveData[] wallSaveData;
        ProductSaveData[] productSaveData;
        ContainerSaveData[] containerSaveData;
        VehicleSaveData[] vehiclesSaveData;
        
        try {
            playerSaveData = PlayerData.instance.GetPlayerSaveData();
            if (isStartingSave) {
                playerSaveData.playerMoney = startingMoney;
                playerSaveData.pickedupProducts = new int[0];
                playerSaveData.pickedupContainers = new int[0];
            }
        }
        catch (Exception e){
            Debug.LogError("Failed creating PlayerSaveData: " + e.Message);
            playerSaveData = new PlayerSaveData(Vector3.zero, 1000, 0, new int[0], new int[0]);
        }
        try {
            shopSaveData = ShopData.instance.GetSaveData();
            if (isStartingSave) {
                shopSaveData.unlockedCars = startingUnlockedCars.ToArray();
                shopSaveData.unlockedLand = startingUnlockedLand.ToArray();
            }
        }
        catch (Exception e) {
            Debug.LogError("Failed creating ShopSaveData: " + e.Message);
            shopSaveData = new ShopSaveData(startingUnlockedCars.ToArray(), startingUnlockedLand.ToArray());
        }
        try {
            workersSaveData = WorkersManager.instance.GetSaveData();
            if (isStartingSave) {
                workersSaveData = new WorkerSaveData[0];
            }
        }
        catch (Exception e) {
            Debug.LogError("Failed creating WorkersSaveData: " + e.Message);
            workersSaveData = new WorkerSaveData[0];
        }
        try {
            timeSaveData = TimeManager.instance.GetSaveData();
            if (isStartingSave) {
                timeSaveData.day = 1;
                timeSaveData.hour = 12;
                timeSaveData.minute = 0;
            }
        }
        catch (Exception e) {
            Debug.LogError("Failed creating PlayerSaveData: " + e.Message);
            timeSaveData = new TimeSaveData(1, 12, 0);
        }
        try {
            shelfSaveData = ShopData.instance.GetShelfSaveData();
        }
        catch (Exception e) {
            Debug.LogError("Failed creating ShelvesSaveData: " + e.Message);
            shelfSaveData = new ShelfSaveData[0];
        }
        try {
            registerSaveData = ShopData.instance.GetRegisterSaveData();
        }
        catch (Exception e) {
            Debug.LogError("Failed creating RegistersSaveData: " + e.Message);
            registerSaveData = new RegisterSaveData[0];
        }
        try {
            wallSaveData = ShopData.instance.GetWallSaveData();
        }
        catch (Exception e) {
            Debug.LogError("Failed creating WallsSaveData: " + e.Message);
            wallSaveData = new WallSaveData[0];
        }
        try {
            productSaveData = ProductsData.instance.GetProductsSaveData();
        }
        catch (Exception e) {
            Debug.LogError("Failed creating ProductsSaveData: " + e.Message);
            productSaveData = new ProductSaveData[0];
        }
        try {
            containerSaveData = ProductsData.instance.GetContainersSaveData();
        }
        catch (Exception e) {
            Debug.LogError("Failed creating ContainersSaveData: " + e.Message);
            containerSaveData = new ContainerSaveData[0];
        }
        try {
            vehiclesSaveData = VehicleManager.instance.GetVehiclesSaveData();
        }
        catch (Exception e) {
            Debug.LogError("Failed creating VehiclesSaveData: " + e.Message);
            vehiclesSaveData = new VehicleSaveData[0];
        }

        SaveData saveData = new SaveData(playerSaveData, shopSaveData, workersSaveData, timeSaveData, shelfSaveData, wallSaveData, registerSaveData, productSaveData, containerSaveData, vehiclesSaveData);
        return saveData;
    }

    private void ClearScene()
    {
        CustomerManager.instance.DestroyAll();
        ShopData.instance.DestroyAll();
        ProductsData.instance.DestroyAll();        
        VehicleManager.instance.DestroyAll();
        WorkersManager.instance.DestroyAll();
        PlayerData.instance.ClearScene();
    }
}
