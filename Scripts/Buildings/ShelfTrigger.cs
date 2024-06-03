using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShelfTrigger : TriggerHandler, IPlacingTriggerArea
{
    public BoxCollider trigger;
    public List<Product> productsInArea = new List<Product>();
    public List<Container> containersInArea = new List<Container>();
    public Shelf shelf;

    public void Init(Shelf shelf)
    {
        this.shelf = shelf;
        triggerEnter += OnShelfTriggerEnter;
        triggerExit += OnShelfTriggerExit;
        trigger = GetComponent<BoxCollider>();
    }

    public void OnProductPlacedInArea(Product product)
    {
        if (!productsInArea.Contains(product)) {
            //Debug.Log("Product placed at shelf trigger");
            productsInArea.Add(product);
        }
        shelf.OnProductPlacedInArea(product);
    }

    public void OnProductTakenFromArea(Product product)
    {
        if (productsInArea.Contains(product)) {
            //Debug.Log("Product taken from shelf trigger");
            productsInArea.Remove(product);
        }
        shelf.OnProductTakenFromArea(product);

    }

    public void OnContainerPlacedInArea(Container container)
    {
        if (!containersInArea.Contains(container)) {
            //Debug.Log("Container placed at shelf trigger");
            containersInArea.Add(container);
        }
        shelf.OnContainerPlacedInArea(container);

    }

    public void OnContainerTakenFromArea(Container container)
    {
        if (containersInArea.Contains(container)) {
            //Debug.Log("Container taken from shelf trigger");
            containersInArea.Remove(container);
        }
        shelf.OnContainerTakenFromArea(container);
    }
    private void OnShelfTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out ProductGO productGO)) {
            if (!productGO.product.placingTriggerAreas.Contains(this))
                productGO.product.placingTriggerAreas.Add(this);
            OnProductPlacedInArea(productGO.product);
        }
        if (other.gameObject.name != "ContainerTrigger")
            return;
        if (other.transform.parent != null && other.transform.parent.TryGetComponent(out ContainerGO containerGO)) {
            if (!containerGO.container.placingTriggerAreas.Contains(this))
                containerGO.container.placingTriggerAreas.Add(this);
            OnContainerPlacedInArea(containerGO.container);
        }
    }
    private void OnShelfTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out ProductGO productGO)) {
            if (productGO.product.placingTriggerAreas.Contains(this))
                productGO.product.placingTriggerAreas.Remove(this);
            else {
                Debug.LogError("Sus");
            }
            OnProductTakenFromArea(productGO.product);
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
}
