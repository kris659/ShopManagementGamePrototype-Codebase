using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatsType {
    CustomersServed,
    CustomersServedPlayer,
    CustomersServedWorkers,

    ProductsBought,
    ProductsSold,
    BoxesBought,
    BoxesThrowedAway,
}


public class StatsManager : MonoBehaviour
{
    public static StatsManager instance;

    public int productsOnWarehouseShelves = 0;
    private void Awake()
    {
        if(instance != null) {
            Debug.LogError("Multiple stats managers");
            Destroy(this);
            return;
        }
        instance = this;
    }
}
