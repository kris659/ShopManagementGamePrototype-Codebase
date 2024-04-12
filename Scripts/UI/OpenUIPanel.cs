using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpenUIPanel : MonoBehaviour
{
    [SerializeField] Button buildButton;
    [SerializeField] Button orderButton;
    [SerializeField] Button saveButton;

    private void Awake()
    {
        buildButton.onClick.AddListener(OnBuildButtonPressed);
        orderButton.onClick.AddListener(OnOrderButtonPressed);
        saveButton.onClick.AddListener(OnSaveButtonPressed);
    }

    void OnBuildButtonPressed()
    {
        if (UIManager.buildingUI.isOpen)
            UIManager.buildingUI.CloseUI();
        else
            UIManager.buildingUI.OpenUI();
    }
    void OnOrderButtonPressed()
    {
        if (UIManager.ordersUI.isOpen)
            UIManager.ordersUI.CloseUI();
        else
            UIManager.ordersUI.OpenUI();
        
    }
    void OnSaveButtonPressed()
    {
        if (UIManager.savingUI.isOpen)
            UIManager.savingUI.CloseUI();
        else
            UIManager.savingUI.OpenUI();
    }
}
