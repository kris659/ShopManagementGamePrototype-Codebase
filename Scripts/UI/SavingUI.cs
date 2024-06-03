using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SavingUI : WindowUI
{
    private SavingManager savingManager;

    [SerializeField] private TMP_InputField savingInputField;
    [SerializeField] private TMP_Dropdown loadingDropdown;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;

    public override bool canClose => !savingInputField.isFocused;

    private List<string> options;
    public void Init(SavingManager savingManager)
    {
        this.savingManager = savingManager;
        
        saveButton.onClick.AddListener(OnSaveButtonClicked);
        loadButton.onClick.AddListener(OnLoadButtonClicked);
        loadingDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    public override void OpenUI()
    {
        base.OpenUI();
        loadingDropdown.ClearOptions();
        options = new List<string> { "Select save to load" };
        options.AddRange(SaveFilesManager.GetSaveFileNames());        
        loadingDropdown.AddOptions(options);
    }

    public override void CloseUI()
    {
        base.CloseUI();
        Time.timeScale = 1.0f;
    }

    private void OnSaveButtonClicked()
    {
        string saveName = savingInputField.text;
        if (saveName == string.Empty)
            return;
        savingManager.Save(saveName);
        CloseUI();
    }
    private void OnLoadButtonClicked()
    {
        savingManager.Load(options[loadingDropdown.value]);
        CloseUI();
    }
    private void OnDropdownValueChanged(int index)
    {
        loadButton.interactable = (index != 0);
    }
}
