using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OrdersManager : MonoBehaviour
{
    public List<Transform> palletSpawnPositions;
    private List<Transform> boxSpawnPosition = new List<Transform>();
    [SerializeField] List<ProductSO> customProduct;
    [SerializeField] List<Transform> customProductSpawningPositions;
    [SerializeField] private GameObject palletPrefab;

    private BoxCollider palletSpawnTrigger;

    public List<int> discountThresholds;    
    public List<int> discountPercents;

    public List<int> productsInBoxCount;
    public List<List<Vector3>> productsInBoxPositions;
    public List<List<Vector3>> productsInBoxRotations;

    BoxCollider palletCheckCollider;
    private void Awake()
    {
        Transform boxPositionsParent = palletPrefab.transform.GetChild(1);
        for(int i = 0; i < boxPositionsParent.childCount; i++) {
            boxSpawnPosition.Add(boxPositionsParent.GetChild(i));
        }
        palletCheckCollider = palletPrefab.transform.GetChild(2).GetComponent<BoxCollider>();
        SceneLoader.OnUISceneLoaded += () => UIManager.ordersUI.Init(this);
        palletSpawnTrigger = palletPrefab.GetComponent<Pallet>().spawnTrigger;
    }

    private void Start()
    {
        SetupProductsInBoxesCount();
    }

    public void SpawnProducts(Dictionary<ProductSO, int> orderDictionary)
    {
        GameObject pallet = null; 
        float cost = 0;
        ProductSO[] keys = orderDictionary.Keys.ToArray();
        Dictionary<ProductSO, float> boxPrices = new Dictionary<ProductSO, float>();
        foreach (ProductSO productSO in keys) {
            boxPrices[productSO] = (PriceManager.instance.GetWholesalePrice(productSO) * GetProductBoxCapacity(productSO) * (100 - GetDiscount(orderDictionary[productSO])) / 100);
        }

        while (keys.Length > 0) {
            pallet = SpawnNewPallet();
            if (pallet == null) {
                UIManager.textUI.UpdateText("Not enough space for new pallet", 3f);
                break;
            }

            List<BoxCollider> containerColliders = new List<BoxCollider>();
            List<ProductSO> productSOs = new List<ProductSO>();
            foreach (ProductSO productSO in keys) {
                int boxAmount = orderDictionary[productSO];
                BoxCollider containerCollider = productSO.containerType.prefab.GetComponentInChildren<BoxCollider>();
                containerColliders.AddRange(Enumerable.Repeat(containerCollider, boxAmount));
                productSOs.AddRange(Enumerable.Repeat(productSO, boxAmount));
            }
            
            List<ContainerPacking.PackingResult> packingResult = ContainerPacking.Pack(palletSpawnTrigger, containerColliders);
            int spawnedCount = 0;
            for (int i = 0; i < packingResult.Count; i++) {
                if (!packingResult[i].isPacked)
                    continue;
                spawnedCount++;
                int containerType = SOData.GetContainerIndex(productSOs[i].containerType);
                //Debug.Log(packingResult[i].position);
                Vector3 offset = new Vector3(-palletSpawnTrigger.size.x / 2, palletSpawnTrigger.center.y - palletSpawnTrigger.size.y / 2, -palletSpawnTrigger.size.z / 2);
                Vector3 position = pallet.transform.position + pallet.transform.TransformVector(packingResult[i].position + offset);

                Quaternion rotation = Quaternion.Euler(pallet.transform.eulerAngles + packingResult[i].rotation.eulerAngles);
                Container container = new Container(containerType, true, position, rotation, false);

                int productTypeIndex = SOData.GetProductIndex(productSOs[i]);
                container.CreateProductsInContainer(productTypeIndex, productsInBoxPositions[productTypeIndex], productsInBoxRotations[productTypeIndex]);
                //Destroy(container.containerGO.transform.GetComponent<Rigidbody>());

                orderDictionary[productSOs[i]]--;
                if(orderDictionary[productSOs[i]] == 0)
                    orderDictionary.Remove(productSOs[i]);
                cost += boxPrices[productSOs[i]];
            }

            keys = orderDictionary.Keys.ToArray();

            if(spawnedCount == 0) {
                Debug.LogError("Box is to big to spawn");
                break;
            }
        }


        PlayerData.instance.TakeMoney(cost);
    }

    public int GetProductBoxCapacity(ProductSO productSO)
    {
        int productTypeIndex = SOData.GetProductIndex(productSO);
        return productsInBoxCount[productTypeIndex];
    }

    private GameObject SpawnNewPallet()
    {
        for(int i = 0; i < palletSpawnPositions.Count; i++){
            Vector3 center = palletSpawnPositions[i].position + palletCheckCollider.center + palletCheckCollider.transform.localPosition;
            Vector3 halfExtents = palletCheckCollider.size / 2f;
            Collider[] hitColliders = Physics.OverlapBox(center, halfExtents, transform.rotation);
            //Debug.Log(hitColliders.Length);
            bool shouldContinue = false;
            List<GameObject> palletGOList = new List<GameObject>();
            foreach (Collider collider in hitColliders){
                Pallet pallet = collider.transform.GetComponentInParent<Pallet>();
                if (pallet == null) {
                    shouldContinue = true; 
                    break;
                }
                if(!palletGOList.Contains(pallet.gameObject))
                    palletGOList.Add(pallet.gameObject);
            }
            if (!shouldContinue) {
                for(int j = 0; j < palletGOList.Count; j++) {
                    Destroy(palletGOList[j]);
                }
                return BuildingManager.instance.SpawnProp(0, palletSpawnPositions[i].position, palletSpawnPositions[i].rotation);
            }
        }
        return null;
    }

    public int GetDiscount(int boxesAmount)
    {
        int index = 0;
        while (index < discountThresholds.Count && boxesAmount >= discountThresholds[index]) { index++; }
        return discountPercents[--index];
    }

    private void SetupProductsInBoxesCount()
    {
        productsInBoxCount = new List<int>();
        productsInBoxPositions = new List<List<Vector3>>();
        productsInBoxRotations = new List<List<Vector3>>();
        for (int i = 0; i < SOData.productsList.Length; i++) {
            List<Vector3> positions = new List<Vector3>();
            List<Vector3> rotations = new List<Vector3>();

            if (customProduct.Contains(SOData.productsList[i])) {
                int index = customProduct.IndexOf(SOData.productsList[i]);
                int count = customProductSpawningPositions[index].childCount;
                for (int j = 0; j < count; j++) {
                    BoxCollider boxCollider = SOData.productsList[i].containerType.prefab.GetComponentInChildren<BoxCollider>();
                    Vector3 colliderSize = new Vector3(boxCollider.size.x * boxCollider.transform.localScale.x, boxCollider.size.y * boxCollider.transform.localScale.y, boxCollider.size.z * boxCollider.transform.localScale.z);

                    Vector3 offset = new Vector3(colliderSize.x / 2, 0, colliderSize.z / 2);
                    positions.Add(customProductSpawningPositions[index].GetChild(j).transform.localPosition + offset);
                    rotations.Add(customProductSpawningPositions[index].GetChild(j).transform.localEulerAngles);                    
                }

                productsInBoxCount.Add(positions.Count);
                productsInBoxPositions.Add(positions);
                productsInBoxRotations.Add(rotations);
                continue;
            }

            BoxCollider containerCollider = SOData.productsList[i].containerType.prefab.GetComponent<ContainerGO>().containerTrigger;

            ProductsData.instance.GetInTriggerPositions(SOData.productsList[i], containerCollider, out positions, true);
            if (positions.Count > 16) {
                positions.Clear();
                ProductsData.instance.GetInTriggerPositions(SOData.productsList[i], containerCollider, out positions, true, 0.02f);
            }            
            rotations = Enumerable.Repeat(Vector3.zero, positions.Count).ToList();
            
            productsInBoxCount.Add(positions.Count);
            productsInBoxPositions.Add(positions);
            productsInBoxRotations.Add(rotations);
            
        }
    }
}
