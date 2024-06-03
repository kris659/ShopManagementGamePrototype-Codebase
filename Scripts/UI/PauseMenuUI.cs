using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuUI : WindowUI
{
    [SerializeField] private List<Button> buttons;
    [SerializeField] private SettingsUI settingsUI;
    [SerializeField] private SavingUI saveUI;

    internal override void Awake()
    {
        base.Awake();
        buttons[0].onClick.AddListener(OnContinueButtonPressed);
        buttons[1].onClick.AddListener(OnSaveButtonPressed);
        buttons[2].onClick.AddListener(OnSettingsButtonPressed);
        buttons[3].onClick.AddListener(OnQuitGameButtonPressed);
        settingsUI.Init(windowsManager);
        saveUI.Init(windowsManager);
    }
    public override void OpenUI()
    {
        base.OpenUI();
        Time.timeScale = 0f;
    }

    public override void CloseUI()
    {
        base.CloseUI();
        Time.timeScale = 1.0f;
    }

    private void OnContinueButtonPressed()
    {        
        CloseUI();
    }

    private void OnSaveButtonPressed()
    {
        saveUI.OpenUI();
        Time.timeScale = 0;
    }

    private void OnSettingsButtonPressed()
    {
        settingsUI.OpenUI();
        Time.timeScale = 0;
    }

    private void OnQuitGameButtonPressed()
    {
        Application.Quit();
    }
}
