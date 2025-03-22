using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProductGO : MonoBehaviour, IPickableGO, IInformationDisplay
{
    public Product product;
    public IPickable pickable => product;

    public string InformationDisplayText => product.productType.Name;

    public bool isPhysixSpawned;

    public static ProductGO Spawn(bool isOnlyVisual, Vector3 position, Quaternion rotation, Transform parent, Product product)
    {        
        GameObject productGO;
        ProductGO productGOScript;
        if (isOnlyVisual) {
            productGO = SpawnVisual(product.productType);
            productGO.transform.SetParent(parent);
            productGO.transform.transform.position = position;
            productGO.transform.transform.rotation = rotation;  
        }
        else {
            productGO = Instantiate(product.productType.prefab, position, rotation, parent);
        }

        productGOScript = productGO.GetComponent<ProductGO>();
        productGOScript.product = product;
        productGOScript.isPhysixSpawned = !isOnlyVisual;
        return productGOScript;
    }

    public static GameObject SpawnVisual(ProductSO productType)
    {
        GameObject visual = GameObject.Instantiate(productType.prefab);
        GameObject.Destroy(visual.GetComponent<Rigidbody>());
        visual.GetComponentsInChildren<Collider>().ToList().ForEach((col) => { col.isTrigger = true; col.enabled = false; });
        return visual;
    }
}
