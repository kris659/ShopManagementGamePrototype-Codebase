using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriceManager : MonoBehaviour
{
    public static PriceManager instance;

    [SerializeField] private float[] productBaseWholesalePrices;
    [SerializeField] private float[] productBaseMarketPrices;

    [SerializeField] private float[] productSellingPrices;

    [SerializeField] private int[] wholesalePriceChanges;
    [SerializeField] private int[] marketPriceChanges;

    [SerializeField] private int[] previousWholesalePriceChanges;
    [SerializeField] private int[] previousMarketPriceChanges;

    [SerializeField] private int[] wholesaleLowestChanges;
    [SerializeField] private int[] marketLowestChanges;

    [SerializeField] private int[] wholesaleHighestChanges;
    [SerializeField] private int[] marketHighestChanges;


    public List<int> currentChangedPricesIndexes = new List<int>();
    public List<int> currentChangedMarketPricesValues = new List<int>();
    public List<int> currentChangedWholesalePricesValues = new List<int>();
    public List<int> currentChangedLowestHighest = new List<int>(); // -1 - new lowest, 1 - new highest 


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
        productBaseWholesalePrices = new float[SOData.productsList.Length];
        productBaseMarketPrices = new float[SOData.productsList.Length];
        productSellingPrices = new float[SOData.productsList.Length];
        marketPriceChanges = new int[SOData.productsList.Length];
        wholesalePriceChanges = new int[SOData.productsList.Length];
        previousMarketPriceChanges = new int[SOData.productsList.Length];
        previousWholesalePriceChanges = new int[SOData.productsList.Length];

        wholesaleLowestChanges = new int[SOData.productsList.Length];
        marketLowestChanges = new int[SOData.productsList.Length];
        wholesaleHighestChanges = new int[SOData.productsList.Length];
        marketHighestChanges = new int[SOData.productsList.Length];

        for (int i = 0; i < productBaseWholesalePrices.Length; i++) {
            productBaseWholesalePrices[i] = SOData.productsList[i].Price;
            productBaseMarketPrices[i] = SOData.productsList[i].Price * 2;
            marketPriceChanges[i] = Random.Range(-10, 10);
            wholesalePriceChanges[i] = marketPriceChanges[i];
            productSellingPrices[i] = GetMarketPrice(i);

            wholesaleLowestChanges[i] = wholesalePriceChanges[i];
            wholesaleHighestChanges[i] = wholesalePriceChanges[i];

            marketLowestChanges[i] = marketPriceChanges[i];
            marketHighestChanges[i] = marketPriceChanges[i];
        }
    }

    public void UpdatePrices()
    {
        for (int i = 0; i < marketPriceChanges.Length; i++) {
            previousMarketPriceChanges[i] = marketPriceChanges[i];
            previousWholesalePriceChanges[i] = wholesalePriceChanges[i];
        }

        //previousChangedPricesIndexes = new List<int>(currentChangedPricesIndexes);
        //previousChangedMarketPricesValues = new List<int>(currentChangedMarketPricesValues);
        //previousChangedWholesalePricesValues = new List<int>(currentChangedWholesalePricesValues);

        currentChangedPricesIndexes.Clear();
        currentChangedMarketPricesValues.Clear();
        currentChangedWholesalePricesValues.Clear(); 
        currentChangedLowestHighest.Clear();

        int changesCount = Random.Range(3, 6);

        while(changesCount-- > 0) {
            int index = ProductsData.instance.GetRandomProduct();
            int value = Mathf.RoundToInt(Mathf.Pow(2, Random.Range(2, 10.0f)) / Mathf.Pow(2, 10) * 25 + 5);
            
            if (currentChangedPricesIndexes.Contains(index)) {
                changesCount++;
                continue;
            }
            
            if(Random.Range(0, 100) < 50) {
                if (Random.Range(0, 100) < wholesalePriceChanges[index] + 50)
                    value *= -1;
                wholesalePriceChanges[index] += value;
                currentChangedPricesIndexes.Add(index);
                currentChangedMarketPricesValues.Add(value);
                currentChangedWholesalePricesValues.Add(0);
                if (wholesalePriceChanges[index] < wholesaleLowestChanges[index])
                    currentChangedLowestHighest.Add(-1);
                else {
                    if (wholesalePriceChanges[index] > wholesaleHighestChanges[index])
                        currentChangedLowestHighest.Add(1);
                    else currentChangedLowestHighest.Add(0);
                }

                wholesaleLowestChanges[index] = Mathf.Min(wholesaleLowestChanges[index], wholesalePriceChanges[index]);
                wholesaleHighestChanges[index] = Mathf.Max(wholesaleHighestChanges[index], wholesalePriceChanges[index]);
            }
            else {
                if (Random.Range(0, 100) < marketPriceChanges[index] + 50 - wholesalePriceChanges[index])
                    value *= -1;
                marketPriceChanges[index] += value;
                currentChangedPricesIndexes.Add(index);
                currentChangedMarketPricesValues.Add(0);
                currentChangedWholesalePricesValues.Add(value);

                if (marketPriceChanges[index] < marketLowestChanges[index])
                    currentChangedLowestHighest.Add(-1);
                else {
                    if (marketPriceChanges[index] > marketHighestChanges[index])
                        currentChangedLowestHighest.Add(1);
                    else currentChangedLowestHighest.Add(0);
                }

                marketLowestChanges[index] = Mathf.Min(marketLowestChanges[index], marketPriceChanges[index]);
                marketHighestChanges[index] = Mathf.Max(marketHighestChanges[index], marketPriceChanges[index]);
            }
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

    public float GetMarketPrice(ProductSO productType)
    {
        return GetMarketPrice(SOData.GetProductIndex(productType));
    }

    public float GetMarketPrice(int productTypeIndex)
    {
        return Mathf.Round(productBaseMarketPrices[productTypeIndex] * (100 + marketPriceChanges[productTypeIndex])) / 100;
    }

    public float GetWholesalePrice(ProductSO productType)
    {
        return GetWholesalePrice(SOData.GetProductIndex(productType));
    }

    public float GetWholesalePrice(int productTypeIndex)
    {
        return Mathf.Round(productBaseWholesalePrices[productTypeIndex] * (100 + wholesalePriceChanges[productTypeIndex])) / 100;
    }

    public float GetPreviousMarketPrice(int productTypeIndex)
    {
        return Mathf.Round(productBaseMarketPrices[productTypeIndex] * (100 + previousMarketPriceChanges[productTypeIndex])) / 100;
    }

    public float GetPreviousWholesalePrice(int productTypeIndex)
    {
        return Mathf.Round(productBaseWholesalePrices[productTypeIndex] * (100 + previousWholesalePriceChanges[productTypeIndex])) / 100;
    }

    public void SetProductSellPrice(int productTypeIndex, float price)
    {
        productSellingPrices[productTypeIndex] = price;
    }

    public float GetLowestMarketPrice(int productTypeIndex)
    {
        return Mathf.Round(productBaseMarketPrices[productTypeIndex] * (100 + marketLowestChanges[productTypeIndex])) / 100;
    }

    public float GetHighestMarketPrice(int productTypeIndex)
    {
        return Mathf.Round(productBaseMarketPrices[productTypeIndex] * (100 + marketHighestChanges[productTypeIndex])) / 100;
    }

    public float GetLowestWholesalePrice(int productTypeIndex)
    {
        return Mathf.Round(productBaseWholesalePrices[productTypeIndex] * (100 + wholesaleLowestChanges[productTypeIndex])) / 100;
    }

    public float GetHighestWholesalePrice(int productTypeIndex)
    {
        return Mathf.Round(productBaseWholesalePrices[productTypeIndex] * (100 + wholesaleHighestChanges[productTypeIndex])) / 100;
    }
    public PricesSaveData GetPricesSaveData()
    {
        return new PricesSaveData(productSellingPrices, wholesalePriceChanges, marketPriceChanges, wholesaleLowestChanges, marketLowestChanges,
            wholesaleHighestChanges, marketHighestChanges, currentChangedPricesIndexes.ToArray(), currentChangedMarketPricesValues.ToArray(),
            currentChangedWholesalePricesValues.ToArray(), currentChangedLowestHighest.ToArray());
    }

    public void LoadFromSaveData(PricesSaveData pricesSaveData)
    {
        productSellingPrices = new float[SOData.productsList.Length];


        currentChangedPricesIndexes = new List<int>(pricesSaveData.currentChangedPricesIndexes);
        currentChangedMarketPricesValues = new List<int>(pricesSaveData.currentChangedMarketPricesValues);
        currentChangedWholesalePricesValues = new List<int>(pricesSaveData.currentChangedWholesalePricesValues);
        currentChangedLowestHighest = new List<int>(pricesSaveData.currentChangedLowestHighest);

        int i = 0;
        for (; i < pricesSaveData.productSellingPrices.Length; i++) {
            productSellingPrices[i] = pricesSaveData.productSellingPrices[i];
            wholesalePriceChanges[i] = pricesSaveData.wholesalePriceChanges[i];
            marketPriceChanges[i] = pricesSaveData.marketPriceChanges[i];
            wholesaleLowestChanges[i] = pricesSaveData.wholesaleLowestChanges[i];
            marketLowestChanges[i] = pricesSaveData.marketLowestChanges[i];
            wholesaleHighestChanges[i] = pricesSaveData.wholesaleHighestChanges[i];
            marketHighestChanges[i] = pricesSaveData.marketHighestChanges[i];
        }
        for (; i < productSellingPrices.Length; i++) {
            marketPriceChanges[i] = Random.Range(-10, 10);
            wholesalePriceChanges[i] = marketPriceChanges[i];
            productSellingPrices[i] = GetMarketPrice(i);

            wholesaleLowestChanges[i] = wholesalePriceChanges[i];
            wholesaleHighestChanges[i] = wholesalePriceChanges[i];

            marketLowestChanges[i] = marketPriceChanges[i];
            marketHighestChanges[i] = marketPriceChanges[i];
        }
    }
}
