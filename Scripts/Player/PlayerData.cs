using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData instance;


    private PlayerInteractions playerInteractions;
    [HideInInspector]
    public PlayerPickup playerPickup;
    [SerializeField] private int playerStartingMoney;
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

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftBracket) && Input.GetKey(KeyCode.RightBracket)) {
            AddMoney(1000, false);
        }
    }

    public void AddMoney(float moneyAmount, bool shoudProgressTaskEarnMoney)
    {
        if (moneyAmount < 0)
            Debug.LogError("Use TakeMoney for negative $");
        playerMoney += moneyAmount;
        if(shoudProgressTaskEarnMoney)
            TasksManager.instance.ProgressTasks(TaskType.EarnMoney, Mathf.RoundToInt(moneyAmount));
    }
    public void TakeMoney(float moneyAmount)
    {
        playerMoney -= moneyAmount;
        TasksManager.instance.ProgressTasks(TaskType.SpendMoney, Mathf.RoundToInt(moneyAmount));
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
        playerInteractions.LoadFromSaveData(saveData.vehicleIndex);        
        playerPickup.LoadFromSaveData(saveData.pickablesTypeID, saveData.pickableID);
    }

    public void ClearScene()
    {
        playerPickup.ClearPickupList();
    }

    public void OnDumpsterUsed()
    {
        playerPickup.OnDumpsterUsed();
    }
}
