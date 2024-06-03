using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ManageShopUI : WindowUI
{
    [SerializeField] private TMP_InputField shopNameInputField;
    [SerializeField] private TMP_Text maxCustomersText;
    [SerializeField] private Button unlockLandButton;
    [SerializeField] private Button shopOpenButton;
    [SerializeField] private Image openButtonImage;
    [SerializeField] private TMP_Text openButtonText;

    [SerializeField] private Image buttonOpenPrefab;
    [SerializeField] private Image buttonClosedPrefab;

    public override bool canClose => !shopNameInputField.isFocused;
    private float changeDuration = 0.5f;

    
    internal override void Awake()
    {
        base.Awake();
        shopOpenButton.onClick.AddListener(OnShopOpenButtonClicked);
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
        maxCustomersText.text = "Max customers: " + ShopData.instance.MaxCustomers.ToString();
    }

    public override void UpdateOnParentOpen()
    {
        base.UpdateOnParentOpen();
        maxCustomersText.text = "Max customers: " + ShopData.instance.MaxCustomers.ToString();
    }
}
