using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : WindowUI
{
    [SerializeField] private Slider mouseSensitivitySlider;
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_Dropdown maxFpsDropdown;
    [SerializeField] private TMP_Dropdown qualitySettingsDropdown;

    [SerializeField] private Slider masterVolume;
    [SerializeField] private Slider musicVolume;
    [SerializeField] private Slider sfxVolume;

    public static Action<float> OnMouseSensitivityChanged;

    List<string> options;
    internal override void Awake()
    {
        base.Awake();
        closeButton.onClick.AddListener(CloseUI);
        masterVolume.onValueChanged.AddListener(OnMasterValueChanged);
        musicVolume.onValueChanged.AddListener(OnMusicValueChanged);
        sfxVolume.onValueChanged.AddListener(OnSfxValueChanged);

        maxFpsDropdown.ClearOptions();
        options = new List<string> { "Inf", "120", "90", "60", "50", "40", "30"};
        maxFpsDropdown.AddOptions(options);

        maxFpsDropdown.onValueChanged.AddListener(OnMaxFPSValueChanged);
        qualitySettingsDropdown.onValueChanged.AddListener(OnQualitySettingsChanged);

        QualitySettings.vSyncCount = 0;
        LoadSettings();

    }

    public override void OpenUI()
    {
        base.OpenUI();
        masterVolume.value = AudioManager.masterVolume;
        musicVolume.value = AudioManager.musicVolume;
        sfxVolume.value = AudioManager.sfxVolume;
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
        masterVolume.value = PlayerPrefs.GetFloat("MasterVolume", 0.8f);
        musicVolume.value = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
        sfxVolume.value = PlayerPrefs.GetFloat("sfxVolume", 0.8f);
        maxFpsDropdown.value = PlayerPrefs.GetInt("MaxFPSOption", 0);
        qualitySettingsDropdown.value = PlayerPrefs.GetInt("GraphicsQuality", 2);
        OnQualitySettingsChanged(qualitySettingsDropdown.value);
    }

    private void OnMaxFPSValueChanged(int value)
    {
        if(value == 0) {
            Application.targetFrameRate = -1;
        }
        else {
            //Debug.Log("Set FSP to " + int.Parse(options[value]));
            Application.targetFrameRate = int.Parse(options[value]);
        }
    }

    private void OnQualitySettingsChanged(int value)
    {
        QualitySettings.SetQualityLevel(value, false);
    }

    private void OnMasterValueChanged(float value)
    {
        AudioManager.masterVolume = value;
    }
    private void OnMusicValueChanged(float value)
    {
        AudioManager.musicVolume = value;
    }
    private void OnSfxValueChanged(float value)
    {
        AudioManager.sfxVolume = value;
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetFloat("MouseSensitivity", mouseSensitivitySlider.value);
        PlayerPrefs.SetFloat("MasterVolume", AudioManager.masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", AudioManager.musicVolume);
        PlayerPrefs.SetFloat("sfxVolume", AudioManager.sfxVolume);
        PlayerPrefs.SetInt("MaxFPSOption", maxFpsDropdown.value);
        PlayerPrefs.SetInt("GraphicsQuality", qualitySettingsDropdown.value);
    }
}
