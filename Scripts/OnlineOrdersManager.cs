using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct OnlineOrderData {
    public House house;
    public int productTypeIndex;
    public int amount;
    public int price;
    public int timeLeft;

    public OnlineOrderData(OnlineOrderSaveData saveData)
    {
        house = HousesManager.instance.GetHouseByIndex(saveData.houseIndex);
        productTypeIndex = saveData.houseIndex;
        amount = saveData.productType;
        price = saveData.price;
        timeLeft = saveData.timeLeft;
    }    
}


public class OnlineOrdersManager : MonoBehaviour
{
    public static OnlineOrdersManager instance;

    [SerializeField] private int ordersCooldown = 2;
    private int maxOrders = 2;

    public List<OnlineOrderData> currentOrders = new();

    public readonly int[] deliveryFees = { 40, 30, 20, 15};
    public readonly int[] possibleDeliveryTimes ={ 6, 12, 24, 48 };
    public int selectedDeliveryType;

    private void Awake()
    {
        if (instance != null) {
            Debug.LogError("Multiple OnlineOrdersManagers");
            Destroy(this);
            return;
        }
        instance = this;
        StartCoroutine(CreateOrdersCoroutine());
        ShopData.instance.onOnlineOrdersDisabled += OnOnlineOrdersDisabled;
    }

    private void Start()
    {
        TimeManager.instance.OnHourChanged = UpdateOrdersTime;
    }

    private IEnumerator CreateOrdersCoroutine()
    {
        yield return new WaitForSeconds(ordersCooldown);
        if (!MainMenu.isMainMenuOpen && ShopData.instance.AreOnlineOrdersEnabled && currentOrders.Count < maxOrders) {
            CreateRandomOrder();
            UIManager.onlineOrdersUI.UpdateOrdersList();
            HousesManager.instance.UpdateHouseOrdersVisual(currentOrders);
        }
        StartCoroutine(CreateOrdersCoroutine());
    }

    private void CreateRandomOrder()
    {
        OnlineOrderData orderData = new OnlineOrderData();
        orderData.house = HousesManager.instance.GetRandomHouse();
        orderData.productTypeIndex = ProductsData.instance.GetRandomProduct();
        if (!PriceDecision(orderData.productTypeIndex))
            return;
        orderData.amount = Random.Range(4, 10);
        orderData.price = Mathf.RoundToInt(orderData.amount * PriceManager.instance.GetProductSellPrice(orderData.productTypeIndex) + deliveryFees[selectedDeliveryType]);
        orderData.timeLeft = possibleDeliveryTimes[selectedDeliveryType];
        currentOrders.Add(orderData);
    }

    bool PriceDecision(int productTypeIndex) // Skopiowane z Customer.cs
    {
        float marketPrice = PriceManager.instance.GetMarketPrice(productTypeIndex);
        float price = PriceManager.instance.GetProductSellPrice(productTypeIndex);

        float chance = 50 + Mathf.RoundToInt((marketPrice - price) / marketPrice * 100);
        chance *= SOData.productsList[productTypeIndex].Popularity / 100.0f;
        //Debug.Log(product.productType.Name + " Chance: " + chance);
        return Random.Range(0, 100) < chance;
    }

    private void OnOnlineOrdersDisabled()
    {
        currentOrders.Clear();        
    }

    public void CheckOnlineOrderCompletition(House house, ContainerGO containerGO)
    {
        if (house == null || containerGO == null) {
            Debug.Log("Null value?");
            return;
        }
        for (int i = 0; i < currentOrders.Count; i++) {
            if (currentOrders[i].house == house) {
                int productsCount = 0;
                containerGO.container.GetProductsInContainerData(out List<Product> productsInContainer, out _, out _);
                for (int j = 0; j < productsInContainer.Count; j++) {
                    //Debug.Log(j + ": " + productsInContainer[j].productTypeIndex + " " + currentOrders[i].productTypeIndex);
                    if (productsInContainer[j].productTypeIndex == currentOrders[i].productTypeIndex)
                        productsCount++;
                }

                if (productsCount > currentOrders[i].amount) {
                    string warningText = "There are to many products in a box! All excesive products will be lost. Do you want to continue?";
                    UIManager.confirmUI.OpenUI(warningText, () => OnOnlineOrderCompletition(i, house, containerGO), null, true);
                    return;
                }
                if (productsCount == currentOrders[i].amount) {
                    OnOnlineOrderCompletition(i, house, containerGO);
                    return;
                }
            }
        }
        Debug.Log("Can't be finished");
    }

    private void OnOnlineOrderCompletition(int orderIndex, House house, ContainerGO containerGO)
    {
        OnlineOrderData orderData = currentOrders[orderIndex];
        currentOrders.RemoveAt(orderIndex);
        HousesManager.instance.UpdateHouseOrdersVisual(currentOrders);
        PlayerData.instance.AddMoney(orderData.price, true);
        containerGO.container.RemoveFromGame(true);
    }

    private void UpdateOrdersTime()
    {
        for (int i = 0; i < currentOrders.Count; i++) {
            var currentOrder = currentOrders[i];
            currentOrder.timeLeft--;
            currentOrders[i] = currentOrder;
        }
        int ordersCount = currentOrders.Count;

        currentOrders.RemoveAll(s => s.timeLeft <= 0);
        HousesManager.instance.UpdateHouseOrdersVisual(currentOrders);

        while (ordersCount > currentOrders.Count) {
            float rating = ShopPopularityManager.instance.shopPopularityValues[(int)ShopPopularityCategory.CustomerService];
            float newRating = rating - 1;
            if (rating > 3)
                newRating -= 1;
            if (rating > 6)
                newRating -= 1;
            ShopPopularityManager.instance.UpdatePopularity(ShopPopularityCategory.CustomerService, Mathf.Max(0, newRating));
        }

        UIManager.onlineOrdersUI.UpdateOrdersList();
    }

    public OnlineOrdersManagerSaveData GetSaveData()
    {
        OnlineOrdersManagerSaveData saveData = new OnlineOrdersManagerSaveData(selectedDeliveryType, new OnlineOrderSaveData[currentOrders.Count]);
        for (int i = 0; i < saveData.onlineOrdersData.Length; i++) {
            int houseIndex = HousesManager.instance.GetHouseIndex(currentOrders[i].house);
            saveData.onlineOrdersData[i] = new OnlineOrderSaveData(houseIndex, currentOrders[i].productTypeIndex, currentOrders[i].amount, currentOrders[i].price, currentOrders[i].timeLeft);
        }
        return saveData;
    }

    public void LoadSaveData(OnlineOrdersManagerSaveData saveData)
    {
        currentOrders.Clear();
        if (saveData == null || saveData.onlineOrdersData == null) {
            selectedDeliveryType = 2;
            return;
        }

        for (int i = 0; i < saveData.onlineOrdersData.Length; i++) {
            currentOrders.Add(new OnlineOrderData(saveData.onlineOrdersData[i]));
        }
        selectedDeliveryType = saveData.selectedDeliveryType;
        HousesManager.instance.UpdateHouseOrdersVisual(currentOrders);
    }
}
