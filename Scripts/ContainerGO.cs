using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class ContainerGO : MonoBehaviour, IInteractable, IPickableGO, IInformationDisplay
{
    [SerializeField] bool canBeClosed = false;
    public bool isOpen = false;
    [SerializeField] float openingTime = 0.5f;

    [SerializeField] private GameObject[] containerDoors;
    [SerializeField] private Vector3[] containerOpenDoorsRotations;

    [SerializeField] private GameObject containerOpenCollider;
    [SerializeField] private GameObject containerClosedCollider;

    public BoxCollider containerTrigger;
    public bool isOpeningOrClosing = false;

    [HideInInspector]
    public Container container;
    public string InteractionText { 
        get {
            if (!isOpen)
                return "F - open box";
            else
                return "F - close box";
        } }
    public int InteractionTextSize => 60;

    public string InformationDisplayText => GetInformationDisplayText();

    public void OnMouseButtoDown() { }
    public void OnMouseButton() { }
    public void OnMouseButtonUp() { }

    public IPickable pickable => container;


    public bool isPhysixSpawned;

    public static ContainerGO Spawn(bool isOnlyVisual, Vector3 position, Quaternion rotation, Transform parent, Container container)
    {
        GameObject containerGO;
        if (isOnlyVisual) {
            containerGO = SpawnVisual(container.containerType);
            containerGO.transform.SetParent(parent);
            containerGO.transform.position = position;
            containerGO.transform.rotation = rotation;
        }
        else 
            containerGO = Instantiate(container.containerType.prefab, position, rotation, parent);
        ContainerGO containerGOScript = containerGO.GetComponent<ContainerGO>();
        containerGOScript.container = container;
        containerGOScript.Init(false);
        containerGOScript.isPhysixSpawned = !isOnlyVisual;
        return containerGOScript;
    }

    public static GameObject SpawnVisual(ContainerSO containerType)
    {
        GameObject visual = GameObject.Instantiate(containerType.prefab);
        GameObject.Destroy(visual.GetComponent<Rigidbody>());
        visual.GetComponentsInChildren<Collider>().ToList().ForEach((col) => Destroy(col));
        ContainerGO container = visual.GetComponent<ContainerGO>();
        Destroy(container.containerTrigger);
        return visual;
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
    public void OnPlayerButtonInteract()
    {
        if (isOpeningOrClosing)
            return;
        if (isOpen) {
            CloseContainer();
            AudioManager.PlaySound(Sound.BoxOpen, transform.position);
        }
        else {
            OpenContainter();
            AudioManager.PlaySound(Sound.BoxClose, transform.position);
        }
    }

    public void OpenContainter(bool openInstantly = false)
    {
        if (!canBeClosed)
            return;
        float localOpeningTime = openInstantly ? 0: openingTime;
        for (int i = 0; i < containerDoors.Length; i++) {
            containerDoors[i].transform.DOLocalRotate(containerOpenDoorsRotations[i], localOpeningTime, RotateMode.LocalAxisAdd);
        }

        //StartCoroutine(SetColliderActive(localOpeningTime, true));

        isOpen = true;
        StartCoroutine(OpenContainerCoroutine(localOpeningTime));
    }

    public void CloseContainer(bool closeInstantly = false)
    {        
        if (!canBeClosed)
            return;
        float localClosingTime = closeInstantly ? 0 : openingTime;
        for (int i = 0; i < containerDoors.Length; i++) {
            containerDoors[i].transform.DOLocalRotate(-containerOpenDoorsRotations[i], localClosingTime, RotateMode.LocalAxisAdd);
        }

        //StartCoroutine(SetColliderActive(localClosingTime, false));

        isOpen = false;
        StartCoroutine(CloseContainerCoroutine(localClosingTime));
    }

    IEnumerator OpenContainerCoroutine(float duration)
    {
        isOpeningOrClosing = true;        

        containerOpenCollider.SetActive(true);
        containerClosedCollider.SetActive(false);
        SpawnProducts();
        yield return new WaitForSeconds(duration);
        isOpeningOrClosing = false;
    }

    IEnumerator CloseContainerCoroutine(float duration)
    {
        isOpeningOrClosing = true;

        yield return new WaitForSeconds(duration);

        container.UpdateProductsInContainer();
        container.DestroyProductsInTrigger();
        containerOpenCollider.SetActive(false);
        containerClosedCollider.SetActive(true);

        isOpeningOrClosing = false;
    }

    //IEnumerator SetColliderActive(float duration, bool desiredState)
    //{
    //    isOpeningOrClosing = true;

    //    if (desiredState) {
    //        containerOpenCollider.SetActive(true);
    //        containerClosedCollider.SetActive(false);
    //        SpawnProducts();
    //    }
    //    yield return new WaitForSeconds(duration);
    //    isOpen = desiredState;
    //    if (!desiredState) {
    //        container.UpdateProductsInContainer();
    //        container.DestroyProductsInTrigger();
    //        containerOpenCollider.SetActive(isOpen);
    //        containerClosedCollider.SetActive(!isOpen);
    //    }
    //    isOpeningOrClosing = false;
    //}

    public void GetProductsInTrigger(out List<Product> productsInTriggerList, out List<Vector3> productsInTriggerPositionsList, out List<Vector3> productsInTriggerRotationsList)
    {
        productsInTriggerList = new List<Product>();
        productsInTriggerPositionsList = new List<Vector3>();
        productsInTriggerRotationsList = new List<Vector3>();

        Vector3 center = transform.position + transform.rotation * (containerTrigger.center +  containerTrigger.transform.localPosition);
        Vector3 colliderSize = new Vector3(containerTrigger.size.x * containerTrigger.transform.localScale.x, containerTrigger.size.y * containerTrigger.transform.localScale.y, containerTrigger.size.z * containerTrigger.transform.localScale.z);
        Vector3 halfExtents = colliderSize / 2f;
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

            if(isPhysixSpawned)            
                productsInContainerList[i].Place(position, rotation, null, false);
            else
                productsInContainerList[i].SpawnVisual(position, rotation, transform);
        }
    }

    private string GetInformationDisplayText()
    {
        List<Product> productsInTriggerList;
        if (isOpen)
            GetProductsInTrigger(out productsInTriggerList, out _, out _);
        else 
            container.GetProductsInContainerData(out productsInTriggerList, out _, out _);
       
        if (productsInTriggerList.Count == 0)
            return "Empty box";
        
        List<ProductSO> productTypesInList = new List<ProductSO>();
        List<int> count = new List<int>();
        foreach (Product product in productsInTriggerList) {
            if (productTypesInList.Contains(product.productType)) {
                count[productTypesInList.IndexOf(product.productType)]++;
            }
            else {
                productTypesInList.Add(product.productType);
                count.Add(1);
            }
        }
        string text = "Box:";
        for (int i = 0;i < productTypesInList.Count;i++) {
            //text += "\n- " + productTypesInList[i].Name + ": " + count[i];
            text += "\n- " + count[i] + " " + productTypesInList[i].Name;
        }
        return text;
    }
}
