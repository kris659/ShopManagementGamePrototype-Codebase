using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : WindowUI
{
    [SerializeField] private Slider mouseSensitivitySlider;
    [SerializeField] private Button closeButton;

    public static Action<float> OnMouseSensitivityChanged;

    internal override void Awake()
    {
        base.Awake();
        //mouseSensitivitySlider.onValueChanged.AddListener(OnSliderValueChanged);
        closeButton.onClick.AddListener(CloseUI);
        LoadSettings();
    }

    public override void CloseUI()
    {
        base.CloseUI();
        SaveSettings();

        OnMouseSensitivityChanged?.Invoke(mouseSensitivitySlider.value);
        Time.timeScale = 1.0f;
    }

    private void LoadSettings()
    {
        mouseSensitivitySlider.value = PlayerPrefs.GetFloat("MouseSensitivity", 0.5f);
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetFloat("MouseSensitivity", mouseSensitivitySlider.value);
    }
}
