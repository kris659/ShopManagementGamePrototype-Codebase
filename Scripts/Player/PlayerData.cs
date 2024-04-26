using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    [SerializeField] private PlayerInteractions playerInteractions;
    [SerializeField] private int playerStartingMoney;

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
        playerMoney = playerStartingMoney;
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
        PlayerSaveData playerSaveData = new PlayerSaveData(playerInteractions.GetPlayerPosition(), playerMoney, vehicleIndex,
            TimeManager.instance.Hour, TimeManager.instance.Minute);
        return playerSaveData;
    }

    public void LoadFromSaveData(PlayerSaveData saveData)
    {
        playerMoney = saveData.playerMoney;
        playerInteractions.SetPlayerPosition(saveData.position);
        if (saveData.vehicleIndex != -1)
            playerInteractions.GetInVehicle(VehicleManager.instance.vehiclesSpawned[saveData.vehicleIndex]);

        TimeManager.instance.SetTime(saveData.hour, saveData.minute);
    }
}
