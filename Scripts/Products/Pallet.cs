using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pallet : MonoBehaviour
{
    public List<GameObject> spawnPositions;
    List<GameObject> spawnedObjects = new List<GameObject>();

    public void SpawnProducts(int productTypeIndex, int amount)
    {
        if(amount > spawnPositions.Count) {
            Debug.LogError("Za du¿o");
            return;
        }

        for(int i = 0; i < amount; i++){
            ProductsData.instance.products.Add(new Product(productTypeIndex, spawnPositions[i].transform.position, spawnPositions[i].transform.rotation));
            //spawnedObjects.Add(ProductGO.Spawn(false, productTypeIndex, spawnPositions[i].transform.position,
            //    spawnPositions[i].transform.rotation, null, );
        }
    }
}
