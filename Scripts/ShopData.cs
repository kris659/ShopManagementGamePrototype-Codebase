using System;
using System.Collections;
using System.Collections.Generic;
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

    [SerializeField] List<CashRegister> registersList = new List<CashRegister>();
    [SerializeField] List<CashRegister> activeRegistersList = new List<CashRegister>();

    [SerializeField] List<Wall> wallsList = new List<Wall>();

    public Action<bool> onShopOpenStatusChanged;

    public LayerMask shelfTriggerLayer;
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

    public int RemoveShelf(Shelf shelf)
    {
        shelvesList.Remove(shelf);
        return shelvesList.Count - 1;
    }

    public void RemoveRegister(CashRegister register)
    {
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
    }

    public void UpdateRegisterStatus(CashRegister register)
    {
        if (register.isActive && !activeRegistersList.Contains(register))
            activeRegistersList.Add(register);
        if (!register.isActive && activeRegistersList.Contains(register))
            activeRegistersList.Remove(register);
    }
    

    public CashRegister GetActiveRegister()
    {
        if (activeRegistersList.Count == 0)
            return null;
        return activeRegistersList[UnityEngine.Random.Range(0, activeRegistersList.Count)];
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
}
