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
    [SerializeField] List<Shelf> shelvesWithProductsList = new List<Shelf>();

    public List<CashRegister> registersList = new List<CashRegister>();
    [SerializeField] List<CashRegister> activeRegistersList = new List<CashRegister>();

    [SerializeField] List<Wall> wallsList = new List<Wall>();

    public Action<bool> onShopOpenStatusChanged;

    public LayerMask shelfTriggerLayer;

    public List<GameObject> floorsToUnlock = new List<GameObject>();

    public int MaxCustomers { get; private set; }

    private void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        instance = this;
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
        UpdateMaxCustomers();
        return shelvesList.Count - 1;
    }

    public void AddRegister(CashRegister register)
    {
        registersList.Add(register);
    }

    public void AddWall(Wall wall)
    {
        wallsList.Add(wall);
    }

    public void RemoveShelf(Shelf shelf)
    {
        if(shelvesWithProductsList.Contains(shelf))
            shelvesWithProductsList.Remove(shelf);
        shelvesList.Remove(shelf);
        UpdateMaxCustomers();
    }

    public void RemoveRegister(CashRegister register)
    {
        if(activeRegistersList.Contains(register))
            activeRegistersList.Remove(register);
        registersList.Remove(register);
    }

    public void RemoveWall(Wall wall)
    {
        wallsList.Remove(wall);
    }

    public void UpdateShelfStatus(Shelf shelf)
    {
        if (shelf.products.Count > 0 && !shelvesWithProductsList.Contains(shelf))
            shelvesWithProductsList.Add(shelf);
        if (shelf.products.Count <= 0 && shelvesWithProductsList.Contains(shelf))
            shelvesWithProductsList.Remove(shelf);
        UpdateMaxCustomers();
    }

    private void UpdateMaxCustomers()
    {
        int maxCustomers = shelvesList.Count + shelvesWithProductsList.Count;
        maxCustomers = Math.Max(5, maxCustomers);
        maxCustomers = Math.Min(50, maxCustomers);
        MaxCustomers = maxCustomers;
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
    
    public CashRegister GetActiveRegister()
    {
        if (activeRegistersList.Count == 0)
            return null;
        return activeRegistersList[UnityEngine.Random.Range(0, activeRegistersList.Count)];
    }

    public ShelfTrigger GetEmptyShelfTrigger()
    {
        foreach (Shelf shelf in shelvesList) { 
            foreach (ShelfTrigger shelfTrigger in shelf.shelfTriggers) {
                if (shelfTrigger.productsInArea.Count == 0) 
                    return shelfTrigger;
            }
        }
        return null;
    }

    public void ChangeShopOpenStatus(bool isOpen)
    {
        isShopOpen = isOpen;
    }


    public void LoadFromSaveData(SaveData saveData)
    {
        for(int i = 0; i < saveData.shelvesData.Length; i++) {
            ShelfSaveData shelfSaveData = saveData.shelvesData[i];
            Shelf.Spawn(shelfSaveData.shelfTypeIndex, shelfSaveData.position, shelfSaveData.rotation);
        }
        for (int i = 0; i < saveData.registersData.Length; i++) {
            RegisterSaveData registerSaveData = saveData.registersData[i];
            CashRegister.Spawn(registerSaveData.registerTypeIndex, registerSaveData.position, registerSaveData.rotation);
        }
        for (int i = 0; i < saveData.wallsData.Length; i++) {
            WallSaveData wallSaveData = saveData.wallsData[i];
            Wall.Spawn(wallSaveData.wallTypeIndex, wallSaveData.position, wallSaveData.rotation);
        }
        VehicleManager.instance.vehiclesUnlocked = saveData.shopData.unlockedCars.ToList();
        for(int i = 0; i < saveData.shopData.unlockedLand.Length; i++) {
            floorsToUnlock[i].SetActive(saveData.shopData.unlockedLand[i]);
        }
    }

    public ShelfSaveData[] GetShelfSaveData()
    {
        ShelfSaveData[] shelfSaveData = new ShelfSaveData[shelvesList.Count];

        for(int i = 0; i < shelvesList.Count; i++) {
            Shelf shelf = shelvesList[i];
            shelfSaveData[i] = new ShelfSaveData(shelf.transform.position, shelf.transform.rotation, shelf.shelfTypeIndex);
        }
        return shelfSaveData;
    }

    public RegisterSaveData[] GetRegisterSaveData()
    {
        RegisterSaveData[] registerSaveData = new RegisterSaveData[registersList.Count];

        for (int i = 0; i < registersList.Count; i++) {
            CashRegister register = registersList[i];
            registerSaveData[i] = new RegisterSaveData(register.transform.position, register.transform.rotation, 0);
        }
        return registerSaveData;
    }

    public WallSaveData[] GetWallSaveData()
    {
        WallSaveData[] wallSaveData = new WallSaveData[wallsList.Count];

        for (int i = 0; i < wallsList.Count; i++) {
            Wall wall = wallsList[i];
            wallSaveData[i] = new WallSaveData(wall.transform.position, wall.transform.rotation, wall.typeIndex);
        }
        return wallSaveData;
    }

    public void DestroyAll()
    {
        for (int i = 0; i < shelvesList.Count; i++)
            Destroy(shelvesList[i].gameObject);
        for (int i = 0; i < wallsList.Count; i++)
            Destroy(wallsList[i].gameObject);
        for (int i = 0;i < registersList.Count;i++)
            Destroy(registersList[i].gameObject);
        shelvesList.Clear();
        shelvesWithProductsList.Clear();
        registersList.Clear();
        activeRegistersList.Clear();
        wallsList.Clear();
    }

    public ShopSaveData GetSaveData()
    {
        bool[] unlockedLand = new bool[floorsToUnlock.Count];
        for(int i = 0; i < unlockedLand.Length; i++) {
            unlockedLand[i] = floorsToUnlock[i].activeSelf;
        }
        return new ShopSaveData(VehicleManager.instance.vehiclesUnlocked.ToArray(), unlockedLand);
    }
}
