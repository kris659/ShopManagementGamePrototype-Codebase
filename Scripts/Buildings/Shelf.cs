using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shelf : MonoBehaviour, IBuildable, IPlacingTriggerArea
{
    [SerializeReference]
    public List<Product> products = new List<Product>();
    public int shelfTypeIndex;

    public int shelfIndex;
    public Transform customerDestination;

    public ShelfTrigger[] shelfTriggers;

    public static void Spawn(int shelfTypeIndex, Vector3 position, Quaternion rotation)
    {        
        GameObject shelfGO = Instantiate(SOData.shelvesList[shelfTypeIndex].Prefab, position, rotation);
        Shelf shelf = shelfGO.GetComponent<Shelf>();
        shelf.shelfIndex = ShopData.instance.AddShelf(shelf);
        shelf.shelfTypeIndex = shelfTypeIndex;
        shelf.Init();
    }

    private void Init()
    {
        shelfTriggers = transform.GetComponentsInChildren<ShelfTrigger>();
        foreach (ShelfTrigger shelfTrigger in shelfTriggers) {
            shelfTrigger.Init(this);
        }
    }
    public void OnProductPlacedInArea(Product product)
    {
        if (!products.Contains(product)) {
            Debug.Log("Placed product on shelf");
            products.Add(product);
            ShopData.instance.UpdateShelfStatus(this);
        }
    }

    public void OnProductTakenFromArea(Product product)
    {
        if (products.Contains(product)) {
            products.Remove(product);
            ShopData.instance.UpdateShelfStatus(this);
        }
    }

    public void OnContainerPlacedInArea(Container container) {
        container.isContainerOnShelf = true;   
    }
    public void OnContainerTakenFromArea(Container container) {
        container.isContainerOnShelf = false;
    }

    public Product GetRandomProduct()
    {
        if(products.Count > 0) {
            int index = Random.Range(0, products.Count);
            Product product = products[index];
            return product;
        }            
        return null;
    }

    public bool CanBuildHere(Vector3 position, Quaternion rotation)
    {
        return true;
    }

    public void Build(int typeIndex, Vector3 position, Quaternion rotation)
    {
        Spawn(typeIndex, position, rotation);
    }
    public void Destroy()
    {
        ShopData.instance.RemoveShelf(this);
        products.Clear();
        Destroy(gameObject);
    }
}
