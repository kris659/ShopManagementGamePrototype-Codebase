using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestingProductsSpawning : MonoBehaviour
{
    public bool spawn;

    public List<ProductSO> productsToSpawn;
    public BoxCollider boxCollider;
    private List<Product> productsSpawned = new List<Product>();

    private void Update()
    {
        if (spawn) {
            spawn = false;
            Spawn();
        }
    }

    private void Spawn()
    {
        foreach(Product product in productsSpawned) {
            product.DestroyGameObject();
        }
        productsSpawned.Clear();
        ProductsData.instance.GetInTriggerPositions(productsToSpawn, boxCollider, out List<Vector3> positions, true);
        for(int i = 0; i < positions.Count; i++){
            int index = SOData.GetProductIndex(productsToSpawn[i]);
            Debug.Log(positions[i]);
            productsSpawned.Add(
                new Product(index, true, positions[i] + boxCollider.transform.position,
                Quaternion.identity));
        }
    }
}
