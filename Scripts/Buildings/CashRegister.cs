using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class CashRegister : MonoBehaviour, IBuildable
{
    [SerializeField] private TMP_Text openText;
    [SerializeField] private TMP_Text fullText;
    [SerializeField] private Color closedTextColor;
    [SerializeField] private Color openTextColor;

    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float checkoutTime;

    public bool isPlayer = false;
    public bool isWorker = false;
    public bool IsFull { get { return customers.Count >= maxCustomers; } }
    public bool IsOpen { get { return isPlayer || isWorker; } }
    public int maxCustomers;
    List<Customer> customers = new List<Customer>();
    [SerializeField] private List<Transform> customerQueuePositions = new List<Transform>();
    public Transform customerDestination;
    public Transform workerDestination;
    public Action OnDestroy;

    private List<Product> productsOnRegister = new List<Product>();
    [SerializeField] private BoxCollider productsSpawnCollider;
    [SerializeField] private Transform productsParent;

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
        register.UpdateStatus();
    }

    public bool IsCustomer(Customer customer)
    {
        return customers.Contains(customer);
    }

    public void AddCustomer(Customer customer)
    {
        if (!IsOpen || IsFull)
            return;
        customers.Add(customer);
        UpdateStatus();
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
        SpawnProducts();
        //Debug.Log("Start checkout");
        for (int i = firstCustomer.productsTaken.Count - 1; i >= 0; i--){
            yield return new WaitForSeconds(checkoutTime);
            if (!IsOpen){
                customers.Clear();
                yield break;
            }
            PlayerData.instance.AddMoney(firstCustomer.productsTakenPrices[i]);
            productsOnRegister.Remove(firstCustomer.productsTaken[i]);            
            firstCustomer.productsTaken[i].DestroyGameObject();
            firstCustomer.productsTaken.RemoveAt(i);
            if (productsOnRegister.Count == 0 && firstCustomer.productsTaken.Count != 0)
                SpawnProducts();
        }
        customers.RemoveAt(0);
        for (int i = 0; i < customers.Count; i++) {
            customers[i].StartCoroutine(customers[i].GoTo(customerQueuePositions[i].position));
        }
        UpdateStatus();
        if (customers.Count > 0) {
            StartCoroutine(CheckoutCustomers());
        }
    }
    
    private void SpawnProducts()
    {
        List<Product> productsReversed = new List<Product>(customers[0].productsTaken);
        productsReversed.Reverse();
        ProductsData.instance.GetInTriggerPositions(productsReversed, productsSpawnCollider, out List<Vector3> positions, false);
        for (int i = 0; i < positions.Count; i++) {
            productsOnRegister.Add(productsReversed[i]);
            Vector3 position = positions[i] + productsSpawnCollider.transform.position - productsSpawnCollider.transform.localScale * 0.5f;
            Quaternion rotation = transform.rotation;
            productsReversed[i].SpawnVisual(position, rotation, productsParent);
        }
        productsParent.localEulerAngles += transform.localEulerAngles + new Vector3(0, 180, 0);
    }

    private void TriggerEnter(Collider other)
    {        
        if(other.tag == "Player")
        {
            isPlayer = true;
            UpdateStatus();
        }
    }
    private void TriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            isPlayer = false;
            UpdateStatus();
        }
    }

    public void AddWorker()
    {
        isWorker = true;
        UpdateStatus();
    }

    public void RemoveWorker()
    {
        isWorker = false;
        UpdateStatus();
    }

    void UpdateStatus()
    {
        fullText.text = "";
        if (IsOpen) {
            openText.text = "Open";
            openText.color = openTextColor;
            if (IsFull)
                fullText.text = "Full";            
        }
        else{
            openText.text = "Closed";
            openText.color = closedTextColor;            
        }
        ShopData.instance.UpdateRegisterStatus(this);
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
        OnDestroy?.Invoke();
        customers.Clear();
        StopAllCoroutines();
        Destroy(gameObject);
    }
}
