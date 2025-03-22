using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacingTriggerAreaParent : MonoBehaviour
{
    public delegate void ProductTriggerEvent (Product product);
    public delegate void ContainerTriggerEvent (Container container);
    public delegate void FurnitureBoxTriggerEvent(FurnitureBox furnitureBox);

    public bool IsOverridingInteractionPosition;
    public Transform InteractionPosition; 

    [HideInInspector]
    public PlacingTriggerArea[] placingTriggers;

    public List<Product> products = new();
    public List<Container> containers = new();
    public List<FurnitureBox> furnitureBoxes = new();

    public ProductTriggerEvent OnProductTriggerEnterEvent;
    public ProductTriggerEvent OnProductTriggerExitEvent;
    public ContainerTriggerEvent OnContainerTriggerEnterEvent;
    public ContainerTriggerEvent OnContainerTriggerExitEvent;
    public FurnitureBoxTriggerEvent OnFurnitureBoxTriggerEnterEvent;
    public FurnitureBoxTriggerEvent OnFurnitureBoxTriggerExitEvent;

    private void Awake()
    {
        placingTriggers = transform.GetComponentsInChildren<PlacingTriggerArea>();
        foreach (PlacingTriggerArea shelfTrigger in placingTriggers) {
            shelfTrigger.Init(this);
        }
    }

    public void OnProductPlacedInArea(Product product)
    {
        if (!products.Contains(product)) {
            //Debug.Log("Placed product on shelf");
            products.Add(product);
            OnProductTriggerEnterEvent?.Invoke(product);
        }
    }

    public void OnProductTakenFromArea(Product product)
    {
        if (products.Contains(product)) {
            //Debug.Log("Placed taken from shelf");
            foreach (PlacingTriggerArea triggerArea in placingTriggers) {
                if(triggerArea.productsInArea.Contains(product)) {
                    return;
                }
            }
            products.Remove(product);
            OnProductTriggerExitEvent?.Invoke(product);
        }
    }

    public void OnContainerPlacedInArea(Container container)
    {
        if (!containers.Contains(container)) {
            //Debug.Log("Container product on shelf");
            containers.Add(container);
            OnContainerTriggerEnterEvent?.Invoke(container);
        }
    }

    public void OnContainerTakenFromArea(Container container)
    {
        if (containers.Contains(container)) {
            //Debug.Log("Container taken from shelf");
            foreach (PlacingTriggerArea triggerArea in placingTriggers) {
                if (triggerArea.containersInArea.Contains(container)) {
                    return;
                }
            }
            containers.Remove(container);
            OnContainerTriggerExitEvent?.Invoke(container);
        }
    }

    public void OnFurnitureBoxPlacedInArea(FurnitureBox furnitureBox)
    {
        if (!furnitureBoxes.Contains(furnitureBox))
        {
            furnitureBoxes.Add(furnitureBox);
            OnFurnitureBoxTriggerEnterEvent?.Invoke(furnitureBox);
        }
    }

    public void OnFurnitureBoxTakenFromArea(FurnitureBox furnitureBox)
    {
        if (furnitureBoxes.Contains(furnitureBox))
        {
            //Debug.Log("Placed product on shelf");
            foreach (PlacingTriggerArea triggerArea in placingTriggers)
            {
                if (triggerArea.furnitureBoxesInArea.Contains(furnitureBox))
                {
                    return;
                }
            }
            furnitureBoxes.Remove(furnitureBox);
            OnFurnitureBoxTriggerExitEvent?.Invoke(furnitureBox);
        }
    }
}
