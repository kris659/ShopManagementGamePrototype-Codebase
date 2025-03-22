using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lamp : MonoBehaviour
{
    private Light[] lightComponents;
    private void Awake()
    {
        LightsController.onDayNightStatusChanged += UpdateStatus;
        lightComponents = GetComponentsInChildren<Light>();
        UpdateStatus(LightsController.dayNightStatus);
    }

    private void UpdateStatus(bool dayStatus)
    {
        foreach (Light light in lightComponents) {
            light.enabled = !dayStatus;
        }
    }

    private void OnDestroy()
    {
        LightsController.onDayNightStatusChanged -= UpdateStatus;
    }
}
