using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrdersUI : WindowUI
{
    [SerializeField] private Transform categoryListParent;
    [SerializeField] private GameObject categoryListElementPrefab;

    [SerializeField] private Transform productsListParent;
    [SerializeField] private GameObject productsListElementPrefab;
    private List<TMP_InputField> productsListInputFields;

    [SerializeField] private Transform ordersListParent;
    [SerializeField] private GameObject ordersListElementPrefab;


    [SerializeField] private GameObject comingSoonText;
    [SerializeField] private Button orderButton;
    [SerializeField] private TMP_Text totalPriceText;

    Dictionary<ProductSO, int> orderDictionary = new Dictionary<ProductSO, int>();

    private int minValue = 0;
    private int maxValue = 200;

    private int totalPrice;
    private int currentCategoryIndex = 0;

    private OrdersManager ordersManager;
    ProductSO[] currentProducts;

    public override bool canClose => CanCloseUI();

    public void Init(OrdersManager ordersManager)
    {
        this.ordersManager = ordersManager;
        orderButton.onClick.AddListener(OnOrderButtonClicked);
        productsListInputFields = new List<TMP_InputField>();
        InitCategoryList();
        UpdateTotalPrice();
    }

    public override void OpenUI()
    {
        base.OpenUI();
        UpdateTotalPrice();
        UpdateProductsList(currentCategoryIndex);
    }


    private void InitCategoryList()
    {
        DestroyChildren(categoryListParent);
        string[] categoryNames = ProductsData.instance.GetCategoryNames();
        for (int i = 0; i < categoryNames.Length; i++) {
            GameObject categoryButtonGO = Instantiate(categoryListElementPrefab, categoryListParent).gameObject;
            categoryButtonGO.SetActive(true);
            Button categoryButton = categoryButtonGO.transform.GetComponent<Button>();
            TMP_Text nameText = categoryButtonGO.transform.GetChild(1).GetComponent<TMP_Text>();
            nameText.text = categoryNames[i];
            int index = i;
            categoryButton.onClick.AddListener(() => { OnCategoryButtonPressed(index); });
        }
        UpdateProductsList(currentCategoryIndex);
    }

    private void UpdateProductsList(int categoryIndex)
    {
        currentCategoryIndex = categoryIndex;
        DestroyChildren(productsListParent);
        productsListInputFields.Clear();
        currentProducts = ProductsData.instance.GetCategoryProducts(categoryIndex);
        comingSoonText.SetActive(currentProducts.Length == 0);
        for (int i = 0; i < currentProducts.Length; i++) {
            GameObject productListElement = Instantiate(productsListElementPrefab, productsListParent).gameObject;
            productListElement.SetActive(true);
            TMP_Text nameText = productListElement.transform.GetChild(0).GetComponent<TMP_Text>();
            TMP_Text priceText = productListElement.transform.GetChild(1).GetComponent<TMP_Text>();
            TMP_InputField amountInputField = productListElement.transform.GetChild(2).GetComponent<TMP_InputField>();
            Button decreaseButton = productListElement.transform.GetChild(3).GetComponent<Button>();
            Button increaseButton = productListElement.transform.GetChild(4).GetComponent<Button>();

            ProductSO product = currentProducts[i];
            nameText.text = product.Name;
            priceText.text = "$" + product.Price;

            if(orderDictionary.TryGetValue(product, out int value))
                amountInputField.text = value.ToString();
            else
                amountInputField.text = "0";

            int index = i;
            decreaseButton.onClick.AddListener(() => { OnProductsAmountButtonPressed(index, -1); });
            increaseButton.onClick.AddListener(() => { OnProductsAmountButtonPressed(index, 1); });
            amountInputField.onValueChanged.AddListener((string value) => { OnInputFieldValueChanged(index, value); });
            amountInputField.onDeselect.AddListener((string value) => { OnInputFieldValueChanged(index, value); });
            productsListInputFields.Add(amountInputField);
        }
    }

    private void UpdateOrderList()
    {
        DestroyChildren(ordersListParent);
        ProductSO[] keys = orderDictionary.Keys.ToArray();
        foreach (ProductSO productSO in keys) {
            GameObject orderListElement = Instantiate(ordersListElementPrefab, ordersListParent).gameObject;
            orderListElement.SetActive(true);
            TMP_Text nameText = orderListElement.transform.GetChild(0).GetComponent<TMP_Text>();
            TMP_Text priceText = orderListElement.transform.GetChild(1).GetComponent<TMP_Text>();

            nameText.text = productSO.Name + " x" + orderDictionary[productSO];
            priceText.text = "$" + productSO.Price * orderDictionary[productSO];
        }
    }

    private void OnCategoryButtonPressed(int buttonIndex)
    {
        UpdateProductsList(buttonIndex);
    }

    private void OnProductsAmountButtonPressed(int buttonIndex, int buttonValue)
    {
        int.TryParse(productsListInputFields[buttonIndex].text, out int value);
        value += buttonValue;
        if (value < minValue) value = minValue;
        if (value > maxValue) value = maxValue;
        productsListInputFields[buttonIndex].text = value.ToString();
        if (value == 0 && orderDictionary.ContainsKey(currentProducts[buttonIndex]))
            orderDictionary.Remove(currentProducts[buttonIndex]);
        if(value != 0)
            orderDictionary[currentProducts[buttonIndex]] = value;        
    }

    private void OnOrderButtonClicked()
    {
        if (!PlayerData.instance.CanAfford(totalPrice)) {
            Debug.LogError("Button should not be enabled");
            return;
        }
        //PlayerData.instance.TakeMoney(totalPrice);
        ordersManager.SpawnProducts(orderDictionary);
        //orderDictionary.Clear();
        CloseUI();
    }

    private void OnInputFieldValueChanged(int inputFieldIndex, string textValue)
    {
        int.TryParse(textValue, out int value);
        if (value < minValue) value = minValue;
        if (value > maxValue) value = maxValue;
        productsListInputFields[inputFieldIndex].text = value.ToString();

        if (value == 0 && orderDictionary.ContainsKey(currentProducts[inputFieldIndex]))
            orderDictionary.Remove(currentProducts[inputFieldIndex]);
        if (value != 0)
            orderDictionary[currentProducts[inputFieldIndex]] = value;

        UpdateTotalPrice();
    }

    private void UpdateTotalPrice()
    {
        ProductSO[] keys = orderDictionary.Keys.ToArray();
        totalPrice = 0;
        foreach(ProductSO productSO in keys) {
            totalPrice += productSO.Price * orderDictionary[productSO];
        }
        totalPriceText.text = "Total: $" + totalPrice;
        orderButton.interactable = PlayerData.instance.CanAfford(totalPrice);
        UpdateOrderList();
    }

    private bool CanCloseUI()
    {
        for (int i = 0; i < productsListInputFields.Count; i++) {
            if (productsListInputFields[i].isFocused)
                return false;
        }
        return true;
    }
}
