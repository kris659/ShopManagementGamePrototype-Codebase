using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PricesUI : WindowUI
{
    [SerializeField] private Transform listParent;
    [SerializeField] private GameObject listElementPrefab;

    float minPrice = 0;
    float maxPrice = 999.99f;

    private List<TMP_InputField> inputFields = new List<TMP_InputField>();
    public override bool canClose => CanClose();


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

        for (int i = 0; i < SOData.productsList.Length; i++) {
            GameObject buttonGO = Instantiate(listElementPrefab, listParent);
            buttonGO.SetActive(true);
            TMP_Text nameText = buttonGO.transform.GetChild(0).GetComponent<TMP_Text>();
            TMP_Text buyPrice = buttonGO.transform.GetChild(1).GetComponent<TMP_Text>();
            TMP_Text marketPrice = buttonGO.transform.GetChild(2).GetComponent<TMP_Text>();
            TMP_InputField sellingPriceInputField = buttonGO.transform.GetChild(3).GetComponentInChildren<TMP_InputField>();
            inputFields.Add(sellingPriceInputField);

            ProductSO productData = SOData.productsList[i];

            nameText.text = productData.Name;
            buyPrice.text = "$" + productData.Price;
            marketPrice.text = "$" + PriceManager.instance.GetProductMarketPrice(i).ToString().Replace(',','.');
            sellingPriceInputField.text = "$" + PriceManager.instance.GetProductSellPrice(i).ToString().Replace(',', '.');

            int index = i;
            sellingPriceInputField.onValueChanged.AddListener((string value) => { OnInputFieldValueChanged(index, value); });
            sellingPriceInputField.onSubmit.AddListener((string value) => { OnInputFieldSubmit(index, value); });
            sellingPriceInputField.onDeselect.AddListener((string value) => { OnInputFieldSubmit(index, value); });
        }
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
            
            if (value < minPrice) value = minPrice;
            if (value > maxPrice) value = maxPrice;

            inputFields[inputFieldIndex].text = textValue.Replace(',', '.');
        }
    }

    private void OnInputFieldSubmit(int inputFieldIndex, string textValue)
    {
        if (!string.IsNullOrEmpty(textValue)) {
            if (textValue.StartsWith('$'))
                textValue = textValue.Remove(0, 1);
            textValue = textValue.Replace('.', ',');
            float.TryParse(textValue, out float value);
            if (value < minPrice) value = minPrice;
            if (value > maxPrice) value = maxPrice;

            inputFields[inputFieldIndex].text = "$" + value.ToString().Replace(',', '.');
            PriceManager.instance.SetProductSellPrice(inputFieldIndex, value);
        }
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
