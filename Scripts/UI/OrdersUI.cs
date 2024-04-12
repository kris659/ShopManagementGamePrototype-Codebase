using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrdersUI : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button orderButton;
    [SerializeField] private TMP_Text totalPriceText;

    GameObject UIGameObject;

    private int minValue = 0;
    private int maxValue = 200;

    private int selectedItemIndex;
    private int inputFieldValue;
    private int totalPrice;

    private OrdersManager ordersManager;

    public bool isOpen { get { return UIGameObject.activeSelf; } }
    private void Start()
    {
        UIGameObject = transform.GetChild(0).gameObject;
        CloseUI();
    }

    public void Init(OrdersManager ordersManager)
    {
        this.ordersManager = ordersManager;
        dropdown.ClearOptions();
        List<string> options = new List<string>();
        for (int i = 0; i < SOData.sellableProductsList.Count; i++) {
            options.Add(SOData.sellableProductsList[i].Name);
        }
        dropdown.AddOptions(options);
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        inputField.onValueChanged.AddListener(OnInputFieldValueChanged);
        orderButton.onClick.AddListener(OnOrderButtonClicked);
        UpdatePrice();
    }

    private void OnOrderButtonClicked()
    {
        if (!PlayerData.instance.CanAfford(totalPrice)) {
            Debug.LogError("Button should not be enabled");
            return;
        }
        PlayerData.instance.TakeMoney(totalPrice);
        ordersManager.SpawnProducts(selectedItemIndex, inputFieldValue);
        CloseUI();
    }

    private void OnDropdownValueChanged(int value)
    {
        selectedItemIndex = value;
        selectedItemIndex = SOData.GetProductIndex(SOData.sellableProductsList[selectedItemIndex]);
        UpdatePrice();
    }
    private void OnInputFieldValueChanged(string textValue)
    {
        if (!string.IsNullOrEmpty(textValue)) {
            int.TryParse(textValue, out int value);
            if (value < minValue) value = minValue;
            if (value > maxValue) value = maxValue;
            inputField.text = value.ToString();
            inputFieldValue = value;
            UpdatePrice();
        }
    }

    private void UpdatePrice()
    {
        totalPrice = SOData.productsList[selectedItemIndex].Price * inputFieldValue;
        totalPriceText.text = "Total: " + totalPrice + "$";
        orderButton.interactable = PlayerData.instance.CanAfford(totalPrice);
    }

    public void OpenUI()
    {
        PlayerInteractions.Instance.LockCameraForUI();
        Cursor.lockState = CursorLockMode.None;
        UIGameObject.SetActive(true);
    }

    public void CloseUI()
    {
        PlayerInteractions.Instance.UnlockCameraForUI();

        Cursor.lockState = CursorLockMode.Locked;
        UIGameObject.SetActive(false);
    }
}
