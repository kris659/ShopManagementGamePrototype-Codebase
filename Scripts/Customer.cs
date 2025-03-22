using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum CustomerState { Shopping, Checkout, LeavingShop }

public class Customer : MonoBehaviour
{
    [System.Serializable]
    public struct CustomerModelsGroup {
        public Transform modelParent;
        public List<Material> materials;
        public List<GameObject> accesories;
        public Transform GrabPlace;
        public Vector3 GrabPlaceOffset;
    }

    [SerializeField] List<CustomerModelsGroup> modelGroups;
    [SerializeField] GameObject exclamationMark;
    public CustomerState customerState = CustomerState.Shopping;
    public List<Product> productsTaken = new List<Product>();
    public List<float> productsTakenPrices = new List<float>();
    public Transform grabPlace;


    private int money;

    private NavMeshAgent agent;
    private Animator animator;

    private Vector3 spawnPoint;
    private IEnumerator currentGoToCoroutine;

    [SerializeField] private LayerMask buildingsLayerMask;
    private float decorationsCheckingRange = 4f;
    private float customersCheckingRange = 4f;
    private int statsCounter;
    private int decorationStat;
    private int crowdednessStat;
    float checkoutTime = 0;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void Init(int money)
    {
        this.money = money;
        StartCoroutine(SelectAction());
        StartCoroutine(UpdateCustomerStats());
        spawnPoint = transform.position;
        SelectCustomerVisual();
    }

    private void SelectCustomerVisual()
    {
        int customerModelsCount = 0;
        for (int i = 0; i < modelGroups.Count; i++)
        {
            customerModelsCount += modelGroups[i].modelParent.childCount - 1;
        }

        int selectedCustomerModel = Random.Range(0, customerModelsCount);
        int currentIndex = 0;
        bool spawned = false;

        for (int i = 0; i < modelGroups.Count; i++)
        {
            if (!spawned && currentIndex + modelGroups[i].modelParent.childCount >= selectedCustomerModel)
            {
                for (int j = 1; j < modelGroups[i].modelParent.childCount; j++)
                {
                    if (j + currentIndex - 1 == selectedCustomerModel)
                    {
                        animator = modelGroups[i].modelParent.GetComponent<Animator>();
                        spawned = true;
                        modelGroups[i].modelParent.GetChild(j).gameObject.SetActive(true);

                        // Assign a grabPlace of the active root
                        grabPlace.parent = modelGroups[i].GrabPlace;
                        grabPlace.transform.localPosition = Vector3.zero + modelGroups[i].GrabPlaceOffset;
                        grabPlace.transform.localRotation = Quaternion.Euler(71.5000229f, 180, 190.800003f);

                        // Assign a random material
                        Material material = modelGroups[i].materials[Random.Range(0, modelGroups[i].materials.Count)];
                        modelGroups[i].modelParent.GetChild(j).GetComponent<Renderer>().material = material;

                        // Enable only one accessory
                        int accesoryIndex = Random.Range(0, modelGroups[i].accesories.Count);
                        for (int k = 0; k < modelGroups[i].accesories.Count; k++)
                        {
                            if (accesoryIndex == k)
                            {
                                modelGroups[i].accesories[k].gameObject.SetActive(true);
                                modelGroups[i].accesories[k].GetComponent<Renderer>().material = material;
                            }
                            else
                            {
                                Destroy(modelGroups[i].accesories[k].gameObject);
                            }
                        }
                    }
                    else
                    {
                        Destroy(modelGroups[i].modelParent.GetChild(j).gameObject);
                    }
                }
            }
            else
            {
                Destroy(modelGroups[i].modelParent.gameObject);
                foreach (GameObject gameObject in modelGroups[i].accesories)
                {
                    Destroy(gameObject);
                }
            }
            currentIndex += modelGroups[i].modelParent.childCount - 1;
        }

        modelGroups.Clear();
    }

    IEnumerator SelectAction()
    {
        yield return new WaitForSeconds(1.5f);
        yield return StartCoroutine(ShoppingCoroutine());
        customerState = CustomerState.Checkout;        ;
        yield return StartCoroutine(CheckoutCoroutine());
        customerState = CustomerState.LeavingShop;
        StartCoroutine(LeaveShopCoroutine());
    }

    IEnumerator ShoppingCoroutine()
    {
        while (ShopData.instance.isShopOpen && money > 20 && Random.Range(0, 100) > 20) {
            Shelf shelf = ShopData.instance.GetRandomShelfWithProducts();
            if (shelf == null)
                continue;
            yield return StartCoroutine(GoTo(shelf.customerDestination.position));
            Product product = shelf.GetRandomProduct();
            if (product != null && TakeDecision(product)) {
                product.DestroyGameObject();
                productsTaken.Add(product);
                productsTakenPrices.Add(PriceManager.instance.GetProductSellPrice(product.productType));
                UpdateCustomerMoney();
            }
            yield return new WaitForSeconds(1.5f);
        }
        //Use Facility TEST
        if(Random.Range(0, 100) > 75)
        {
            yield return StartCoroutine(FacilityCoroutine());
        }
        //end TEST
        yield return new WaitForSeconds(1.5f);
    }

    bool TakeDecision(Product product)
    {
        float marketPrice = PriceManager.instance.GetMarketPrice(product.productType);
        float price = PriceManager.instance.GetProductSellPrice(product.productType);

        float chance = 50 + Mathf.RoundToInt((marketPrice - price) / marketPrice * 100);
        chance *= product.productType.Popularity / 80.0f;
        //Debug.Log(product.productType.Name + " Chance: " + chance);
        return Random.Range(0, 100) < chance;
    }

