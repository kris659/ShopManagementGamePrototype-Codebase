using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavingManager : MonoBehaviour
{
    [SerializeField] private bool shouldLoadOnStart = false;
    [SerializeField] private string saveNameLoadOnStart;
    SavingUI savingUI;
    
    private void Start()
    {
        savingUI = UIManager.savingUI;
        savingUI.Init(this);

        if (shouldLoadOnStart)
            Load(saveNameLoadOnStart);
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
            if(savingUI.isOpen)
                savingUI.CloseUI();
            else
                savingUI.OpenUI();
    }

    public void Save(string saveName)
    {
        SaveFilesManager.Save(saveName, CreateSaveData());
    }
    public void Load(string saveName)
    {
        ClearScene();
        SaveData saveData = SaveFilesManager.Load(saveName);
        ShopData.instance.LoadFromSaveData(saveData);
        ProductsData.instance.LoadFromSaveData(saveData.productsData);
        VehicleManager.instance.LoadFromSaveData(saveData.vehiclesData);
        PlayerData.instance.LoadFromSaveData(saveData.playerData);
        TimeManager.instance.SetTime(saveData.hour, saveData.minute);
    }


    private SaveData CreateSaveData()
    {
        PlayerSaveData playerSaveData = PlayerData.instance.GetPlayerSaveData();
        ShelfSaveData[] shelfSaveData = ShopData.instance.GetShelfSaveData();
        RegisterSaveData[] registerSaveData = ShopData.instance.GetRegisterSaveData();
        WallSaveData[] wallSaveData = ShopData.instance.GetWallSaveData();
        ProductsSaveData[] productSaveData = ProductsData.instance.GetProductsSaveData();
        VehiclesSaveData[] vehiclesSaveData = VehicleManager.instance.GetVehiclesSaveData();

        SaveData saveData = new SaveData(playerSaveData, shelfSaveData, wallSaveData, registerSaveData, productSaveData, vehiclesSaveData);
        return saveData;
    }

    private void ClearScene()
    {
        CustomerManager.instance.DestroyAll();
        ShopData.instance.DestroyAll();
        ProductsData.instance.DestroyAll();        
        VehicleManager.instance.DestroyAll();
    }
}
