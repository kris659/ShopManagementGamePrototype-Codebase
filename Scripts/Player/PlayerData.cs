using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    private PlayerInteractions playerInteractions;
    private PlayerPickup playerPickup;
    [SerializeField] private int playerStartingMoney;

    public static PlayerData instance;
    public event Action<float> OnPlayerMoneyChanged;

    public float playerMoney { 
        get { return _playerMoney; } 
        set { _playerMoney = value; OnPlayerMoneyChanged?.Invoke(_playerMoney); }
    }
    private float _playerMoney;

    private void Awake()
    {
        if (instance != null) 
            Destroy(gameObject);
        instance = this;
        playerInteractions = GetComponent<PlayerInteractions>();
        playerPickup = GetComponent<PlayerPickup>();
    }

    private void Start()
    {
        playerMoney = playerStartingMoney;
    }

    public void AddMoney(float moneyAmount)
    {
        playerMoney += moneyAmount;
    }
    public void TakeMoney(float moneyAmount)
    {
        playerMoney -= moneyAmount;
    }
    public bool CanAfford(float moneyAmount)
    {
        return playerMoney >= moneyAmount;
    }

    public PlayerSaveData GetPlayerSaveData()
    {
        int vehicleIndex = playerInteractions.GetVehicleIndex();
        playerPickup.GetPickupSaveData(out int[] pickedupProducts, out int[] pickedupContainers);
        PlayerSaveData playerSaveData = new PlayerSaveData(playerInteractions.GetPlayerPosition(), playerMoney, vehicleIndex, pickedupProducts, pickedupContainers);
        return playerSaveData;
    }

    public void LoadFromSaveData(PlayerSaveData saveData)
    {
        playerMoney = saveData.playerMoney;
        playerInteractions.SetPlayerPosition(saveData.position);
        if (saveData.vehicleIndex != -1)
            playerInteractions.GetInVehicle(VehicleManager.instance.vehiclesSpawned[saveData.vehicleIndex]);
        playerPickup.LoadFromSaveData(saveData.pickedupProducts, saveData.pickedupContainers);
    }

    public void ClearScene()
    {
        playerPickup.ClearPickupList();
    }
}