    IEnumerator FacilityCoroutine()
    {
        Facility facility = ShopData.instance.GetBestFreeFacility(transform.position);
        if(facility == null)
        {
            Debug.Log("No free facility");
            yield return new WaitForSeconds(1);
        }
        else
        {
            yield return StartCoroutine(GoTo(facility.customerDestination.position));
            if(facility != null)
            {
                facility.myCustomer = this;
                facility.UseFacility();
                yield return new WaitUntil(() => facility.myCustomer == null || facility == null);
            }
            yield return new WaitForSeconds(1);
        }
    }

    IEnumerator CheckoutCoroutine()
    {
        bool wasWaitingForCashRegister = false;

        while (productsTaken.Count > 0){
            CashRegister cashRegister = ShopData.instance.GetBestActiveRegister(transform.position);
            if(cashRegister == null) {
                if (!wasWaitingForCashRegister) {
                    UIManager.warningsUI.AddCustomer();
                    exclamationMark.SetActive(true);
                }
                wasWaitingForCashRegister = true;
                checkoutTime += 1;
                yield return new WaitForSeconds(1f);
            }
            else{
                if (wasWaitingForCashRegister) {
                    wasWaitingForCashRegister = false;
                    UIManager.warningsUI.RemoveCustomer();
                    exclamationMark.SetActive(false);
                }
                yield return StartCoroutine(GoTo(cashRegister.customerDestination.position));
                if(cashRegister != null) {
                    cashRegister.AddCustomer(this);
                    float checkoutStartTime = Time.time;
                    yield return new WaitUntil(() => cashRegister == null || !cashRegister.IsCustomer(this));
                    for(int i = 0; i < productsTaken.Count; i++)
                        productsTaken[i].DestroyGameObject();
                    checkoutTime += Time.time - checkoutTime;
                }
            }
        }
    }

    IEnumerator LeaveShopCoroutine()
    {
        Destroy(gameObject, 15);
        if(statsCounter > 2)
            CustomerManager.instance.UpdateCustomerStats((float) decorationStat / statsCounter, (float) crowdednessStat / statsCounter, checkoutTime);

        yield return StartCoroutine(GoTo(spawnPoint));
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }
    public IEnumerator GoTo(Vector3 destination)
    {
        destination.y = 24;
        IEnumerator GoToInternal(Vector3 destination)
        {
            agent.SetDestination(destination);
            agent.isStopped = false;
            animator.SetBool("isWalking", true);
            while (Vector3.Distance(transform.position, destination) > 0.5f) {
                yield return new WaitForSeconds(0.2f);
            }
            animator.SetBool("isWalking", false);
            agent.isStopped = true;
        }
        if (currentGoToCoroutine != null)
            StopCoroutine(currentGoToCoroutine);
        currentGoToCoroutine = GoToInternal(destination);
        yield return StartCoroutine(currentGoToCoroutine);
        currentGoToCoroutine = null;
    }

    private void UpdateCustomerMoney()
    {

    }

    public void RemoveProductFromList(int productIndex)
    {
        PlayerData.instance.AddMoney(productsTakenPrices[productIndex], true);
        TasksManager.instance.ProgressTasks(TaskType.SellProducts, 1);
        TasksManager.instance.ProgressTasks(TaskType.SellProductsOfType, 1, new int[1] { SOData.GetProductIndex(productsTaken[productIndex].productType) });
        productsTaken[productIndex].RemoveFromGame(true);
        productsTaken.RemoveAt(productIndex);
        productsTakenPrices.RemoveAt(productIndex);
    }

    IEnumerator UpdateCustomerStats()
    {
        yield return new WaitForSeconds(5);
        statsCounter++;
        decorationStat += GetDecorationsInRange();
        crowdednessStat += GetCustomersInRange();
        StartCoroutine(UpdateCustomerStats());
    }

    private int GetDecorationsInRange()
    {
        Collider[] colliders = Physics.OverlapBox(transform.position + Vector3.up * 1f, new Vector3(decorationsCheckingRange, 3f, decorationsCheckingRange), Quaternion.identity, buildingsLayerMask);
        List<Decoration> decorations = new List<Decoration>();
        foreach (Collider collider in colliders) {
            Decoration decoration = collider.GetComponentInParent<Decoration>();
            if(decoration != null && !decorations.Contains(decoration))
                decorations.Add(decoration);

        }
        int totalDecorationScore = 0;
        foreach (Decoration decoration in decorations) {
            totalDecorationScore += decoration.decorationScore;
        }
        // TO JESZCZE TRZEBA PRZETESTOWAÆ BO COŒ ZA DU¯O RAZ BY£O
        //Debug.Log(gameObject.name + "; Decorations in range: " + decorations.Count + " Decoration score: " + totalDecorationScore);
        return totalDecorationScore;
    }

    private int GetCustomersInRange()
    {
        Collider[] colliders = Physics.OverlapBox(transform.position, new Vector3(customersCheckingRange, 1f, customersCheckingRange), Quaternion.identity, LayerMask.GetMask("Customer"));
        List<Customer> customers = new List<Customer>();
        foreach (Collider collider in colliders) {
            Customer customer = collider.GetComponentInParent<Customer>();
            if (customer != null && customer != this && !customers.Contains(customer))
                customers.Add(customer);

        }
        return customers.Count;
    }

    private void OnDestroy()
    {
        for(int i = 0; i < productsTaken.Count; i++) {
            productsTaken[i].RemoveFromGame(true);
        }
    }
}