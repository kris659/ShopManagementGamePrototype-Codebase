using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LicencesUI : WindowUI
{
    [SerializeField] Transform listParent;
    [SerializeField] GameObject elementPrefab;
    [SerializeField] GameObject productNameTextPrefab;

    public override void OpenUI()
    {
        base.OpenUI();
        UpdateList();
    }

    public void UpdateList()
    {
        DestroyChildren(listParent);

        RectTransform rectTransform = listParent.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(10 + 290 * ProductsData.instance.productsLicenses.Count, rectTransform.sizeDelta.y);
        rectTransform.localPosition = new Vector3(0, rectTransform.localPosition.y, 0);

        for (int i = 0; i < ProductsData.instance.productsLicenses.Count; i++) {
            ProductsData.LicenseData licenseData = ProductsData.instance.productsLicenses[i];
            GameObject element = Instantiate(elementPrefab, listParent);
            element.SetActive(true);

            TMP_Text titleText = element.transform.GetChild(1).GetComponent<TMP_Text>();
            Transform producNamesParent = element.transform.GetChild(2);
            DestroyChildren(producNamesParent);
            TMP_Text priceText = element.transform.GetChild(3).GetComponent<TMP_Text>();
            Button unlockButton = element.transform.GetChild(4).GetComponent<Button>();

            int index = i;
            unlockButton.interactable = PlayerData.instance.CanAfford(ProductsData.instance.productsLicenses[i].unlockPrice);
            unlockButton.onClick.AddListener(() => OnUnlockButtonPressed(index));

            titleText.text = licenseData.name;
            priceText.gameObject.SetActive(true);
            priceText.text = "$" + licenseData.unlockPrice;           


            if (licenseData.products.Length == 0) {
                priceText.gameObject.SetActive(false);
                unlockButton.interactable = false;
                GameObject comingSoonGameObject = element.transform.GetChild(5).gameObject;
                comingSoonGameObject.SetActive(true);
                continue;
            }
            if (ProductsData.instance.unlockedProductsLicenses[i]) {
                priceText.gameObject.SetActive(false);
                unlockButton.interactable = false;
                GameObject unlockedGameObject = element.transform.GetChild(6).gameObject;
                unlockedGameObject.SetActive(true);
                continue;
            }
            for (int j = 0; j < licenseData.products.Length; j++) {
                GameObject productNameGO = Instantiate(productNameTextPrefab, producNamesParent);
                productNameGO.SetActive(true);
                productNameGO.GetComponent<TMP_Text>().text = "- " + licenseData.products[j].name;
            }
        }
    }

    private void OnUnlockButtonPressed(int buttonIndex)
    {
        ProductsData.instance.OnUnlockLicenseButtonPressed(buttonIndex);
        UpdateList();
    }
}
