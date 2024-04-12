using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProductGO : MonoBehaviour
{
    public Product product;
    public Container container;
    public static ProductGO Spawn(bool isOnlyVisual, Vector3 position, Quaternion rotation, Transform parent, Product product, bool isColliderActive = false)
    {        
        GameObject productGO;
        if (isOnlyVisual) {
            productGO = Instantiate(product.productType.visualPrefab, position, rotation, parent);
            if(isColliderActive) {
                productGO.GetComponent<Collider>().enabled = true;
            }
            return productGO.GetComponent<ProductGO>();
        }
        else productGO = Instantiate(product.productType.prefab, position, rotation, parent);
        ProductGO productGOScript = productGO.GetComponent<ProductGO>();
        productGOScript.product = product;
        if (product.productType.isContainer) {
            productGOScript.container = productGO.GetComponent<Container>();
            productGOScript.container.Init(false, new List<Product>(), new List<Vector3>(), new List<Vector3>());
        }
        return productGOScript;
    }

    public void GetProductsInContainerData(out List<Product> productsInContainer, out List<Vector3> productsPositions, out List<Vector3> productsRotations)
    {
        if (!product.productType.isContainer) {
            productsInContainer = new List<Product>();
            productsPositions = new List<Vector3>();
            productsRotations = new List<Vector3>();
            return;
        }            
        container.GetProductsInContainerData(out productsInContainer, out productsPositions, out productsRotations);
    }
}
