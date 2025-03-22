using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightsController : MonoBehaviour
{
    [SerializeField] private List<Material> materialsToEnableEmmision;

    [SerializeField] private int dayStartHour;
    [SerializeField] private int dayEndHour;

    public static bool dayNightStatus = true; // true -- day
    public static Action<bool> onDayNightStatusChanged; 

    private void Start()
    {
        TimeManager.instance.OnHourChanged += OnHourChanged;
        onDayNightStatusChanged += UpdateMaterials;
        SceneLoader.OnWorldSceneLoaded += () => UpdateMaterials(dayNightStatus);
        UpdateMaterials(dayNightStatus);
    }

    private void OnHourChanged()
    {
        //Debug.Log("Hour: " + TimeManager.instance.Hour);
        if((TimeManager.instance.Hour >= dayEndHour || TimeManager.instance.Hour < dayStartHour)) {
            dayNightStatus = false;
            onDayNightStatusChanged.Invoke(dayNightStatus);
        }
        if(TimeManager.instance.Hour < dayEndHour && TimeManager.instance.Hour >= dayStartHour) {
            dayNightStatus = true;
            onDayNightStatusChanged.Invoke(dayNightStatus);
        }
    }

    private void UpdateMaterials(bool dayNightStatus)
    {
        Debug.Log("Update Materials: " + dayNightStatus);
        if (dayNightStatus) {
            foreach (Material material in materialsToEnableEmmision) {
                material.DisableKeyword("_EMISSION");
            }
        }
        else{
            foreach (Material material in materialsToEnableEmmision) {
                material.EnableKeyword("_EMISSION");
            }
        }

    }
}
