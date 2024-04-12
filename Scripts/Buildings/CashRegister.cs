using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CashRegister : MonoBehaviour, IBuildable
{

    [SerializeField] private TMP_Text text;
    [SerializeField] private Color closedTextColor;
    [SerializeField] private Color openTextColor;

    [SerializeField] private LayerMask playerLayer;

    [SerializeField] private float checkoutTime;

    public bool isPlayer = false;
    public bool isActive = false;
    public int maxCustomers;
    List<Customer> customers = new List<Customer>();
    [SerializeField] private List<Transform> customerQueuePositions = new List<Transform>();
    public Transform customerDestination;

    public static void Spawn(int typeIndex, Vector3 position, Quaternion rotation)
    {
        GameObject registerGO = Instantiate(SOData.registersList[typeIndex].Prefab, position, rotation);
        CashRegister register = registerGO.GetComponent<CashRegister>();
        TriggerHandler triggerHandler = registerGO.GetComponentInChildren<TriggerHandler>();

        triggerHandler.triggerEnter += register.TriggerEnter;
        triggerHandler.triggerExit += register.TriggerExit;

        

        if (register.maxCustomers != register.customerQueuePositions.Count)
            Debug.LogError("MaxCustomers != customerPositions.Count");
        ShopData.instance.AddRegister(register);
        register.SetActive();
    }

    public IEnumerator HandleCustomer(Customer customer)
    {
        if (!isActive)
            yield break;
        AddCustomer(customer);
        yield return new WaitUntil(() => customers.Contains(customer) == false);
    }

    void AddCustomer(Customer customer)
    {
        customers.Add(customer);
        SetActive();
        Vector3 customerDestination = customerQueuePositions[customers.Count - 1].position;
        customer.StartCoroutine(customer.GoTo(customerDestination));
        if(customers.Count == 1)
            StartCoroutine(CheckoutCustomers());
    }



    IEnumerator CheckoutCustomers()
    {
        if(customers.Count == 0) {
            Debug.LogError("Nie powinno tak byæ!");
            yield break;
        }
        Customer firstCustomer = customers[0];
        yield return firstCustomer.GoTo(customerQueuePositions[0].position);
        Debug.Log("Start checkout");
        for (int i = firstCustomer.productsTaken.Count - 1; i >= 0; i--){
            yield return new WaitForSeconds(checkoutTime);
            PlayerData.instance.AddMoney(firstCustomer.productsTaken[i].productType.sellingPrice);
            firstCustomer.productsTaken.RemoveAt(i);            
        }
        customers.RemoveAt(0);
        for (int i = 0; i < customers.Count; i++) {
            customers[i].StartCoroutine(customers[i].GoTo(customerQueuePositions[i].position));
        }
        SetActive();
        if (customers.Count > 0) {
            StartCoroutine(CheckoutCustomers());
        }
    }


    private void TriggerEnter(Collider other)
    {        
        if(other.tag == "Player")
        {
            isPlayer = true;
            SetActive();
        }
    }
    private void TriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            isPlayer = false;
            SetActive();
        }
    }

    void SetActive()
    {
        if (isPlayer && maxCustomers > customers.Count) 
            isActive = true;
        else isActive = false;
        if (isActive) {
            text.text = "Open";
            text.color = openTextColor;
            ShopData.instance.UpdateRegisterStatus(this);
        }
        else
        {
            text.text = "Closed";
            text.color = closedTextColor;
            ShopData.instance.UpdateRegisterStatus(this);
        }
    }
    public bool CanBuildHere(Vector3 position, Quaternion rotation)
    {
        return true;
    }

    public void Build(int typeIndex, Vector3 position, Quaternion rotation)
    {
        Spawn(typeIndex, position, rotation);
    }
    public void Destroy()
    {
        ShopData.instance.RemoveRegister(this);
        Destroy(gameObject);
    }
}
