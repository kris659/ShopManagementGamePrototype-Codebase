using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProductGO : MonoBehaviour, IPickableGO
{
    public Product product;
    public IPickable pickable => product;

    public bool isPhysixSpawned;

    public static ProductGO Spawn(bool isOnlyVisual, Vector3 position, Quaternion rotation, Transform parent, Product product, bool isColliderActive = false)
    {        
        GameObject productGO;
        ProductGO productGOScript;
        if (isOnlyVisual) {
            productGO = Instantiate(product.productType.visualPrefab, position, rotation, parent);
            if(isColliderActive) {
                productGO.GetComponent<Collider>().enabled = true;
            }     
        }
        else {
            productGO = Instantiate(product.productType.prefab, position, rotation, parent);
        }

        productGOScript = productGO.GetComponent<ProductGO>();
        productGOScript.product = product;
        productGOScript.isPhysixSpawned = !isOnlyVisual;
        return productGOScript;
    }
}
