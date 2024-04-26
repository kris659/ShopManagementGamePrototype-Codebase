using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shelf : MonoBehaviour, IBuildable
{
    public List<Product> products = new List<Product>();
    public int shelfTypeIndex;

    public int shelfIndex;
    public Transform customerDestination;

    private TriggerHandler triggerHandler;

    public static void Spawn(int shelfTypeIndex, Vector3 position, Quaternion rotation)
    {        
        GameObject shelfGO = Instantiate(SOData.shelvesList[shelfTypeIndex].Prefab, position, rotation);
        Shelf shelf = shelfGO.GetComponent<Shelf>();
        shelf.shelfIndex = ShopData.instance.AddShelf(shelf);
        ShopData.instance.UpdateShelfStatus(shelf);
        shelf.shelfTypeIndex = shelfTypeIndex;
        shelf.Init();
    }

    private void Init()
    {
        triggerHandler = transform.GetComponentInChildren<TriggerHandler>();
        triggerHandler.triggerEnter = OnShelfTriggerEnter;
        triggerHandler.triggerExit = OnShelfTriggerExit;
    }
    public void AddProduct(Product product)
    {
        if (!products.Contains(product)) {
            products.Add(product);
            ShopData.instance.UpdateShelfStatus(this);
        }
    }

    public void TakeProduct(Product product)
    {
        if (products.Contains(product)) {
            products.Remove(product);
            ShopData.instance.UpdateShelfStatus(this);
        }
    }
    public Product TakeRandomProduct()
    {
        if(products.Count > 0) {
            int index = Random.Range(0, products.Count);
            Product product = products[index];
            product.DestroyGameObject();
            ShopData.instance.UpdateShelfStatus(this);
            return product;
        }            
        return null;
    }

    private void OnShelfTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out ProductGO productGO)) {
            productGO.product.shelf = this;
            AddProduct(productGO.product);
        }
    }
    private void OnShelfTriggerExit(Collider other)
    {
        if(other.TryGetComponent(out ProductGO productGO)){
            productGO.product.shelf = null;
            TakeProduct(productGO.product);
        }
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
        ShopData.instance.UpdateShelfStatus(this);
        products.Clear();
        Destroy(gameObject);
    }
}
