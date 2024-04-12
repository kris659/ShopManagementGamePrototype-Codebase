using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TimeManager : MonoBehaviour
{
    public static TimeManager instance;
    public Action OnMinuteChanged;
    public Action OnHourChanged;
    public int Minute { get; private set; }
    public int Hour { get; private set; }    

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
            OnMinuteChanged?.Invoke();
            if(Minute >= 60)
            {
                Hour++;
                Minute = 0;
                OnHourChanged?.Invoke();
                if(Hour >= 24)
                {
                    Hour = 0;
                }
            }
            timer = minuteInRealTime;
        }
    }
    public void SetTime(int hour, int minute)
    {
        Minute = minute;
        Hour = hour;
    }
}
