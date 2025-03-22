using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ManageShopUI : WindowUI
{
    [SerializeField] private TMP_InputField shopNameInputField;
    [SerializeField] private Button unlockLandButton;
    [SerializeField] private Button shopOpenButton;
    [SerializeField] private Image openButtonImage;
    [SerializeField] private TMP_Text openButtonText;

    [SerializeField] private Image buttonOpenPrefab;
    [SerializeField] private Image buttonClosedPrefab;

    [SerializeField] private TMP_Text shopPopularityAlwaysActiveText;
    [SerializeField] private List<GameObject> popularityCategoriesList;
    public override bool canClose => !shopNameInputField.isFocused;
    private float changeDuration = 0.5f;

    private string[] popularityCategoryNames = new string[6] { 
        "Shop popularity", 
        "Shop size", 
        "Decorations",
        "Overcrowding",
        "Customer service",
        "Products variety"
    };

    internal override void Awake()
    {
        base.Awake();
        shopOpenButton.onClick.AddListener(OnShopOpenButtonClicked);
        shopNameInputField.onValueChanged.AddListener(ShopData.instance.ChangeShopName);
        DOTween.Init();
        UpdateVisual(true);
    }

    private void Start()
    {
        UIManager.landUnlockUI.Init(windowsManager.windowsManager);
        unlockLandButton.onClick.AddListener( () => { windowsManager.CloseUI(); UIManager.landUnlockUI.OpenUI(); });
    }

    private void OnShopOpenButtonClicked()
    {
        ShopData.instance.ChangeShopOpenStatus(!ShopData.instance.isShopOpen);
        UpdateVisual();
    }

    public override void OpenUI()
    {
        base.OpenUI();
        UpdateVisual(true);
    }

    public void UpdatePopularityUI()
    {
        List<float> popularityValues = ShopPopularityManager.instance.shopPopularityValues;
        for (int i = 0; i < popularityValues.Count; i++) {
            TMP_Text text = popularityCategoriesList[i].GetComponentInChildren<TMP_Text>();
            text.text = popularityCategoryNames[i] + ": " + popularityValues[i].ToString("0.0").Replace(",", ".") + "/10";
        }
        shopPopularityAlwaysActiveText.text = popularityCategoryNames[0] + ": " + popularityValues[0].ToString("0.0").Replace(",", ".") + "/10";
    }

    private void UpdateVisual(bool instant = false)
    {
        float duration = changeDuration;
        if (instant)
            duration = 0;
        if (ShopData.instance.isShopOpen) {
            openButtonImage.transform.DOMove(buttonOpenPrefab.transform.position, duration);
            openButtonText.transform.DOMove(buttonOpenPrefab.transform.position, duration);
            openButtonImage.DOColor(buttonOpenPrefab.color, duration);
            openButtonText.text = "Open";
        }
        else {
            openButtonImage.transform.DOMove(buttonClosedPrefab.transform.position, duration);
            openButtonText.transform.DOMove(buttonClosedPrefab.transform.position, duration);
            openButtonImage.DOColor(buttonClosedPrefab.color, duration);
            openButtonText.text = "Closed";
        }
        shopNameInputField.text = ShopData.instance.shopName;

        UpdatePopularityUI();
    }
}
