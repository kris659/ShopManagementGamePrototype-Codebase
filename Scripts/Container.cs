using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container : IPickable
{
    public ContainerGO containerGO;
    public ContainerSO containerType;
    public int containerTypeIndex;

    public List<IPlacingTriggerArea> placingTriggerAreas = new List<IPlacingTriggerArea>();

    private bool isContainerOpen;
    public bool isContainerOnShelf;
    private List<Product> productsInContainer = new List<Product>();
    private List<Vector3> positionsInContainer = new List<Vector3>();
    private List<Vector3> rotationsInContainer = new List<Vector3>();

    public int HoldingLimit => 1;
    public int PickableTypeID => -1;
    public int PickableID => ProductsData.instance.containersSpawned.IndexOf(this);
    public BoxCollider BoxCollider => containerType.prefab.GetComponentInChildren<BoxCollider>();

    public GameObject PreviewGameObject => GameObject.Instantiate(containerType.visualPrefab);
    public GameObject GameObject => containerGO.gameObject;

    public Container(int typeIndex, bool isPhysxSpawned, Vector3 position, Quaternion rotation, bool isOpen):
        this(typeIndex, isPhysxSpawned, position, rotation, isOpen, new List<Product>(), new List<Vector3>(), new List<Vector3>())
    {   }

    public Container(int typeIndex, bool isPhysxSpawned, Vector3 position, Quaternion rotation, bool isOpen,
        List<Product> productsInContainer, List<Vector3> positionsInContainer, List<Vector3> rotationsInContainer)        
    {
        ProductsData.instance.containersSpawned.Add(this);
        this.containerType = SOData.containersList[typeIndex];
        this.containerTypeIndex = typeIndex;
        this.isContainerOpen = isOpen;
        this.productsInContainer = productsInContainer;
        this.positionsInContainer = positionsInContainer;
        this.rotationsInContainer = rotationsInContainer;

        if (isPhysxSpawned) {
            containerGO = ContainerGO.Spawn(false, position, rotation, null, this);
            containerGO.Init(isOpen);
        }
    }

    public void Place(Vector3 position, Quaternion rotation, Transform parent)
    {
        DestroyGameObject();
        containerGO = ContainerGO.Spawn(false, position, rotation, parent, this);
        containerGO.Init(isContainerOpen);

        UpdatePlacingTriggerAreas(position, rotation);
    }

    public void OnPlayerTake(bool shoudSpawnVisual, Transform parent)
    {
        HandleContainerTake();
        DestroyGameObject();

        if (shoudSpawnVisual) {
            SpawnVisual(parent);
        }
    }

    private void UpdatePlacingTriggerAreas(Vector3 position, Quaternion rotation)
    {
        BoxCollider collider = containerType.prefab.GetComponentInChildren<BoxCollider>();
        Vector3 center = position + collider.center;
        Vector3 halfExtents = collider.size / 2.2f;
        Collider[] hitColliders = Physics.OverlapBox(center, halfExtents, rotation, ShopData.instance.shelfTriggerLayer);

        for (int i = 0; i < hitColliders.Length; i++) {
            IPlacingTriggerArea placingTriggerArea = hitColliders[i].GetComponentInParent<IPlacingTriggerArea>();
            if (placingTriggerArea != null && !placingTriggerAreas.Contains(placingTriggerArea)) {
                placingTriggerAreas.Add(placingTriggerArea);
            }
            placingTriggerArea = hitColliders[i].GetComponent<IPlacingTriggerArea>();
            if (placingTriggerArea != null && !placingTriggerAreas.Contains(placingTriggerArea)) {
                placingTriggerAreas.Add(placingTriggerArea);
            }
        }
    }

    private void SpawnVisual(Transform parent)
    {
        containerGO = ContainerGO.Spawn(true, Vector3.zero, Quaternion.identity, parent, this);
        containerGO.transform.localPosition = containerType.offset;
        containerGO.transform.localRotation = Quaternion.identity;
        containerGO.transform.localScale = Vector3.one;
    }

    public void CreateProductsInContainer(int productTypeIndexToSpawn, int productAmount, Transform positionsParent)
    {
        for (int i = 0; i < productAmount; i++) {
            Vector3 position = positionsParent.GetChild(i).localPosition;
            Vector3 rotation = positionsParent.GetChild(i).localEulerAngles;
            productsInContainer.Add(new Product(productTypeIndexToSpawn));
            positionsInContainer.Add(position);
            rotationsInContainer.Add(rotation);
        }
        containerGO.Init(false);
    }

    public void DestroyGameObject()
    {
        foreach (IPlacingTriggerArea triggerArea in placingTriggerAreas) {
            triggerArea.OnContainerTakenFromArea(this);
        }
        placingTriggerAreas.Clear();
        if (containerGO != null) {
            GameObject.Destroy(containerGO.gameObject);
        }
    }

    private void HandleContainerTake()
    {
        isContainerOpen = (containerGO.isOpen || containerGO.isOpeningOrClosing);
        if (isContainerOpen) {
            UpdateProductsInContainer();
            for (int i = 0; i < productsInContainer.Count; i++) {
                productsInContainer[i].DestroyGameObject();                
            }
        }
        isContainerOpen = false;
    }

    public void GetProductsInContainerData(out List<Product> productsInTriggerList, out List<Vector3> productsInTriggerPositionsList, out List<Vector3> productsInTriggerRotationsList)
    {
        productsInTriggerList = this.productsInContainer;
        productsInTriggerPositionsList = this.positionsInContainer;
        productsInTriggerRotationsList = this.rotationsInContainer;
    }

    public void ClearProductsInContainer()
    {
        productsInContainer.Clear();
        positionsInContainer.Clear();
        rotationsInContainer.Clear();
    }

    public void UpdateProductsInContainer()
    {
        if(containerGO != null) {
            containerGO.GetProductsInTrigger(out productsInContainer, out positionsInContainer, out rotationsInContainer);
        }
    }

    public void DestroyProductsInTrigger()
    {
        foreach (Product product in productsInContainer) {
            product.DestroyGameObject();
        }
    }

    public ContainerSaveData CreateSaveData()
    {
        Vector3 position = Vector3.zero;
        Quaternion rotation = Quaternion.identity;
        
        bool isPhysxSpawned = (containerGO != null && containerGO.isPhysixSpawned);
        bool isOpen = (containerGO != null && containerGO.isOpen && !containerGO.isOpeningOrClosing);

        if(isOpen){
            UpdateProductsInContainer();
        }

        int[] productsInContainerIndexes = new int[productsInContainer.Count];
        Quaternion[] productsInContainerRotations = new Quaternion[productsInContainer.Count];

        if(containerGO != null) {
            position = containerGO.transform.position;
            rotation = containerGO.transform.rotation;
        }

        for(int i = 0; i < productsInContainer.Count; i++) {
            productsInContainerIndexes[i] = ProductsData.instance.productsSpawned.IndexOf(productsInContainer[i]);
            productsInContainerRotations[i] = Quaternion.Euler(rotationsInContainer[i]);
        }
        return new ContainerSaveData(containerTypeIndex, isPhysxSpawned, isOpen, position, rotation, productsInContainerIndexes, positionsInContainer.ToArray(), productsInContainerRotations);
    }

    public void RemoveProductFromGame(bool shouldRemoveFromContainerList)
    {
        if (containerGO != null)
            GameObject.Destroy(containerGO.gameObject);
        if (shouldRemoveFromContainerList && ProductsData.instance.containersSpawned.Contains(this))
            ProductsData.instance.containersSpawned.Remove(this);
    }

    public void SpawnVisualSavePickup(Transform parent)
    {
        SpawnVisual(parent);
    }
}
