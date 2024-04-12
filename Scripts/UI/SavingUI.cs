using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SavingUI : MonoBehaviour
{
    private GameObject UIGameObject;
    private SavingManager savingManager;

    [SerializeField] private TMP_InputField savingInputField;
    [SerializeField] private TMP_Dropdown loadingDropdown;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;

    private List<string> options;
    public bool isOpen = false;
    public void Init(SavingManager savingManager)
    {
        UIGameObject = transform.GetChild(0).gameObject;
        CloseUI();
        this.savingManager = savingManager;
        
        saveButton.onClick.AddListener(OnSaveButtonClicked);
        loadButton.onClick.AddListener(OnLoadButtonClicked);
        loadingDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    public void OpenUI()
    {
        PlayerInteractions.Instance.LockCameraForUI();
        isOpen = true;
        UIGameObject.SetActive(true);
        loadingDropdown.ClearOptions();

        options = new List<string> { "Select save to load" };
        options.AddRange(SaveFilesManager.GetSaveFileNames());     
        
        loadingDropdown.AddOptions(options);
        Cursor.lockState = CursorLockMode.None;
    }

    public void CloseUI()
    {
        PlayerInteractions.Instance.UnlockCameraForUI();
        isOpen = false; 
        UIGameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
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
