using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class CashRegister: Building
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
    public int CustomerCount { get { return customers.Count; } }

    public int maxCustomers;
    List<Customer> customers = new List<Customer>();
    [SerializeField] private List<Transform> customerQueuePositions = new List<Transform>();
    public Transform customerDestination;
    public Transform workerDestination;
    public Action OnDestroyEvent;

    private List<Product> productsOnRegister = new List<Product>();
    [SerializeField] private BoxCollider productsSpawnCollider;
    [SerializeField] private Transform productsParent;

    public override void Build()
    {
        base.Build();
        TriggerHandler triggerHandler = GetComponentInChildren<TriggerHandler>();

        triggerHandler.triggerEnter += TriggerEnter;
        triggerHandler.triggerExit += TriggerExit;

        if (maxCustomers != customerQueuePositions.Count)
            Debug.LogError("MaxCustomers != customerPositions.Count");
        ShopData.instance.AddRegister(this);
        UpdateStatus();
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
        
        while(firstCustomer.productsTaken.Count > 0) {
            if (!IsOpen) {
                customers.Clear();
                yield break;
            }
            SpawnProducts();
            while (productsOnRegister.Count > 0) {
                yield return new WaitUntil( () => { return isWorker || !isPlayer || productsOnRegister.Count == 0; });
                if(isWorker && productsOnRegister.Count > 0) {
                    yield return new WaitForSeconds(checkoutTime);
                    if (!IsOpen) {
                        customers.Clear();
                        productsOnRegister.Clear();
                        yield break;
                    }
                    if (isWorker && productsOnRegister.Count > 0) {
                        int lastIndex = firstCustomer.productsTaken.Count - 1;
                        productsOnRegister.Remove(firstCustomer.productsTaken[lastIndex]);
                        firstCustomer.RemoveProductFromList(lastIndex);
                        AudioManager.PlaySound(Sound.ProductScan, transform.position);
                    }                    
                }
                if (!IsOpen) {
                    customers.Clear();
                    productsOnRegister.Clear();
                    yield break;
                }
            }
        }
        if(!isWorker && isPlayer)
            TasksManager.instance.ProgressTasks(TaskType.ServeCustomers, 1);
        AudioManager.PlaySound(Sound.CheckoutFinished, transform.position);
        customers.RemoveAt(0);
        for (int i = 0; i < customers.Count; i++) {
            customers[i].StartCoroutine(customers[i].GoTo(customerQueuePositions[i].position));
        }
        UpdateStatus();
        if (customers.Count > 0) {
            StartCoroutine(CheckoutCustomers());
        }
    }

    public void PlayerRemoveProduct(Product product)
    {
        productsOnRegister.Remove(product);
        customers[0].RemoveProductFromList(customers[0].productsTaken.IndexOf(product));
        AudioManager.PlaySound(Sound.ProductScan, transform.position);
    }

    private void SpawnProducts()
    {
        AudioManager.PlaySound(Sound.CustomerPutOnRegister, transform.position);
        List<Product> productsReversed = new List<Product>(customers[0].productsTaken);
        productsReversed.Reverse();
        productsOnRegister.Clear();
        ProductsData.instance.GetInTriggerPositions(productsReversed, productsSpawnCollider, out List<Vector3> positions);
        for (int i = 0; i < positions.Count; i++) {
            productsOnRegister.Add(productsReversed[i]);
            Vector3 position = productsSpawnCollider.transform.position + transform.rotation * (-positions[i] + new Vector3(0, -productsSpawnCollider.size.y, 0) * 0.5f);
            Quaternion rotation = Quaternion.Euler(transform.eulerAngles);
            productsReversed[i].SpawnVisual(position, rotation, productsParent);
            CashRegisterProduct cashRegisterProduct = productsReversed[i].productGO.AddComponent<CashRegisterProduct>();
            cashRegisterProduct.Init(this, productsReversed[i]);
            productsReversed[i].productGO.transform.rotation = rotation;
        }
        //productsParent.localEulerAngles += transform.localEulerAngles + new Vector3(0, 180, 0);
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


    private void OnDestroy()
    {
        ShopData.instance.RemoveRegister(this);
        OnDestroyEvent?.Invoke();
        customers.Clear();
        StopAllCoroutines();
        Destroy(gameObject);
    }
}
