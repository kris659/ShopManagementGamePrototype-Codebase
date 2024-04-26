using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerGO : MonoBehaviour, IInteractable, IPickableGO
{
    [SerializeField] bool canBeClosed = false;
    public bool isOpen = false;
    [SerializeField] float openingTime = 0.5f;

    [SerializeField] private GameObject[] containerDoors;
    private Vector3[] containerClosedDoorsPositions;
    [SerializeField] private Vector3[] containerOpenDoorsRotations;

    [SerializeField] private GameObject containerOpenCollider;
    [SerializeField] private GameObject containerClosedCollider;

    [SerializeField] private BoxCollider containerTrigger;
    public bool isOpeningOrClosing = false;

    [HideInInspector]
    public Container container;
    public string textToDisplay { 
        get {
            if (!isOpen)
                return "E - open box";
            else
                return "E - close box";
        } }

    public IPickable pickable => container;

    public bool isPhysixSpawned;

    public static ContainerGO Spawn(bool isOnlyVisual, Vector3 position, Quaternion rotation, Transform parent, Container container, bool isColliderActive = false)
    {
        GameObject containerGO;
        if (isOnlyVisual) {
            containerGO = Instantiate(container.productType.visualPrefab, position, rotation, parent);
            if (isColliderActive) {
                containerGO.GetComponent<Collider>().enabled = true;
            }            
        }
        else 
            containerGO = Instantiate(container.productType.prefab, position, rotation, parent);
        ContainerGO containerGOScript = containerGO.GetComponent<ContainerGO>();
        containerGOScript.container = container;
        containerGOScript.Init(false);
        containerGOScript.isPhysixSpawned = !isOnlyVisual;
        return containerGOScript;
    }

    public void Init(bool isOpen)
    {
        this.isOpen = isOpen;
        if (!canBeClosed) {
            this.isOpen = true;
        }

        DOTween.Init();
        if (isOpen)
            OpenContainter(true);

        InteractableCollider[] interactableColliders = GetComponentsInChildren<InteractableCollider>();
        foreach(InteractableCollider interactable in interactableColliders)
            interactable.Init(this);
    }
    public void OnPlayerInteract()
    {
        if (isOpeningOrClosing)
            return;
        if (isOpen)
            CloseContainer();
        else
            OpenContainter();
    }

    public void OpenContainter(bool openInstantly = false)
    {
        if (!canBeClosed)
            return;
        float localOpeningTime = openInstantly ? 0: openingTime;
        for (int i = 0; i < containerDoors.Length; i++) {
            containerDoors[i].transform.DOLocalRotate(containerOpenDoorsRotations[i], localOpeningTime, RotateMode.LocalAxisAdd);
        }
        StartCoroutine(SetColliderActive(localOpeningTime, true));
    }
    public void CloseContainer(bool closeInstantly = false)
    {        
        if (!canBeClosed)
            return;
        float localClosingTime = closeInstantly ? 0 : openingTime;
        for (int i = 0; i < containerDoors.Length; i++) {
            containerDoors[i].transform.DOLocalRotate(-containerOpenDoorsRotations[i], localClosingTime, RotateMode.LocalAxisAdd);
        }
        StartCoroutine(SetColliderActive(localClosingTime, false));
    }

    IEnumerator SetColliderActive(float duration, bool desiredState)
    {
        isOpeningOrClosing = true;

        if (desiredState) {
            containerOpenCollider.SetActive(true);
            containerClosedCollider.SetActive(false);
            SpawnProducts();
        }
        yield return new WaitForSeconds(duration);
        isOpen = desiredState;
        if (!desiredState) {
            container.UpdateProductsInContainer();
            container.DestroyProductsInTrigger();
            containerOpenCollider.SetActive(isOpen);
            containerClosedCollider.SetActive(!isOpen);
        }
        isOpeningOrClosing = false;
    }

    public void GetProductsInTrigger(out List<Product> productsInTriggerList, out List<Vector3> productsInTriggerPositionsList, out List<Vector3> productsInTriggerRotationsList)
    {
        productsInTriggerList = new List<Product>();
        productsInTriggerPositionsList = new List<Vector3>();
        productsInTriggerRotationsList = new List<Vector3>();

        Vector3 center = transform.position + containerTrigger.center + containerTrigger.transform.localPosition;
        Vector3 halfExtents = containerTrigger.size / 2f;
        Collider[] hitColliders = Physics.OverlapBox(center, halfExtents, transform.rotation, ProductsData.instance.productsLayerMask);

        for (int i = 0; i < hitColliders.Length; i++) {
            if (hitColliders[i].transform.TryGetComponent(out ProductGO productGO)) {
                if (productGO.gameObject != gameObject && !productsInTriggerList.Contains(productGO.product)) {
                    productsInTriggerList.Add(productGO.product);
                    productsInTriggerPositionsList.Add(transform.InverseTransformPoint(productGO.transform.position));
                    productsInTriggerRotationsList.Add(productGO.transform.eulerAngles - transform.eulerAngles);
                }
            }
            else {
                if (hitColliders[i].transform.parent && hitColliders[i].transform.parent.TryGetComponent(out productGO)) {
                    if (productGO.gameObject != gameObject && !productsInTriggerList.Contains(productGO.product)) {
                        productsInTriggerList.Add(productGO.product);
                        productsInTriggerPositionsList.Add(productGO.transform.position - transform.position);
                        productsInTriggerRotationsList.Add(productGO.transform.eulerAngles - transform.eulerAngles);
                    }
                }
            }
        }        
    }

    private void SpawnProducts()
    {
        container.GetProductsInContainerData(out List<Product> productsInContainerList, out List<Vector3> productsInContainerPositionsList, out List<Vector3> productsInContainerRotationsList);
        for (int i = 0; i < productsInContainerList.Count; i++) {
            Vector3 position = transform.TransformPoint(productsInContainerPositionsList[i]);
            Quaternion rotation = Quaternion.Euler(transform.eulerAngles + productsInContainerRotationsList[i]);
            productsInContainerList[i].Place(position, rotation, null);
        }
    }
}
