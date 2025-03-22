using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HousesManager : MonoBehaviour
{
    public static HousesManager instance;

    [SerializeField] private List<string> streetNames = new();

    private List<House> houses;

    private void Awake()
    {
        if(instance != null) {
            Debug.LogError("Multiple house managers");
            Destroy(this);
            return;
        }
        instance = this;
        InitHouses();
        UpdateHouseOrdersVisual(new());
    }

    private void InitHouses()
    {
        houses = new List<House>();
        for (int i = 0; i < streetNames.Count; i++) {
            Transform street = transform.GetChild(i);
            for(int j = 0; j < street.childCount; j++) {
                House house = street.GetChild(j).GetComponent<House>();
                house.Init(streetNames[i], j + 1);
                houses.Add(house);
            }
        }
    }

    public House GetRandomHouse()
    {
        return houses[Random.Range(0, houses.Count)];
    }

    public House GetHouseByIndex(int houseIndex)
    {
        if (houseIndex >= houses.Count || houseIndex < 0)
            Debug.LogWarning("House with index: " + houseIndex + " doesn't exist");
        return houses[houseIndex % houses.Count];
    }

    public List<House> GetAllHouses()
    {
        return houses;
    }

    public int GetHouseIndex(House house)
    {
        return houses.IndexOf(house);
    }

    public void UpdateHouseOrdersVisual(List<OnlineOrderData> currentOrders)
    {
        foreach (House house in houses) {
            house.gameObject.SetActive(false);
        }

        foreach (var orderData in currentOrders) {
            orderData.house.gameObject.SetActive(true);
        }
    }
}
