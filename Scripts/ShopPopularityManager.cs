using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ShopPopularityCategory
{
    OverallPopularity,
    ShopSize,
    Decorations,
    Overcrowding,
    CustomerService,
    ProductsVariety
}

public class ShopPopularityManager : MonoBehaviour
{
    public static ShopPopularityManager instance;
    public List<float> shopPopularityValues;

    //private float[] shopStartingPopularityValues = new float[6] {
    //    0,  //"Shop popularity"
    //    0,  //"Shop size"
    //    0,  //"Decorations"
    //    0,  //"Overcrowding"
    //    0,  //"Customer service"
    //    0,  //"Products variety"
    //};

    private void Awake()
    {
        if(instance != null) {
            Debug.LogError("Multiple shop popularity managers");
            return;
        }
        instance = this;
        shopPopularityValues = new List<float>(new float[6]);
    }
    public void UpdatePopularity(ShopPopularityCategory category, float newValue)
    {
        shopPopularityValues[(int)category] = Mathf.Clamp(newValue, 0, 10);
        UpdateOverallPopularity();
    }

    private void UpdateOverallPopularity()
    {
        float popularity = 0;
        for(int i = 1; i < shopPopularityValues.Count; i++) popularity += shopPopularityValues[i];
        //Debug.Log("Sum: " + popularity);
        popularity = Mathf.Min(popularity / (shopPopularityValues.Count - 1), 10f);
        shopPopularityValues[0] = popularity;
        UIManager.manageShopUI.UpdatePopularityUI();
    }
    public void LoadPopularitySaveData(ShopSaveData shopSaveData)
    {
        shopPopularityValues = new List<float>();
        if (shopSaveData.shopPopularityValues != null) {
            shopPopularityValues = shopSaveData.shopPopularityValues.ToList();
        }
        while (shopPopularityValues.Count < Enum.GetValues(typeof(ShopPopularityCategory)).Length) {
            shopPopularityValues.Add(0);//(shopStartingPopularityValues[shopPopularityValues.Count]);
        }
        UpdateOverallPopularity();
    }

    public float[] GetPopularitySaveData()
    {
        return shopPopularityValues.ToArray();
    }
}
