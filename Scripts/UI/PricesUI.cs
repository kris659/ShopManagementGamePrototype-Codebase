using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

public class PricesUI : WindowUI
{
    [SerializeField] private Transform listParent;
    [SerializeField] private GameObject listElementPrefab;

    [SerializeField] private TMP_Text historyTitleText;
    [SerializeField] private TMP_Text historyWholesaleLowestText;
    [SerializeField] private TMP_Text historyWholesaleHighestText;
    [SerializeField] private TMP_Text historyMarketLowestText;
    [SerializeField] private TMP_Text historyMarketHighestText;
    [SerializeField] private TMP_Dropdown categoryDropdown;

    private float minPrice = 0;
    private float maxPrice = 999.99f;
    private int currentlySelectedCategory = 0;
    ProductSO[] productsInSelectedCategory;


    private List<TMP_InputField> inputFields = new List<TMP_InputField>();
    public override bool canClose => CanClose();


    internal override void Awake()
    {
        base.Awake();
        categoryDropdown.onValueChanged.AddListener(OnCategoryDropdownValueChanged);
        categoryDropdown.ClearOptions();
        List<string> options = new List<string> { "Price changed today" };
        foreach(ProductsData.ProductsCategory productCategory in ProductsData.instance.productsCategories) {
            options.Add(productCategory.name);
        }
        categoryDropdown.AddOptions(options);
    }

    public override void OpenUI()
    {
        base.OpenUI();
        UpdateList();
    }

    public override void CloseUI()
    {
        base.CloseUI();
        for(int i = 0; i < inputFields.Count; i++) {
            OnInputFieldSubmit(i, inputFields[i].text);
        }
    }

    void UpdateList()
    {
        DestroyChildren(listParent);
        
        inputFields.Clear();
        if(currentlySelectedCategory == 0) {
            productsInSelectedCategory = new ProductSO[PriceManager.instance.currentChangedPricesIndexes.Count];
            for(int i = 0;i < productsInSelectedCategory.Length; i++) {
                productsInSelectedCategory[i] = SOData.productsList[PriceManager.instance.currentChangedPricesIndexes[i]];
            }
        }
        else {
            productsInSelectedCategory = ProductsData.instance.GetCategoryProducts(currentlySelectedCategory - 1);
        }

        RectTransform rectTransform = listParent.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, 70 * productsInSelectedCategory.Length);
        rectTransform.localPosition = Vector3.zero;

        for (int i = 0; i < productsInSelectedCategory.Length; i++) {
            GameObject buttonGO = Instantiate(listElementPrefab, listParent);
            MouseHandler mouseHandler = buttonGO.AddComponent<MouseHandler>();

            ProductSO productData = productsInSelectedCategory[i];
            int productIndex = SOData.GetProductIndex(productData);

            mouseHandler.OnMouseEnterEvent += (_) => { OnMouseEnterProduct(productIndex); };
            buttonGO.SetActive(true);
            
            TMP_Text nameText = buttonGO.transform.GetChild(0).GetComponent<TMP_Text>();
            TMP_Text buyPrice = buttonGO.transform.GetChild(1).GetComponent<TMP_Text>();
            TMP_Text marketPrice = buttonGO.transform.GetChild(2).GetComponent<TMP_Text>();
            TMP_InputField sellingPriceInputField = buttonGO.transform.GetChild(3).GetComponentInChildren<TMP_InputField>();
            inputFields.Add(sellingPriceInputField);

            
            nameText.text = productData.Name;
            buyPrice.text = "$" + PriceManager.instance.GetWholesalePrice(productIndex).ToString("0.00").Replace(',', '.');
            marketPrice.text = "$" + PriceManager.instance.GetMarketPrice(productIndex).ToString("0.00").Replace(',','.');
            sellingPriceInputField.text = "$" + PriceManager.instance.GetProductSellPrice(productIndex).ToString().Replace(',', '.');

            int inputFieldIndex = i;
            sellingPriceInputField.onValueChanged.AddListener((string value) => { OnInputFieldValueChanged(inputFieldIndex, value); });
            sellingPriceInputField.onSubmit.AddListener((string value) => { OnInputFieldSubmit(inputFieldIndex, value); });
            sellingPriceInputField.onDeselect.AddListener((string value) => { OnInputFieldSubmit(inputFieldIndex, value); });
        }
        OnMouseEnterProduct(0);
    }

    private void OnCategoryDropdownValueChanged(int index)
    {
        currentlySelectedCategory = index;
        UpdateList();
    }

    private void OnInputFieldValueChanged(int inputFieldIndex, string textValue)
    {
        if (!string.IsNullOrEmpty(textValue)){            
            textValue = textValue.Replace('.', ',');
            if(textValue.IndexOf(",") != -1 && textValue.Length > textValue.IndexOf(",") + 3) {
                textValue = textValue.Substring(0, textValue.IndexOf(",") + 3);
            }

            float value;
            if (textValue.StartsWith('$'))
                float.TryParse(textValue.Remove(0, 1), out value);            
            else
                float.TryParse(textValue, out value);
            
            //if (value < minPrice) value = minPrice;
            //if (value > maxPrice) value = maxPrice;
            inputFields[inputFieldIndex].text = textValue.Replace(',', '.');
            if (inputFields[inputFieldIndex].text == "0" || inputFields[inputFieldIndex].text == "$0")
                inputFields[inputFieldIndex].text = "";

        }
    }

    private void OnInputFieldSubmit(int inputFieldIndex, string textValue)
    {
        if (!string.IsNullOrEmpty(textValue)) {
            if (textValue.StartsWith('$'))
                textValue = textValue.Remove(0, 1);

            textValue = textValue.Replace('.', ',');
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ",";
            float.TryParse(textValue, NumberStyles.Any, nfi, out float value);

            //float.TryParse(textValue, out float valueWithDot);
            //textValue = textValue.Replace('.', ',');
            //float.TryParse(textValue, out float value);
            //if(valueWithDot != 0 && valueWithDot < value) {
            //    value = valueWithDot;
            //}

            if (value < minPrice) value = minPrice;
            if (value > maxPrice) value = maxPrice;

            inputFields[inputFieldIndex].text = "$" + value.ToString().Replace(',', '.');
            int productIndex = SOData.GetProductIndex(productsInSelectedCategory[inputFieldIndex]);
            PriceManager.instance.SetProductSellPrice(productIndex, value);
        }
    }

    private void OnMouseEnterProduct(int productIndex)
    {
        historyTitleText.text = SOData.productsList[productIndex].Name;

        historyWholesaleLowestText.text = "Lowest: $" + PriceManager.instance.GetLowestWholesalePrice(productIndex).ToString("0.00").Replace(",", ".");
        historyWholesaleHighestText.text = "Highest: $" + PriceManager.instance.GetHighestWholesalePrice(productIndex).ToString("0.00").Replace(",", ".");
        historyMarketLowestText.text = "Lowest: $" + PriceManager.instance.GetLowestMarketPrice(productIndex).ToString("0.00").Replace(",", ".");
        historyMarketHighestText.text = "Highest: $" + PriceManager.instance.GetHighestMarketPrice(productIndex).ToString("0.00").Replace(",", ".");
    }
    bool CanClose()
    {
        foreach (TMP_InputField inputField in inputFields) {
            if (inputField.isFocused)
                return false;
        }
        return true;
    }
}
