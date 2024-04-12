using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    bool isUIOpen = false;
    BoxCollider palletCheckCollider;
    private void Awake()
    {
        Transform boxPositionsParent = palletPrefab.transform.GetChild(1);
        for(int i = 0; i < boxPositionsParent.childCount; i++) {
            boxSpawnPosition.Add(boxPositionsParent.GetChild(i));
        }
        palletCheckCollider = palletPrefab.transform.GetChild(2).GetComponent<BoxCollider>();
    }
    private void Start()
    {
        UIManager.ordersUI.Init(this);
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.O)) {
            if (!UIManager.ordersUI.isOpen)
                UIManager.ordersUI.OpenUI();
            else
                UIManager.ordersUI.CloseUI();
        }
    }

    public void SpawnProducts(int productTypeIndex, int amountTotal)
    {
        GameObject pallet = null;// = Instantiate(palletPrefab, palletSpawnPositions[i].position, Quaternion.identity);
        int parentIndex = productTypesPositionParentIndex[productTypeIndex];
        Transform parent = productsInBoxSpawnPositionsParent[parentIndex];

        for (int i = 0; i < boxSpawnPosition.Count && amountTotal > 0; i++) { 
            if(i%boxSpawnPosition.Count == 0){
                pallet = SpawnNewPallet();
                if (pallet == null) {
                    Debug.LogError("Nie ma miejsca na palete");
                    return;
                }
            }
            
            Product boxProduct = new Product(boxTypeIndex, pallet.transform.TransformPoint(boxSpawnPosition[i].localPosition), pallet.transform.rotation);
            ProductsData.instance.products.Add(boxProduct);
            
            int amountToSpawn = Mathf.Min(amountTotal, parent.childCount);
            amountTotal -= amountToSpawn;

            boxProduct.InitContainer(productTypeIndex, amountToSpawn, parent);
        }
       
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
            Debug.Log(hitColliders.Length);
            bool shouldContinue = false;
            foreach (Collider collider in hitColliders){
                if(collider.transform.GetComponentInParent<Pallet>() == null) {
                    shouldContinue = true; 
                    break;
                }
            }
            if (!shouldContinue) {
                for(int j = 0; j < hitColliders.Length; j++) {
                    Destroy(hitColliders[i].gameObject);
                }
                return Instantiate(palletPrefab, palletSpawnPositions[i].position, palletSpawnPositions[i].rotation);
            }
        }
        return null;
    }
}
