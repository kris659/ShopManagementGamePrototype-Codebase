using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour, IInteractable
{
    [SerializeField] bool canBeClosed = false;
    public bool isOpen = false;
    [SerializeField] float openingTime = 0.5f;

    [SerializeField] private GameObject[] containerDoors;
    [SerializeField] private GameObject[] containerOpenDoorsPositions;
    private Vector3[] containerClosedDoorsPositions;
    [SerializeField] private Vector3[] containerOpenDoorsRotations;

    [SerializeField] private GameObject containerOpenCollider;
    [SerializeField] private GameObject containerClosedCollider;

    [SerializeField] private BoxCollider containerTrigger;
    private bool isOpeningOrClosing = false;

    List<Product> productsInTriggerList = new List<Product>();
    List<Vector3> productsInTriggerPositionsList = new List<Vector3>();
    List<Vector3> productsInTriggerRotationsList = new List<Vector3>();

    public string textToDisplay { 
        get {
            if (!isOpen)
                return "E - open box";
            else
                return "E - close box";
        } }


    public void Init(bool isOpen, List<Product> productsInTriggerList, List<Vector3> productsInTriggerPositionsList, List<Vector3> productsInTriggerRotationsList)
    {
        this.productsInTriggerList = productsInTriggerList;
        this.productsInTriggerPositionsList = productsInTriggerPositionsList;
        this.productsInTriggerRotationsList = productsInTriggerRotationsList;

        this.isOpen = isOpen;
        if (!canBeClosed) {
            this.isOpen = true;
            SpawnProducts();
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
            //containerDoors[i].transform.DOMove(containerOpenDoorsTransforms[i].transform.position, localOpeningTime);
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
            //containerDoors[i].transform.DOMove(containerClosedDoorsTransforms[i].transform.position, localClosingTime);
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
            UpdateProductsInContainerData();
            DestroyProductsInTrigger();
            containerOpenCollider.SetActive(isOpen);
            containerClosedCollider.SetActive(!isOpen);
        }        
        isOpeningOrClosing = false;
    }

    public void GetProductsInContainerData(out List<Product> productsInContainer, out List<Vector3> productsPositions, out List<Vector3> productsRotations)
    {
        //if (isOpen)
        //    UpdateProductsInContainerData();
        if (isOpen)
            CloseContainer(true);

        productsInContainer = this.productsInTriggerList;
        productsPositions = this.productsInTriggerPositionsList;
        productsRotations = this.productsInTriggerRotationsList;
    }

    private void UpdateProductsInContainerData()
    {
        productsInTriggerList.Clear();
        productsInTriggerPositionsList.Clear();
        productsInTriggerRotationsList.Clear();

        Vector3 center = transform.position + containerTrigger.center + containerTrigger.transform.localPosition;
        Vector3 halfExtents = containerTrigger.size / 2f;
        Collider[] hitColliders = Physics.OverlapBox(center, halfExtents, transform.rotation, ProductsData.instance.productsLayerMask);

        for (int i = 0; i < hitColliders.Length; i++) {
            Debug.Log(hitColliders[i].transform.name);

            if (hitColliders[i].transform.TryGetComponent(out ProductGO productGO)) {
                if (productGO.gameObject != gameObject && !productsInTriggerList.Contains(productGO.product)) {
                    productsInTriggerList.Add(productGO.product);
                    productsInTriggerPositionsList.Add(productGO.transform.position - transform.position);
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
        //Debug.Log("Pick up: " + productsInTriggerList.Count);
    }

    private void SpawnProducts()
    {
        //Debug.Log("Place: " + productsInTriggerList.Count);

        for (int i = 0; i < productsInTriggerList.Count; i++) {
            Vector3 position = transform.position + productsInTriggerPositionsList[i];
            Quaternion rotation = Quaternion.Euler(transform.eulerAngles + productsInTriggerRotationsList[i]);
            productsInTriggerList[i].Place(position, rotation, null);
        }
    }

    private void DestroyProductsInTrigger()
    {
        foreach (Product product in productsInTriggerList) {
            product.CloseInContainer();
        }
    }
}
