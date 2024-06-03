using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class OrdersManager : MonoBehaviour
{
    public List<Transform> palletSpawnPositions;
    private List<Transform> boxSpawnPosition = new List<Transform>();
    public List<Transform> productsInBoxSpawnPositionsParent;
    public List<int> productTypesPositionParentIndex;
    public GameObject palletPrefab;
    public int boxTypeIndex;

    public List<int> discountThresholds;    
    public List<int> discountPercents;

    BoxCollider palletCheckCollider;
    private void Awake()
    {
        Transform boxPositionsParent = palletPrefab.transform.GetChild(1);
        for(int i = 0; i < boxPositionsParent.childCount; i++) {
            boxSpawnPosition.Add(boxPositionsParent.GetChild(i));
        }
        palletCheckCollider = palletPrefab.transform.GetChild(2).GetComponent<BoxCollider>();
        SceneLoader.OnUISceneLoaded += () => UIManager.ordersUI.Init(this);
    }

    public void SpawnProducts(Dictionary<ProductSO, int> orderDictionary)
    {
        GameObject pallet = null; 
        int boxPositionIndex = 0;
        float cost = 0;
        ProductSO[] keys = orderDictionary.Keys.ToArray();
        foreach (ProductSO productSO in keys) {            
            int productAmount = orderDictionary[productSO];
            int productTypeIndex = SOData.GetProductIndex(productSO);
            int parentIndex = productTypesPositionParentIndex[productTypeIndex];
            Transform parent = productsInBoxSpawnPositionsParent[parentIndex];

            while(productAmount > 0) {
                if (boxPositionIndex == 0) {
                    pallet = SpawnNewPallet();
                    if (pallet == null) {
                        UIManager.textUI.UpdateText("Not enough space for new pallet", 3f);
                        PlayerData.instance.TakeMoney(cost);
                        return;
                    }
                }
                Container container = new Container(boxTypeIndex, true, pallet.transform.TransformPoint(boxSpawnPosition[boxPositionIndex].localPosition), pallet.transform.rotation, false);

                int amountToSpawn = Mathf.Min(productAmount, parent.childCount);
                productAmount -= amountToSpawn;
                if (productAmount == 0)
                    orderDictionary.Remove(productSO);
                else
                    orderDictionary[productSO] = productAmount;
                cost += amountToSpawn * productSO.Price;
                container.CreateProductsInContainer(productTypeIndex, amountToSpawn, parent);
                boxPositionIndex++;
                if (boxPositionIndex == boxSpawnPosition.Count)
                    boxPositionIndex = 0;
            }            
        }
        PlayerData.instance.TakeMoney(cost);
        orderDictionary.Clear();
    }

    public bool IsEnoughSpace(ProductSO productType, int amount)
    {
        return true;
    }

    private GameObject SpawnNewPallet()
    {
        for(int i = 0;i < palletSpawnPositions.Count; i++){
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
                return Instantiate(palletPrefab, palletSpawnPositions[i].position, palletSpawnPositions[i].rotation);
            }
        }
        return null;
    }
}
