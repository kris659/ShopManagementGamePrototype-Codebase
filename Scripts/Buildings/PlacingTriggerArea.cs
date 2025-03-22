using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlacingTriggerArea : TriggerHandler
{
    public delegate void ProductTriggerEvent (Product product);

    [HideInInspector] public BoxCollider trigger;
    public List<Product> productsInArea = new List<Product>();
    public List<Container> containersInArea = new List<Container>();
    public List<FurnitureBox> furnitureBoxesInArea = new List<FurnitureBox>();
    private PlacingTriggerAreaParent triggerParent;
    public Transform InteractionPosition => triggerParent.InteractionPosition;
    public Transform Transform => transform;
    public bool IsOverridingInteractionPosition => triggerParent.IsOverridingInteractionPosition;

    [HideInInspector]
    public ProductSO currentProduct;
    internal int currentProductCapacity;
    internal List<Vector3> currentProductPlacingPositions;
    internal List<Quaternion> currentProductPlacingRotations;

    public void Init(PlacingTriggerAreaParent triggerParent)
    {
        this.triggerParent = triggerParent;
        triggerEnter += OnPlacingTriggerEnter;
        triggerExit += OnPlacingTriggerExit;
        trigger = GetComponent<BoxCollider>();
    }
    public void OnProductPlacedInArea(Product product)
    {
        if (!productsInArea.Contains(product)) {
            //Debug.Log("Product placed at shelf trigger");
            if(currentProduct != product.productType) {
                currentProduct = product.productType;
                UpdateCurrentProduct();
            }
            productsInArea.Add(product);
            product.placingTriggerAreas.Add(this);
            triggerParent.OnProductPlacedInArea(product);
        }
    }
    public void OnProductTakenFromArea(Product product)
    {
        if (productsInArea.Contains(product)) {
            //Debug.Log("Product taken from shelf trigger");
            productsInArea.Remove(product);
            triggerParent.OnProductTakenFromArea(product);
        }
    }
    public void OnContainerPlacedInArea(Container container)
    {
        if (!containersInArea.Contains(container)) {
            //Debug.Log("Container placed at shelf trigger");
            containersInArea.Add(container);
            container.placingTriggerAreas.Add(this);
            container.GetProductsInContainerData(out List<Product> productsInContainer, out _, out _);
            if(productsInContainer.Count > 0) {
                currentProduct = productsInContainer[0].productType;
            }
            triggerParent.OnContainerPlacedInArea(container);
        }
    }
    public void OnContainerTakenFromArea(Container container)
    {
        if (containersInArea.Contains(container)) {
            //Debug.Log("Container taken from shelf trigger");
            containersInArea.Remove(container);            
            triggerParent.OnContainerTakenFromArea(container);
        }
    }
    public void OnFurnitureBoxPlacedInArea(FurnitureBox furnitureBox)
    {
        if (!furnitureBoxesInArea.Contains(furnitureBox)) {
            furnitureBoxesInArea.Add(furnitureBox);
            furnitureBox.placingTriggerAreas.Add(this);
            triggerParent.OnFurnitureBoxPlacedInArea(furnitureBox);
        }
    }
    public void OnFurnitureBoxTakenFromArea(FurnitureBox furnitureBox)
    {
        if (furnitureBoxesInArea.Contains(furnitureBox)) {
            furnitureBoxesInArea.Remove(furnitureBox);
            triggerParent.OnFurnitureBoxTakenFromArea(furnitureBox);
        }
    }
    private void OnPlacingTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out ProductGO productGO)) {            
            OnProductPlacedInArea(productGO.product);
        }
        if (other.TryGetComponent(out FurnitureBoxGO furnitureBoxGO)) {
            OnFurnitureBoxPlacedInArea(furnitureBoxGO.furnitureBox);
        }
        if (other.gameObject.name != "ContainerTrigger")
            return;
        if (other.transform.parent != null && other.transform.parent.TryGetComponent(out ContainerGO containerGO)) {
            OnContainerPlacedInArea(containerGO.container);
        }

    }
    private void OnPlacingTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out ProductGO productGO)) {
            if (productGO.product.placingTriggerAreas.Contains(this))
                productGO.product.placingTriggerAreas.Remove(this);
            else {
                Debug.LogError("Sus");
            }
            OnProductTakenFromArea(productGO.product);
        }
        if (other.TryGetComponent(out FurnitureBoxGO furnitureBoxGO)) {
            if (furnitureBoxGO.furnitureBox.placingTriggerAreas.Contains(this))
                furnitureBoxGO.furnitureBox.placingTriggerAreas.Remove(this);
            else {
                Debug.LogError("Sus");
            }
            OnFurnitureBoxTakenFromArea(furnitureBoxGO.furnitureBox);
        }

        if (other.gameObject.name != "ContainerTrigger")
            return;
        if (other.transform.parent != null && other.transform.parent.TryGetComponent(out ContainerGO containerGO)) {
            if (containerGO.container.placingTriggerAreas.Contains(this))
                containerGO.container.placingTriggerAreas.Remove(this);
            else {
                Debug.LogError("Sus");
            }
            OnContainerTakenFromArea(containerGO.container);
        }        
    }

    public int GetPercentageUsed()
    {
        if(currentProductCapacity != 0)
        {
            return Mathf.RoundToInt(100 * productsInArea.Count / currentProductCapacity);
        }
        return 100;
    }

    public virtual void UpdateCurrentProduct()
    {
        ProductsData.instance.GetInTriggerPositions(currentProduct, trigger, out currentProductPlacingPositions, false, 0.003f, true);
        for (int i = 0; i < currentProductPlacingPositions.Count; i++) {
            currentProductPlacingPositions[i] = trigger.transform.position + transform.rotation * (currentProductPlacingPositions[i] + trigger.center - new Vector3(0, trigger.size.y / 2, 0));
        }
        currentProductPlacingRotations = Enumerable.Repeat(transform.rotation, currentProductPlacingPositions.Count).ToList();
        currentProductCapacity = currentProductPlacingPositions.Count;
    }

    public void GetProductsPlacingPositions(out List<Vector3> positions, out List<Quaternion> rotations)
    {
        positions = currentProductPlacingPositions;
        rotations = currentProductPlacingRotations; 
    }

    private List<ContainerPacking.PackingResult> PackContainer(Container container)
    {
        List<BoxCollider> containerColliders = new List<BoxCollider>();
        for (int i = 0; i < containersInArea.Count; i++) {
            containerColliders.Add(containersInArea[i].BoxCollider);
        }
        containerColliders.Add(container.BoxCollider);
        return ContainerPacking.Pack(trigger, containerColliders);
    }

    public bool CanRestockerPlaceContainer(Container container)
    {
        List<ContainerPacking.PackingResult> result = PackContainer(container);
        for(int i = 0; i < result.Count; i++) {
            if (!result[i].isPacked)
                return false;
        }
        return true;
    }

    public void PlaceRestockerContainer(Container container)
    {
        List<ContainerPacking.PackingResult> result = PackContainer(container);
        //Debug.Log("Containers in area: " + containersInArea.Count + " result count: " + result.Count);
        List<Container> containers = new List<Container>(containersInArea);
        for (int i = 0; i < result.Count; i++) {
            if (!result[i].isPacked) {
                Debug.LogError("Container is not packed!");
                continue;
            }
            Vector3 position = trigger.transform.position + transform.rotation * (result[i].position + trigger.center - trigger.size / 2);
            Quaternion rotation = Quaternion.Euler(result[i].rotation.eulerAngles + trigger.transform.eulerAngles);
            if (i < result.Count - 1) {
                containers[i].Place(position, rotation, null, false);
            }
            else
                container.Place(position, rotation, null, true);
        }
    }

}
