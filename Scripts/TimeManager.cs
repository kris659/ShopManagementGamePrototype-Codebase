using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TimeManager : MonoBehaviour
{
    public static TimeManager instance;
    public Action OnMinuteChanged;
    public Action OnHourChanged;
    //public Action OnDayChanged;
    public int Minute { get; private set; }
    public float MinuteFloat { get; private set; }
    public int Hour { get; private set; }    
    public int Day { get; private set; }

    [SerializeField] private float minuteInRealTime = 1f;
    private float timer;

    [SerializeField] private int dayStartHour;
    [SerializeField] private int dayEndHour;

    [SerializeField] private KeyCode nextDayKeyCode;
    [SerializeField] private bool isWaitingForNextDayInput;
    private string nextDayActionText = "X - start next day";

    private void Awake()
    {
        if(instance != null) {
            Debug.LogError("Multiple time managers");
            Destroy(gameObject);
        }
        instance = this;
    }

    void Start()
    {
        Minute = 0;
        Hour = 12;
        timer = minuteInRealTime;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        
        if (timer <=0){
            Minute++;            
            if(Minute >= 60){
                Minute = 0;
                MinuteFloat = Minute;
                Hour++;

                if(Hour == dayStartHour) {
                    HandleNextDay();
                }
                if (Hour == dayEndHour) {
                    isWaitingForNextDayInput = true;
                    UIManager.possibleActionsUI.AddAction(nextDayActionText);
                }
                if(Hour >= 24){
                    Hour = 0;
                }
                OnHourChanged?.Invoke();
            }
            MinuteFloat = Minute;
            OnMinuteChanged?.Invoke();
            timer = minuteInRealTime;
        }
        MinuteFloat = Minute + (minuteInRealTime - timer) / minuteInRealTime;

        if (isWaitingForNextDayInput && Input.GetKeyDown(nextDayKeyCode) && UIManager.leftPanelUI.currentlyOpenWindow == null) {
            HandleNextDay();
        }
    }

    public TimeSaveData GetSaveData()
    {
        return new TimeSaveData(Day, Hour, Minute);
    }

    public void LoadFromSaveData(TimeSaveData saveData)
    {
        Minute = saveData.minute;
        MinuteFloat = Minute;
        Hour = saveData.hour;
        Day = saveData.day;
        OnMinuteChanged?.Invoke();
        OnHourChanged?.Invoke();

        if (Hour >= dayEndHour || Hour < dayStartHour) {
            isWaitingForNextDayInput = true;
            UIManager.possibleActionsUI.AddAction(nextDayActionText);
        }
        //OnDayChanged?.Invoke();
    }

    private void HandleNextDay()
    {
        UIManager.possibleActionsUI.RemoveAction(nextDayActionText);
        isWaitingForNextDayInput = false;
        Minute = 0;
        Hour = dayStartHour;
        Day += 1;
        if (!MainMenu.isMainMenuOpen) {
            OnMinuteChanged?.Invoke();
            OnHourChanged?.Invoke();
            UIManager.nextDayUI.OpenUI();
            SavingManager.instance.Save();
        }        
    }
}
