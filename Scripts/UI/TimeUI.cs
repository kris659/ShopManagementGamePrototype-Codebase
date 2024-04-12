using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimeUI : MonoBehaviour
{
    [SerializeField] private TMP_Text timeText;

    private void Start()
    {
        TimeManager.instance.OnMinuteChanged += UpdateTimeText;
    }


    private void UpdateTimeText()
    {
        timeText.text = TimeManager.instance.Hour.ToString("00") + ":" + TimeManager.instance.Minute.ToString("00");
    }
}
