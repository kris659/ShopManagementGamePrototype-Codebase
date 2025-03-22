using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NextDayUI : WindowUI
{
    [SerializeField] GameObject daySummary;
    [SerializeField] GameObject priceChanges;

    [SerializeField] TMP_Text summaryTitle;
    [SerializeField] Button continueButton;



    [SerializeField] TMP_Text priceChangesTitle;
    [SerializeField] Button nextDayButton;

    [SerializeField] Transform priceChangesParent;
    [SerializeField] GameObject priceChangePrefab;


    public override bool canClose => false;

    internal override void Awake()
    {
        base.Awake();
        continueButton.onClick.AddListener(OnContinueButtonPressed);
        nextDayButton.onClick.AddListener(CloseUI);
    }

    public override void OpenUI()
    {
        base.OpenUI();
        UpdateUI();
        daySummary.SetActive(true);
        priceChanges.SetActive(false);
        Time.timeScale = 0f;
    }
    public override void CloseUI()
    {
        if(daySummary.activeSelf) {
            OnContinueButtonPressed();
            return;
        }
        base.CloseUI();
        Time.timeScale = 1.0f;
    }

    private void UpdateUI()
    {
        summaryTitle.text = "Day " + (TimeManager.instance.Day - 1) + " - Summary";

        PriceManager.instance.UpdatePrices();
        DestroyChildren(priceChangesParent);
        priceChangesTitle.text = "Price changes";

        List<int> newIndexes = PriceManager.instance.currentChangedPricesIndexes;

        for (int i = 0; i < newIndexes.Count; i++) {
            if(PriceManager.instance.GetPreviousWholesalePrice(newIndexes[i]) != PriceManager.instance.GetWholesalePrice(newIndexes[i]))
                CreatePriceChangeGO(newIndexes[i], i);
        }
        for (int i = 0; i < newIndexes.Count; i++) {
            if (PriceManager.instance.GetPreviousWholesalePrice(newIndexes[i]) == PriceManager.instance.GetWholesalePrice(newIndexes[i]))
                CreatePriceChangeGO(newIndexes[i], i);
        }
    }

    private void CreatePriceChangeGO(int productIndex, int changeIndex)
    {
        GameObject gameObject = Instantiate(priceChangePrefab, priceChangesParent);
        gameObject.SetActive(true);

        TMP_Text nameText = gameObject.transform.GetChild(0).GetComponent<TMP_Text>();

        TMP_Text wholesalePriceLeftText = gameObject.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>();
        TMP_Text wholesalePriceMiddleText = gameObject.transform.GetChild(1).GetChild(1).GetComponent<TMP_Text>();
        TMP_Text wholesalePriceRightText = gameObject.transform.GetChild(1).GetChild(2).GetComponent<TMP_Text>();
        TMP_Text wholesaleLowestHighestText = gameObject.transform.GetChild(2).GetComponent<TMP_Text>();

        TMP_Text marketPriceLeftText = gameObject.transform.GetChild(3).GetChild(0).GetComponent<TMP_Text>();
        TMP_Text marketPriceMiddleText = gameObject.transform.GetChild(3).GetChild(1).GetComponent<TMP_Text>();
        TMP_Text marketPriceRightText = gameObject.transform.GetChild(3).GetChild(2).GetComponent<TMP_Text>();
        TMP_Text marketLowestHighestText = gameObject.transform.GetChild(4).GetComponent<TMP_Text>();

        wholesaleLowestHighestText.text = "";
        marketLowestHighestText.text = "";

        nameText.text = SOData.productsList[productIndex].Name;

        float previousWholesalePrice = PriceManager.instance.GetPreviousWholesalePrice(productIndex);
        float currentWholesalePrice = PriceManager.instance.GetWholesalePrice(productIndex);
        float previousMarketPrice = PriceManager.instance.GetPreviousMarketPrice(productIndex);
        float currentMarketPrice = PriceManager.instance.GetMarketPrice(productIndex);
        int currentLowestHighest = PriceManager.instance.currentChangedLowestHighest[changeIndex];

        if (previousWholesalePrice == currentWholesalePrice) {
            wholesalePriceLeftText.text = "";
            wholesalePriceMiddleText.text = "$" + previousWholesalePrice.ToString("0.00").Replace(',', '.');
            wholesalePriceRightText.text = "";
        }
        else {
            wholesalePriceLeftText.text = "$" + previousWholesalePrice.ToString("0.00").Replace(',', '.');
            wholesalePriceMiddleText.text = "->";
            wholesalePriceRightText.text = "$" + currentWholesalePrice.ToString("0.00").Replace(',', '.');

            if (currentLowestHighest == -1)
                wholesaleLowestHighestText.text = "New lowest";
            if (currentLowestHighest == 1)
                wholesaleLowestHighestText.text = "New highest";
        }

        if (previousMarketPrice == currentMarketPrice) {
            marketPriceLeftText.text = "";
            marketPriceMiddleText.text = "$" + currentMarketPrice.ToString("0.00").Replace(',', '.');
            marketPriceRightText.text = "";
        }
        else {
            marketPriceLeftText.text = "$" + previousMarketPrice.ToString("0.00").Replace(',', '.');
            marketPriceMiddleText.text = "->";
            marketPriceRightText.text = "$" + currentMarketPrice.ToString("0.00").Replace(',', '.');
            if (currentLowestHighest == -1)
                marketLowestHighestText.text = "New lowest";
            if (currentLowestHighest == 1)
                marketLowestHighestText.text = "New highest";
        }
    }

    private void OnContinueButtonPressed()
    {
        daySummary.SetActive(false);
        priceChanges.SetActive(true);
    }
}
