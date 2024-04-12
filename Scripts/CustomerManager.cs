using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    public static CustomerManager instance;
    [SerializeField] private GameObject customerPrefab;
    [SerializeField] private Transform[] spawnPositions;
    [SerializeField] private float spawnCooldown;
    [SerializeField] private int maxCustomers;

    [SerializeField] private List<Customer> customers;

    [SerializeField] private int customerMinMoney;
    [SerializeField] private int customerMaxMoney;

    private void Awake()
    {
        if (instance != null)
            Destroy(this);
        instance = this;
    }

    void Start()
    {
        StartCoroutine(SpawnCoroutine());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            SpawnCustomer();
    }

    private IEnumerator SpawnCoroutine()
    {
        while (true) {
            yield return new WaitForSeconds(spawnCooldown);
            SpawnCustomer();
        }
    }

    private void SpawnCustomer()
    {
        UpdateCustomerList();
        if (customers.Count >= maxCustomers)
            return;
        Vector3 spawnPosition = GetSpawnPosition();
        GameObject customerGO = Instantiate(customerPrefab, spawnPosition, Quaternion.identity, transform);
        Customer customer = customerGO.GetComponent<Customer>();
        customer.Init(Random.Range(customerMinMoney, customerMaxMoney + 1));
        customers.Add(customer);
    }

    Vector3 GetSpawnPosition()
    {
        return spawnPositions[Random.Range(0, spawnPositions.Length)].position;
    }

    void UpdateCustomerList()
    {
        customers.RemoveAll((customer) => customer == null);
    }

    public void DestroyAll()
    {
        UpdateCustomerList();
        for (int i = 0; i < customers.Count; i++)
            Destroy(customers[i].gameObject);        
        customers.Clear();
    }
}
