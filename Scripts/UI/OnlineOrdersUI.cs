using System;
using System.Linq;
using TMPro;
using UnityEngine;


public class OnlineOrdersUI : WindowUI
{
    [SerializeField] private GameObject lockedUI;
    [SerializeField] private GameObject unlockedUI;
    [SerializeField] private GameObject rowPrefab;
    [SerializeField] private GameObject orderElementPrefab;
    [SerializeField] private Transform ordersListParent;

    [SerializeField] private TMP_Text deliveryFeeText;
    [SerializeField] private TMP_Dropdown deliveryTimeDropdown;

    internal override void Awake()
    {
        base.Awake();
        deliveryTimeDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        deliveryTimeDropdown.ClearOptions();
        deliveryTimeDropdown.AddOptions(OnlineOrdersManager.instance.possibleDeliveryTimes.ToList().ConvertAll(x => x.ToString() + "h"));
    }

    public override void OpenUI()
    {
        base.OpenUI();
        lockedUI.SetActive(!ShopData.instance.AreOnlineOrdersEnabled);
        unlockedUI.SetActive(ShopData.instance.AreOnlineOrdersEnabled);
        UpdateOrdersList();
        deliveryTimeDropdown.value = OnlineOrdersManager.instance.selectedDeliveryType;
    }

    public void UpdateOrdersList()
    {
        if(!SceneLoader.isUISceneLoaded) 
            return;

        DestroyChildren(ordersListParent);

        int maxElementsInRow = 3;
        GameObject rowParent = null;
        for (int i = 0; i < OnlineOrdersManager.instance.currentOrders.Count; i++) {

            if (i % maxElementsInRow == 0) {
                rowParent = Instantiate(rowPrefab, ordersListParent);
                rowParent.SetActive(true);
            }

            GameObject orderGO = Instantiate(orderElementPrefab, rowParent.transform);
            orderGO.SetActive(true);

            TMP_Text adressText = orderGO.transform.GetChild(1).GetComponent<TMP_Text>();
            TMP_Text orderText = orderGO.transform.GetChild(2).GetComponent<TMP_Text>();
            TMP_Text timeLeft = orderGO.transform.GetChild(3).GetComponent<TMP_Text>();

            OnlineOrderData onlineOrderData = OnlineOrdersManager.instance.currentOrders[i];
            string productName = SOData.productsList[onlineOrderData.productTypeIndex].Name;

            adressText.text = onlineOrderData.house.streetName + "  " + onlineOrderData.house.houseNumber;
            orderText.text = onlineOrderData.amount + " " + productName;
            timeLeft.text = "Time left: " + onlineOrderData.timeLeft + " h";
        }
    }

    private void OnDropdownValueChanged(int index)
    {
        OnlineOrdersManager.instance.selectedDeliveryType = index;
        deliveryFeeText.text = "Delivery fee: $" + OnlineOrdersManager.instance.deliveryFees[index];
    }
}
