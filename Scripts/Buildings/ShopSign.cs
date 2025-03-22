using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopSign : Decoration
{
    private TextDisplay textDisplay;
    private GameObject textCollider;

    private void Awake()
    {
        textDisplay = transform.GetComponentInChildren<TextDisplay>();
    }

    public override void Build()
    {
        ShopData.instance.onShopNameChanged += UpdateSign;
        UpdateSign(ShopData.instance.shopName);

        base.Build();       
    }

    public override void StartBuilding()
    {
        //textDisplay = transform.GetComponentInChildren<TextDisplay>();
        UpdateSign(ShopData.instance.shopName);
        
        base.StartBuilding();
    }

    private void UpdateSign(string shopName)
    {
        if (shopName == string.Empty)
            shopName = "Shop name";
        textDisplay.GenerateText(shopName);

        if(textCollider == null) {
            textCollider = new GameObject("TextCollider");
            textCollider.layer = LayerMask.NameToLayer("BuildingCheck");
            textCollider.transform.SetParent(buildingCollidersParent.transform);
            textCollider.transform.localPosition = Vector3.zero;
            textCollider.AddComponent<BoxCollider>();
        }
        BoxCollider collider = textCollider.GetComponent<BoxCollider>();
        collider.size = textDisplay.TextColliderSize;
        collider.center = textDisplay.TextColliderCenter;
        if (buildingCollisionVisual != null)
            buildingCollisionVisual.transform.GetChild(0).transform.localScale = textDisplay.TextColliderSize + Vector3.one * 0.05f;
    }

    private void OnDestroy()
    {
        ShopData.instance.onShopNameChanged -= UpdateSign;
    }
}
