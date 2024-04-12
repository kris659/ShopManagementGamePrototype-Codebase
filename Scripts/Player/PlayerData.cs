using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    [SerializeField] private PlayerInteractions playerInteractions;

    public static PlayerData instance;
    public event Action<int> OnPlayerMoneyChanged;

    public int playerMoney { 
        get { return _playerMoney; } 
        set { _playerMoney = value; OnPlayerMoneyChanged?.Invoke(_playerMoney); }
    }
    private int _playerMoney;

    private void Awake()
    {
        if (instance != null) 
            Destroy(gameObject);
        instance = this;
    }

    private void Start()
    {
        playerMoney = 1000;
    }

    public void AddMoney(int moneyAmount)
    {
        playerMoney += moneyAmount;
    }
    public void TakeMoney(int moneyAmount)
    {
        playerMoney -= moneyAmount;
    }
    public bool CanAfford(int moneyAmount)
    {
        return playerMoney >= moneyAmount;
    }

    public PlayerSaveData GetPlayerSaveData()
    {
        int vehicleIndex = playerInteractions.GetVehicleIndex();
        PlayerSaveData playerSaveData = new PlayerSaveData(playerInteractions.GetPlayerPosition(), playerMoney, vehicleIndex);
        return playerSaveData;
    }

    public void LoadFromSaveData(PlayerSaveData saveData)
    {
        playerMoney = saveData.playerMoney;
        playerInteractions.SetPlayerPosition(saveData.position);
        if (saveData.vehicleIndex != -1)
            playerInteractions.GetInVehicle(VehicleManager.instance.vehiclesSpawned[saveData.vehicleIndex]);
    }
}
