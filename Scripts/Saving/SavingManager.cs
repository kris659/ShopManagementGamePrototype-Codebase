using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class SavingManager : MonoBehaviour
{
    public static SavingManager instance;
    public string startingSaveName;
    [SerializeField] private int startingMoney;
    [SerializeField] private int startingHour;
    [SerializeField] private List<bool> startingUnlockedCars;
    [SerializeField] private List<bool> startingUnlockedLand;
    [SerializeField] private NavMeshSurface navMeshSurface;

    private string currentGameName;

    private void Awake()
    {
        if(instance != null) {
            Debug.LogError("Multiple saving managers");
            Destroy(this);
            return;
        }
        instance = this;
    }

    public void StartNewGame(string newGameName)
    {
        currentGameName = newGameName;
        Load(startingSaveName, true);
    }

    public void Load(string saveName)
    {
        currentGameName = saveName;
        Load(saveName, false);
    }

    public void Save()
    {        
        SaveFilesManager.Save(currentGameName, CreateSaveData(false));
        PlayerPrefs.SetString("LastSaveName", currentGameName);
        UIManager.textUI.UpdateText("Game saved", 1.5f);
    }

    public void CreateStartingSave()
    {
        SaveFilesManager.CreateStartingSave(startingSaveName, CreateSaveData(true));
    }

    private void Load(string saveName, bool isStartingSave = false)
    {
        ClearScene();
        SaveData saveData;
        if (isStartingSave)
            saveData = SaveFilesManager.LoadStartingSave(saveName);
        else
            saveData = SaveFilesManager.Load(saveName);

        //ShopData.instance.LoadFromSaveData(saveData.shopData);
        try { ShopData.instance.LoadFromSaveData(saveData.shopData); }
        catch (Exception e) { Debug.LogError("Failed loading ShopData " + e.Message); }

        //BuildingManager.instance.LoadFromSaveData(saveData.buildingsData);
        try { BuildingManager.instance.LoadFromSaveData(saveData.buildingsData); }
        catch (Exception e) { Debug.LogError("Failed loading BuildingsData " + e.Message); }

        try { TasksManager.instance.LoadFromSaveData(saveData.tasksSaveData); }
        catch (Exception e) { Debug.LogError("Failed loading tasks data " + e.Message); }

        //ProductsData.instance.LoadFromSaveData(saveData.productsData, saveData.containersData, saveData.furnitureBoxData);
        try { ProductsData.instance.LoadFromSaveData(saveData.productsData, saveData.containersData, saveData.furnitureBoxData); }
        catch (Exception e) { Debug.LogError("Failed loading ProductsData " + e.Message); }

        try { VehicleManager.instance.LoadFromSaveData(saveData.vehiclesData); }
        catch (Exception e) { Debug.LogError("Failed loading VehiclesData " + e.Message); }

        //PlayerData.instance.LoadFromSaveData(saveData.playerData);
        try { PlayerData.instance.LoadFromSaveData(saveData.playerData); }
        catch (Exception e) { Debug.LogError("Failed loading PlayerData " + e.Message); }

        try { WorkersManager.instance.LoadFromSaveData(saveData.workersData); }
        catch (Exception e) { Debug.LogError("Failed loading ShopData " + e.Message); }

        //TimeManager.instance.LoadFromSaveData(saveData.timeData);
        try { TimeManager.instance.LoadFromSaveData(saveData.timeData); }
        catch (Exception e) { Debug.LogError("Failed loading TimeData " + e.Message); }

        try { PriceManager.instance.LoadFromSaveData(saveData.priceSaveData); }
        catch (Exception e) { Debug.LogError("Failed loading priceData " + e.Message); }

        //OnlineOrdersManager.instance.LoadSaveData(saveData.onlineOrdersData);
        try { OnlineOrdersManager.instance.LoadSaveData(saveData.onlineOrdersData); }
        catch (Exception e) { Debug.LogError("Failed loading onlineOrderData " + e.Message); }

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
        BuildingSaveData[] buildingSaveData;
        ProductSaveData[] productSaveData;
        ContainerSaveData[] containerSaveData;
        FurnitureBoxSaveData[] furnitureBoxSaveData;
        VehicleSaveData[] vehiclesSaveData;
        PricesSaveData priceSaveData;
        TasksSaveData tasksSaveData;
        OnlineOrdersManagerSaveData onlineOrdersData;

        try {
            playerSaveData = PlayerData.instance.GetPlayerSaveData();
            if (isStartingSave) {
                playerSaveData.playerMoney = startingMoney;
                playerSaveData.pickablesTypeID = new int[0];
                playerSaveData.pickableID = new int[0];     
            }
        }
        catch (Exception e){
            Debug.LogError("Failed creating PlayerSaveData: " + e.Message + "\nSource: " + e.Source + "\nStack Trace: " + e.StackTrace);
            playerSaveData = new PlayerSaveData(Vector3.zero, 1000, 0, new int[0], new int[0]);
        }
        try {
            shopSaveData = ShopData.instance.GetSaveData();
            if (isStartingSave) {
                shopSaveData.unlockedCars = startingUnlockedCars.ToArray();
                shopSaveData.unlockedLand = startingUnlockedLand.ToArray();
                shopSaveData.unlockedLicenses = new bool[0];
                shopSaveData.isLocationUnlocked = new bool[0];
                shopSaveData.shopPopularityValues = new float[0];
                shopSaveData.shopName = "";
                shopSaveData.isShopOpen = true;
                shopSaveData.billboardIndex = 0;
            }
        }
        catch (Exception e) {
            Debug.LogError("Failed creating ShopSaveData: " + e.Message + "\nSource: " + e.Source + "\nStack Trace: " + e.StackTrace);
            shopSaveData = new ShopSaveData("", true, startingUnlockedCars.ToArray(), startingUnlockedLand.ToArray(), new bool[0], new bool[0], new float[0], 0);
        }

        try {
            workersSaveData = WorkersManager.instance.GetSaveData();
            if (isStartingSave) {
                workersSaveData = new WorkerSaveData[0];
            }
        }
        catch (Exception e) {
            Debug.LogError("Failed creating WorkersSaveData: " + e.Message + "\nSource: " + e.Source + "\nStack Trace: " + e.StackTrace);
            workersSaveData = new WorkerSaveData[0];
        }
        try {
            timeSaveData = TimeManager.instance.GetSaveData();
            if (isStartingSave) {
                timeSaveData.day = 1;
                timeSaveData.hour = startingHour;
                timeSaveData.minute = 0;
            }
        }
        catch (Exception e) {
            Debug.LogError("Failed creating PlayerSaveData: " + e.Message + "\nSource: " + e.Source + "\nStack Trace: " + e.StackTrace);
            timeSaveData = new TimeSaveData(1, startingHour, 0);
        }
        try {
            buildingSaveData = BuildingManager.instance.GetSaveData();
            if (isStartingSave) {
                for(int i = 0; i < buildingSaveData.Length; i++) {
                    buildingSaveData[i].additionalBuildingData = new int[0];
                }
            }
        }
        catch (Exception e) {
            Debug.LogError("Failed creating BuildingsSaveData: " + e.Message + "\nSource: " + e.Source + "\nStack Trace: " + e.StackTrace);
            buildingSaveData = new BuildingSaveData[0];
        }

        try {
            productSaveData = ProductsData.instance.GetProductsSaveData();
        }
        catch (Exception e) {
            Debug.LogError("Failed creating ProductsSaveData: " + e.Message + "\nSource: " + e.Source + "\nStack Trace: " + e.StackTrace);
            productSaveData = new ProductSaveData[0];
        }
        try {
            containerSaveData = ProductsData.instance.GetContainersSaveData();
        }
        catch (Exception e) {
            Debug.LogError("Failed creating ContainersSaveData: " + e.Message + "\nSource: " + e.Source + "\nStack Trace: " + e.StackTrace);
            containerSaveData = new ContainerSaveData[0];
        }
        //FurnitureBoxTest
        try
        {
            furnitureBoxSaveData = ProductsData.instance.GetFurnitureBoxSaveData();
        }
        catch (Exception e)
        {
            Debug.LogError("Failed creating FurnitureBoxSaveData: " + e.Message + "\nSource: " + e.Source + "\nStack Trace: " + e.StackTrace);
            furnitureBoxSaveData = new FurnitureBoxSaveData[0];
        }
        //FurnitureBoxEnd
        try
        {
            vehiclesSaveData = VehicleManager.instance.GetVehiclesSaveData();
            if (isStartingSave) {
                for(int i = 0; i < startingUnlockedCars.Count && i < vehiclesSaveData.Length; i++) {
                    vehiclesSaveData[i].isUnlocked = startingUnlockedCars[i];
                }
            }
        }
        catch (Exception e) {
            Debug.LogError("Failed creating VehiclesSaveData: " + e.Message + "\nSource: " + e.Source + "\nStack Trace: " + e.StackTrace);
            vehiclesSaveData = new VehicleSaveData[0];
        }

        try {
            priceSaveData = PriceManager.instance.GetPricesSaveData();
            if (isStartingSave) {
                priceSaveData = new PricesSaveData(new float[0], new int[0], new int[0], new int[0], new int[0], new int[0], new int[0], new int[0], new int[0], new int[0], new int[0]);
            }
        }
        catch (Exception e) {
            Debug.LogError("Failed creating PricesSaveData: " + e.Message + "\nSource: " + e.Source + "\nStack Trace: " + e.StackTrace);
            priceSaveData = new PricesSaveData(new float[0], new int[0], new int[0], new int[0], new int[0], new int[0], new int[0], new int[0], new int[0], new int[0], new int[0]);
        }

        try {
            tasksSaveData = TasksManager.instance.GetSaveData();
            if (isStartingSave) {
                tasksSaveData = new TasksSaveData(new Vector2Int[0], new TaskSaveData[0]);
            }
        }
        catch (Exception e) {
            Debug.LogError("Failed creating TasksSaveData: " + e.Message + "\nSource: " + e.Source + "\nStack Trace: " + e.StackTrace);
            tasksSaveData = new TasksSaveData(new Vector2Int[0], new TaskSaveData[0]);
        }

        try {
            onlineOrdersData = OnlineOrdersManager.instance.GetSaveData();
            if (isStartingSave) {
                onlineOrdersData = new OnlineOrdersManagerSaveData(2, new OnlineOrderSaveData[0]);
            }
        }
        catch (Exception e) {
            Debug.LogError("Failed creating onlineOrdersData: " + e.Message + "\nSource: " + e.Source + "\nStack Trace: " + e.StackTrace);
            onlineOrdersData = new OnlineOrdersManagerSaveData(2, new OnlineOrderSaveData[0]);
        }

        SaveData saveData = new SaveData(playerSaveData, shopSaveData, workersSaveData, timeSaveData, buildingSaveData, productSaveData, containerSaveData, furnitureBoxSaveData, vehiclesSaveData, priceSaveData, tasksSaveData, onlineOrdersData);
        return saveData;
    }

    public void ClearScene()
    {
        CustomerManager.instance.DestroyAll();
        WorkersManager.instance.DestroyAll();
        ShopData.instance.DestroyAll();
        ProductsData.instance.DestroyAll();        
        VehicleManager.instance.DestroyAll();
        BuildingManager.instance.DestroyAllBuildings();
        PlayerData.instance.ClearScene();
        UIManager.warningsUI.ClearWarnings();
        UIManager.possibleActionsUI.RemoveAllActions();
    }
}
