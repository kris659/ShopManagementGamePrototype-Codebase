using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShopData : MonoBehaviour
{
    public static ShopData instance;
    public bool isShopOpen {
        get { return _isShopOpen; }
        private set { _isShopOpen = value; onShopOpenStatusChanged?.Invoke(value); }
    }
    [SerializeField] private bool _isShopOpen = true;
    public List<Shelf> shelvesList = new List<Shelf>();
    private List<PackingTable> packingTablesList = new List<PackingTable>();
    [SerializeField] List<Shelf> shelvesWithProductsList = new List<Shelf>();

    public List<CashRegister> registersList = new List<CashRegister>();
    [SerializeField] List<CashRegister> activeRegistersList = new List<CashRegister>();
    public List<Dumpster> dumpstersList = new List<Dumpster>();
    public List<WarehouseShelf> warehouseShelvesList = new List<WarehouseShelf>();
    public List<Facility> facilityList = new();

    public bool AreOnlineOrdersEnabled => packingTablesList.Count > 0;

    public Action<bool> onShopOpenStatusChanged;
    public Action onOnlineOrdersDisabled;
    public Action<string> onShopNameChanged;

    public LayerMask shelfTriggerLayer;

    public List<GameObject> floorsToUnlock = new List<GameObject>();

    public string shopName;

    public Billboard billboard;

    [SerializeField] private Transform locationTriggersParent;
    private TriggerHandler[] locationTriggerHandlers;
    [HideInInspector]
    public bool[] isLocationUnlocked;

    private void Awake()
    {
        if (instance != null) {
            Debug.LogError("Multiple ShopData!");
            Destroy(gameObject);
        }
        instance = this;
        StartCoroutine(UpdateShopPopularity());

        locationTriggerHandlers = locationTriggersParent.GetComponentsInChildren<TriggerHandler>();
        for(int i = 0; i < locationTriggerHandlers.Length; i++) {
            int j = i;
            locationTriggerHandlers[i].triggerEnter += (collider) => OnLocationTriggerEnter(j, collider);
        }
        isLocationUnlocked = new bool[locationTriggerHandlers.Length];
    }

    private void OnLocationTriggerEnter(int locationIndex, Collider collider)
    {
        CarPhysx carPhysx = collider.GetComponentInParent<CarPhysx>();
        PlayerInteractions playerInteraction = collider.GetComponentInParent<PlayerInteractions>();

        if ((carPhysx != null && carPhysx.isPlayerControlling) || playerInteraction != null) {
            locationTriggersParent.GetChild(locationIndex).gameObject.SetActive(false);
            isLocationUnlocked[locationIndex] = true;
            UIManager.mapUI.UpdateLocationNames();
        }
        
    }

    private IEnumerator UpdateShopPopularity()
    {
        yield return new WaitForSeconds(10);
        if (!MainMenu.isMainMenuOpen) {
            float rating = 10 * ProductsData.instance.differentProductTypesOnShelvesCount / (float)ProductsData.instance.totalProductsTypeCount;
            ShopPopularityManager.instance.UpdatePopularity(ShopPopularityCategory.ProductsVariety, rating);
        }
        StartCoroutine(UpdateShopPopularity());
    }

    public Shelf GetRandomShelfWithProducts()
    {
        if (shelvesWithProductsList.Count == 0)
            return null;
        return shelvesWithProductsList[UnityEngine.Random.Range(0, shelvesWithProductsList.Count)];
    }

    public int AddShelf(Shelf shelf)
    {
        shelvesList.Add(shelf);
        return shelvesList.Count - 1;
    }

    public void AddRegister(CashRegister register)
    {
        registersList.Add(register);
    }

    public void AddFacility(Facility facility)
    {
        facilityList.Add(facility);
    }

    public void AddDumpster(Dumpster dumpster)
    {
        dumpstersList.Add(dumpster);
    }

    public void AddPackingTable(PackingTable packingTable)
    {
        packingTablesList.Add(packingTable);
    }    

    public void AddWarehouseShelf(WarehouseShelf warehouseShelf)
    {
        warehouseShelvesList.Add(warehouseShelf);
    }

    public void RemoveWarehouseShelf(WarehouseShelf warehouseShelf)
    {
        if (warehouseShelvesList.Contains(warehouseShelf))
            warehouseShelvesList.Remove(warehouseShelf);
    }

    public void RemoveShelf(Shelf shelf)
    {
        if(shelvesWithProductsList.Contains(shelf))
            shelvesWithProductsList.Remove(shelf);
        shelvesList.Remove(shelf);
    }

    public void RemoveRegister(CashRegister register)
    {
        if(activeRegistersList.Contains(register))
            activeRegistersList.Remove(register);
        registersList.Remove(register);
    }
    public void RemoveFacility(Facility facility)
    {
        if (facilityList.Contains(facility))
            facilityList.Remove(facility);
    }

    public void RemoveDumpster(Dumpster dumpster)
    {
        if (dumpstersList.Contains(dumpster))
            dumpstersList.Remove(dumpster);
    }
    public void RemovePackingTable(PackingTable packingTable)
    {
        if (packingTablesList.Contains(packingTable)) {
            packingTablesList.Remove(packingTable);
            if (packingTablesList.Count == 0)
                onOnlineOrdersDisabled.Invoke();
        }        
    }

    public void UpdateShelfStatus(Shelf shelf)
    {
        if (shelf.ProductsOnShelf.Count > 0 && !shelvesWithProductsList.Contains(shelf))
            shelvesWithProductsList.Add(shelf);
        if (shelf.ProductsOnShelf.Count <= 0 && shelvesWithProductsList.Contains(shelf))
            shelvesWithProductsList.Remove(shelf);
    }

    public void UpdateRegisterStatus(CashRegister register)
    {
        if (register.IsOpen && !register.IsFull) {
            if (!activeRegistersList.Contains(register))
                activeRegistersList.Add(register);
        }            
        else {
            if (activeRegistersList.Contains(register))
                activeRegistersList.Remove(register);
        }        
    }
    
    public CashRegister GetBestActiveRegister(Vector3 position)
    {
        if(activeRegistersList.Count == 0)
            return null;

        CashRegister playerCashRegister = null;
        for (int i = 0; i < activeRegistersList.Count; i++) {
            if (activeRegistersList[i].isPlayer && !activeRegistersList[i].isWorker)
                playerCashRegister = activeRegistersList[i];
        }
        int distanceToCustomersInQueueRatio = 5;
        CashRegister bestCashRegister = null;
        float distance = float.MaxValue;
        for (int i = 0; i < activeRegistersList.Count; i++) {
            if (activeRegistersList[i].isWorker) {
                if (bestCashRegister == null) {
                    bestCashRegister = activeRegistersList[i];
                    continue;
                }
                if (Vector3.Distance(activeRegistersList[i].transform.position, position) < distance + (bestCashRegister.CustomerCount - activeRegistersList[i].CustomerCount) * distanceToCustomersInQueueRatio) {
                    bestCashRegister = activeRegistersList[i];
                    continue;
                }
            }
        }
        if(playerCashRegister != null && Vector3.Distance(playerCashRegister.transform.position, position) < distance + 1 * distanceToCustomersInQueueRatio) {
            return playerCashRegister;
        }
        return bestCashRegister;
    }

    public Facility GetBestFreeFacility(Vector3 position)
    {
        if (facilityList.Count == 0)
            return null;

        Facility bestFacility = null;
        float distance = float.MaxValue;
        for (int i = 0; i < facilityList.Count; i++)
        {
            if (facilityList[i] != null && facilityList[i].isFree)
            {
                if (bestFacility == null)
                {
                    facilityList[i].isFree = false;
                    bestFacility = facilityList[i];
                    continue;
                }
                else if (Vector3.Distance(facilityList[i].transform.position, position) < distance)
                {
                    facilityList[i].isFree = false;
                    bestFacility.isFree = true;
                    bestFacility = facilityList[i];
                    continue;
                }
            }
        }
        return bestFacility;
    }

    public PlacingTriggerArea GetEmptyShelfTrigger()
    {
        foreach (Shelf shelf in shelvesList) { 
            foreach (PlacingTriggerArea shelfTrigger in shelf.shelfTriggers) {
                if (shelfTrigger.productsInArea.Count == 0 && shelfTrigger.containersInArea.Count == 0) 
                    return shelfTrigger;
            }
        }
        return null;
    }

    public List<PlacingTriggerArea> GetShelfTriggersList()
    {
        List<PlacingTriggerArea> shelfTriggers = new List<PlacingTriggerArea>();
        foreach (Shelf shelf in shelvesList) {
            foreach (PlacingTriggerArea shelfTrigger in shelf.shelfTriggers) {
                if (shelfTrigger.containersInArea.Count == 0)
                    shelfTriggers.Add(shelfTrigger);
            }
        }
        return shelfTriggers;
    }

    public List<PlacingTriggerArea> GetWarehouseShelfTriggersList()
    {
        List<PlacingTriggerArea> shelfTriggers = new List<PlacingTriggerArea>();
        foreach (WarehouseShelf warehouseShelf in warehouseShelvesList) {
            foreach (PlacingTriggerArea shelfTrigger in warehouseShelf.shelfTriggers) {
                shelfTriggers.Add(shelfTrigger);
            }
        }
        return shelfTriggers;
    }

    public void ChangeShopOpenStatus(bool isOpen)
    {
        isShopOpen = isOpen;
    }

    public void ChangeShopName(string name)
    {
        shopName = name;
        onShopNameChanged?.Invoke(name);
        Billboard.instance.ActivateCurrentUI(Billboard.instance.currentUIIndex);
    }

    public void LoadFromSaveData(ShopSaveData shopData)
    {
        VehicleManager.instance.vehiclesUnlocked = shopData.unlockedCars.ToList();
        isLocationUnlocked = new bool[locationTriggerHandlers.Length];

        for (int i = 0; i < shopData.unlockedLand.Length; i++) {
            floorsToUnlock[i].SetActive(shopData.unlockedLand[i]);
        }

        if (shopData.isLocationUnlocked == null)
            shopData.isLocationUnlocked = new bool[locationTriggerHandlers.Length];

        for (int i = 0; i < shopData.isLocationUnlocked.Length; i++) {
            isLocationUnlocked[i] = shopData.isLocationUnlocked[i];
            locationTriggersParent.GetChild(i).gameObject.SetActive(!isLocationUnlocked[i]);
        }
        shopName = shopData.shopName;
        isShopOpen = shopData.isShopOpen;
        billboard.ActivateCurrentUI(shopData.billboardIndex);
        billboard.currentUIIndex = shopData.billboardIndex;
        ProductsData.instance.LoadSaveUnlockedLicenses(shopData.unlockedLicenses);

        ShopPopularityManager.instance.LoadPopularitySaveData(shopData);
        ShopPopularityManager.instance.UpdatePopularity(ShopPopularityCategory.ShopSize, GetUnlockedLandsCount() + 1);

        UIManager.mapUI.UpdateLocationNames();
    }

    public void DestroyAll()
    {
        shelvesList.Clear();
        shelvesWithProductsList.Clear();
        registersList.Clear();
        activeRegistersList.Clear();
        packingTablesList.Clear();
        dumpstersList.Clear();
    }

    public int GetUnlockedLandsCount()
    {
        int count = 0;
        for (int i = 0; i < floorsToUnlock.Count; i++) {
            if (floorsToUnlock[i].activeSelf) {
                count++;
            }
        }
        return count;
    }

    public ShopSaveData GetSaveData()
    {
        bool[] unlockedLand = new bool[floorsToUnlock.Count];
        for(int i = 0; i < unlockedLand.Length; i++) {
            unlockedLand[i] = floorsToUnlock[i].activeSelf;
        }
        return new ShopSaveData(shopName, isShopOpen, VehicleManager.instance.vehiclesUnlocked.ToArray(), unlockedLand, ProductsData.instance.unlockedProductsLicenses.ToArray(), isLocationUnlocked, ShopPopularityManager.instance.GetPopularitySaveData(), billboard.currentUIIndex);
    }
}
