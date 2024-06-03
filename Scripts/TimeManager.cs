using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TimeManager : MonoBehaviour
{
    public static TimeManager instance;
    public Action OnMinuteChanged;
    public Action OnHourChanged;
    public Action OnDayChanged;
    public int Minute { get; private set; }
    public int Hour { get; private set; }    
    public int Day { get; private set; }

    private float minuteInRealTime = 1f;
    private float timer;

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
        if(timer <=0)
        {
            Minute++;            
            if(Minute >= 60){
                Hour++;
                Minute = 0;                
                if(Hour >= 24){
                    Hour = 0;
                    Day++;
                    OnDayChanged?.Invoke();                    
                }
                OnHourChanged?.Invoke();
            }
            OnMinuteChanged?.Invoke();
            timer = minuteInRealTime;
        }
    }

    public TimeSaveData GetSaveData()
    {
        return new TimeSaveData(Day, Hour, Minute);
    }

    public void LoadFromSaveData(TimeSaveData saveData)
    {
        Minute = saveData.minute;
        Hour = saveData.hour;
        Day = saveData.day;
        OnMinuteChanged?.Invoke();
        //OnHourChanged?.Invoke();
        //OnDayChanged?.Invoke();
    }

}
