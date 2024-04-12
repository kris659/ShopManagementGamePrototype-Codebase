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

    private int money;

    private NavMeshAgent agent;
    private Animator animator;

    private Vector3 spawnPoint;
    public void Init(int money)
    {
        //Debug.Log("Customer money: " + money);
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
            Product product = shelf.TakeRandomProduct();
            yield return new WaitForSeconds(1.5f);
            if (product != null){
                productsTaken.Add(product);
                UpdateCustomerMoney();
            }
        }
        yield return new WaitForSeconds(1.5f);
    }

    IEnumerator CheckoutCoroutine()
    {
        while(productsTaken.Count > 0){
            CashRegister cashRegister = ShopData.instance.GetActiveRegister();
            if(cashRegister == null) {
                Debug.Log("Nie ma ¿adnej otwartej kasy!");
                yield return new WaitForSeconds(0.5f);
            }
            else{
                yield return cashRegister.StartCoroutine(GoTo(cashRegister.customerDestination.position));
                yield return cashRegister.StartCoroutine(cashRegister.HandleCustomer(this));
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
        agent.SetDestination(destination);        
        animator.SetBool("isWalking", true);
        while(Vector3.Distance(transform.position, destination) > 0.5f) {
            yield return new WaitForSeconds(0.2f);
        }
        animator.SetBool("isWalking", false);
    }

    private void UpdateCustomerMoney()
    {

    }

    private void OnDestroy()
    {
        for(int i = 0; i < productsTaken.Count; i++) {
            productsTaken[i].Destroy();
        }
    }
}