using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriceManager : MonoBehaviour
{
    public static PriceManager instance;

    [SerializeField] private float[] productMarketPrices;
    [SerializeField] private float[] productSellingPrices;

    private void Awake()
    {
        if(instance != null) {
            Debug.LogError("Multiple PriceManager");
            Destroy(gameObject);
        }
        instance = this;
    }

    private void Start()
    {
        CreateStartingPrices();
    }

    void CreateStartingPrices()
    {
        productMarketPrices = new float[SOData.productsList.Length];
        productSellingPrices = new float[SOData.productsList.Length];

        for (int i = 0; i < productMarketPrices.Length; i++) {
            productMarketPrices[i] = SOData.productsList[i].Price * 2;
            productSellingPrices[i] = SOData.productsList[i].Price * 2;
        }
    }

    public float GetProductSellPrice(ProductSO productType)
    {
        return GetProductSellPrice(SOData.GetProductIndex(productType));
    }
    public float GetProductSellPrice(int productTypeIndex)
    {
        return Mathf.Round(productSellingPrices[productTypeIndex] * 100) / 100;
    }

    public float GetProductMarketPrice(ProductSO productType)
    {
        return GetProductMarketPrice(SOData.GetProductIndex(productType));
    }

    public float GetProductMarketPrice(int productTypeIndex)
    {
        return Mathf.Round(productMarketPrices[productTypeIndex] * 100) / 100;
    }

    public void SetProductSellPrice(int productTypeIndex, float price)
    {
        productSellingPrices[productTypeIndex] = price;
    }
}
