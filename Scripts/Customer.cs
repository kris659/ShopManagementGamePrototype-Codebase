using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum CustomerState { Shopping, Checkout, LeavingShop }

public class Customer : MonoBehaviour
{
    [SerializeField] GameObject modelParent;
    [SerializeField] int customerVariations;
    public CustomerState customerState = CustomerState.Shopping;
    public List<Product> productsTaken = new List<Product>();
    public List<float> productsTakenPrices = new List<float>();

    private int money;

    private NavMeshAgent agent;
    private Animator animator;

    private Vector3 spawnPoint;
    private IEnumerator currentGoToCoroutine;

    public void Init(int money)
    {
        this.money = money;
        StartCoroutine(SelectAction());
        spawnPoint = transform.position;

        int customerModel = Random.Range(0, customerVariations);
        modelParent.transform.GetChild(customerModel).gameObject.SetActive(true);
        for (int i = 0; i < customerVariations; i++) {
            if (i == customerModel)
                continue;
            Destroy(modelParent.transform.GetChild(i).gameObject);
        }
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = modelParent.GetComponent<Animator>();
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
        while(money > 20 && Random.Range(0, 100) > 20) {
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
        yield return new WaitForSeconds(1.5f);
    }

    bool TakeDecision(Product product)
    {
        float marketPrice = PriceManager.instance.GetProductMarketPrice(product.productType);
        float price = PriceManager.instance.GetProductSellPrice(product.productType);

        float chance = 50 + Mathf.RoundToInt((marketPrice - price) / marketPrice * 100);
        Debug.Log("Chance: " + chance);
        return Random.Range(0, 100) < chance;
    }

    IEnumerator CheckoutCoroutine()
    {
        while(productsTaken.Count > 0){
            CashRegister cashRegister = ShopData.instance.GetActiveRegister();
            if(cashRegister == null) {
                //Debug.Log("Nie ma ¿adnej otwartej kasy!");
                UIManager.textUI.UpdateText("There is no open cash register", 1f);
                yield return new WaitForSeconds(1f);
            }
            else{
                yield return StartCoroutine(GoTo(cashRegister.customerDestination.position));
                if(cashRegister != null) {
                    cashRegister.AddCustomer(this);
                    yield return new WaitUntil(() => cashRegister == null || !cashRegister.IsCustomer(this));
                    for(int i = 0; i < productsTaken.Count; i++)
                        productsTaken[i].DestroyGameObject();
                }
            }
        }
    }

    IEnumerator LeaveShopCoroutine()
    {
        yield return StartCoroutine(GoTo(spawnPoint));
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }
    public IEnumerator GoTo(Vector3 destination)
    {        
        IEnumerator GoToInternal(Vector3 destination)
        {
            agent.SetDestination(destination);
            animator.SetBool("isWalking", true);
            while (Vector3.Distance(transform.position, destination) > 0.5f) {
                yield return new WaitForSeconds(0.2f);
            }
            animator.SetBool("isWalking", false);
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

    private void OnDestroy()
    {
        for(int i = 0; i < productsTaken.Count; i++) {
            productsTaken[i].RemoveProductFromGame(true);
        }
    }
}