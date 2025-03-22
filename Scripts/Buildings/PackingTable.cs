using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PackingTable : Building
{

    public override void Build()
    {
        base.Build();
        ShopData.instance.AddPackingTable(this);
    }

    private void OnDestroy()
    {
        ShopData.instance.RemovePackingTable(this);
    }
}
