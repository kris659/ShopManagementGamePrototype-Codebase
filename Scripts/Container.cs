using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Container : IPickable
{
    public ContainerGO containerGO;
    public ContainerSO containerType;
    public int containerTypeIndex;

    public List<PlacingTriggerArea> placingTriggerAreas = new();

    private bool IsContainerOpen => containerGO != null && (containerGO.isOpen || containerGO.isOpeningOrClosing);
    public bool isContainerOnShelf;
    public bool isContainerOnStorageShelf;
    private List<Product> productsInContainer = new List<Product>();
    private List<Vector3> positionsInContainer = new List<Vector3>();
    private List<Vector3> rotationsInContainer = new List<Vector3>();

    public int HoldingLimit => 1;
    public int PickableTypeID => -containerTypeIndex - 1;
    public int PickableID => ProductsData.instance.containersSpawned.IndexOf(this);
    public BoxCollider BoxCollider => containerType.prefab.GetComponentInChildren<BoxCollider>();

    public GameObject PreviewGameObject => ContainerGO.SpawnVisual(containerType);
    public GameObject GameObject => containerGO.gameObject;
    public bool CanPickableInteract => containerGO != null;
    public string PickableInteractionText => GetPickableInteractionText();
    public List<IPickable> AdditionalPickables => GetAdditionalPickables();

    private List <IPickable> GetAdditionalPickables()
    {
        if(containerGO == null || !containerGO.isOpen)
            return new List<IPickable>();
        return productsInContainer.ConvertAll(x => (IPickable)x);
    }
    
    public void RemoveLastAdditionalPickable() { 
        int lastIndex = productsInContainer.Count - 1;
        productsInContainer.RemoveAt(lastIndex);
        positionsInContainer.RemoveAt(lastIndex);
        rotationsInContainer.RemoveAt(lastIndex);
    }

    private int vehicleAreasCount;
    public Container(int typeIndex, bool isPhysxSpawned, Vector3 position, Quaternion rotation, bool isOpen):
        this(typeIndex, isPhysxSpawned, position, rotation, isOpen, new List<Product>(), new List<Vector3>(), new List<Vector3>())
    {   }

    public Container(int typeIndex, bool isPhysxSpawned, Vector3 position, Quaternion rotation, bool isOpen,
        List<Product> productsInContainer, List<Vector3> positionsInContainer, List<Vector3> rotationsInContainer)        
    {
        ProductsData.instance.containersSpawned.Add(this);
        this.containerType = SOData.containersList[typeIndex];
        this.containerTypeIndex = typeIndex;
        this.productsInContainer = productsInContainer;
        this.positionsInContainer = positionsInContainer;
        this.rotationsInContainer = rotationsInContainer;

        if (isPhysxSpawned) {
            containerGO = ContainerGO.Spawn(false, position, rotation, null, this);
            containerGO.Init(isOpen);
        }
    }

    private string GetPickableInteractionText()
    {
        if(containerGO != null) {
            if (containerGO.isOpen)
                return "E - Close box";
            else
                return "E - Open box";
        }
        return string.Empty;
    }

    public void OnPickableInteract() {
        if (containerGO != null) {
            containerGO.OnPlayerButtonInteract();
            PlayerInteractions.Instance.playerPickup.UpdatePreview();
            PlayerInteractions.Instance.playerPickup.UpdatePossibleAction();
        }
    }


    public void Place(Vector3 position, Quaternion rotation, Transform parent, bool playSound = true)
    {
        DestroyGameObject();
        containerGO = ContainerGO.Spawn(false, position, rotation, parent, this);
        containerGO.Init(false);

        AudioManager.PlaySound(Sound.BoxDrop, position, containerGO.transform);
    }

    public void OnPlayerTake(bool shoudSpawnVisual, Transform parent)
    {
        HandleContainerTake();
        DestroyGameObject();
        if (shoudSpawnVisual) {
            SpawnVisual(parent);
        }
        AudioManager.PlaySound(Sound.BoxPickup, parent.position, containerGO.transform);
    }

    private void SpawnVisual(Transform parent)
    {
        containerGO = ContainerGO.Spawn(true, Vector3.zero, Quaternion.identity, parent, this);
        containerGO.transform.localPosition = containerType.offset;
        containerGO.transform.localRotation = Quaternion.identity;
        containerGO.transform.localScale = Vector3.one;
    }

    public void CreateProductsInContainer(int productTypeIndexToSpawn, List<Vector3> positions, List<Vector3> rotations)
    {
        for (int i = 0; i < positions.Count; i++) {
            Vector3 position = positions[i];// - new Vector3(BoxCollider.size.x / 2, 0, BoxCollider.size.z / 2);
            Vector3 rotation = rotations[i];
            productsInContainer.Add(new Product(productTypeIndexToSpawn));
            positionsInContainer.Add(position);
            rotationsInContainer.Add(rotation);
        }
        containerGO.Init(false);
    }

    public Transform GetTakingPosition()
    {
        PlacingTriggerArea selectedTriggerArea = null;
        float distance = float.MaxValue;
        foreach (PlacingTriggerArea triggerArea in placingTriggerAreas) {            
            if (triggerArea != null && triggerArea.IsOverridingInteractionPosition){
                if(Vector3.Distance(containerGO.transform.position, triggerArea.Transform.position) < distance) {
                    selectedTriggerArea = triggerArea;
                    distance = Vector3.Distance(containerGO.transform.position, triggerArea.Transform.position);
                }
            }
        }
        if (selectedTriggerArea != null)
            return selectedTriggerArea.InteractionPosition;
        return containerGO.transform;
    }

    public void RemoveProductFromContainer(Product product)
    {
        for(int i = 0; i < productsInContainer.Count; i++) {
            if (productsInContainer[i] == product) {
                productsInContainer.RemoveAt(i);
                positionsInContainer.RemoveAt(i);
                rotationsInContainer.RemoveAt(i);
                break;
            }
        }
    }

    public void DestroyGameObject()
    {
        vehicleAreasCount = 0;
        foreach (PlacingTriggerArea triggerArea in placingTriggerAreas) {
            triggerArea.OnContainerTakenFromArea(this);
        }
        placingTriggerAreas.Clear();
        if (containerGO != null) {
            GameObject.Destroy(containerGO.gameObject);
        }
    }

    private void HandleContainerTake()
    {
        if (IsContainerOpen) {
            UpdateProductsInContainer();
            for (int i = 0; i < productsInContainer.Count; i++) {
                productsInContainer[i].DestroyGameObject();                
            }
        }
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
        if(containerGO != null && containerGO.isPhysixSpawned) {
            containerGO.GetProductsInTrigger(out productsInContainer, out positionsInContainer, out rotationsInContainer);
        }
    }

    public void DestroyProductsInTrigger()
    {
        foreach (Product product in productsInContainer) {
            product.DestroyGameObject();
        }
    }

    public bool IsContainerEmpty()
    {
        if (IsContainerOpen) {
            UpdateProductsInContainer();
        }
        return productsInContainer.Count == 0;
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

    public void RemoveFromGame(bool shouldRemoveFromContainerList)
    {
        if (containerGO != null)
            GameObject.Destroy(containerGO.gameObject);
        for(int i = 0; i < productsInContainer.Count; i++) {
            productsInContainer[i].RemoveFromGame(true);
        }
        if (shouldRemoveFromContainerList && ProductsData.instance.containersSpawned.Contains(this))
            ProductsData.instance.containersSpawned.Remove(this);
    }

    public void OnVehicleAreaEnter()
    {
        if (vehicleAreasCount == 0 && containerGO != null && containerGO.transform.TryGetComponent(out Rigidbody rb)) {
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
        vehicleAreasCount++;
    }
    public void OnVehicleAreaExit()
    {
        vehicleAreasCount--;
        if (vehicleAreasCount == 0 && containerGO != null && containerGO.transform.TryGetComponent(out Rigidbody rb)) {
            rb.interpolation = RigidbodyInterpolation.None;
        }
    }
}
