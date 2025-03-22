using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    public static CustomerManager instance;
    [SerializeField] private GameObject customerPrefab;
    public GameObject exlclamationMarkPrefab;
    [SerializeField] private Transform[] spawnPositions;
    [SerializeField] private List<GameObject> garbagePrefab = new();

    [SerializeField] private float baseSpawnCooldown;
    [SerializeField] private AnimationCurve customerSpawnRatingMult;
    [SerializeField] private AnimationCurve customerSpawnTimeMult;

    [SerializeField] private int customerMinMoney;
    [SerializeField] private int customerMaxMoney;

    [SerializeField] private List<Customer> customers;

    public List<GameObject> SpawnedGarbage = new();

    private void Awake()
    {
        if (instance != null)
            Destroy(this);
        instance = this;
    }

    void Start()
    {
        StartCoroutine(SpawnCoroutine());
        StartCoroutine(SpawningGarbage());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            SpawnGarbage();
        }
    }

    private IEnumerator SpawnCoroutine()
    {
        while (true){
            float cooldown = GetCustomerSpawnCooldown();
            yield return new WaitForSeconds(cooldown);

            if(ShopData.instance.isShopOpen && TimeManager.instance.Hour >= 7 && TimeManager.instance.Hour < 20)
                SpawnCustomer();
        }
    }

    private float GetCustomerSpawnCooldown()
    {
        float shopRating = ShopPopularityManager.instance.shopPopularityValues[(int)ShopPopularityCategory.OverallPopularity] / 10f;
        float time = TimeManager.instance.Hour / 24f;

        float cooldown = baseSpawnCooldown / (customerSpawnRatingMult.Evaluate(shopRating) * customerSpawnTimeMult.Evaluate(time));
        //Debug.Log("Customer cooldown: " + cooldown);
        return cooldown;
    }

    private IEnumerator SpawningGarbage()
    {
        while (true)
        {
            int min = Mathf.Max(20 - customers.Count / 2, 6);
            int max = Mathf.Max(40 - customers.Count / 2, 12);
            yield return new WaitForSeconds(Random.Range(min, max));
            SpawnGarbage();
        }
    }

    private void SpawnCustomer()
    {
        UpdateCustomerList();
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

    public void SpawnGarbage()
    {
        if (customers.Count == 0) return; // No customers -> no garbage

        int garbageCount = Mathf.CeilToInt(customers.Count * 0.1f); // Spawn garbage for x% of customers
        for (int i = 0; i < garbageCount; i++)
        {
            Customer randomCustomer = customers[Random.Range(0, customers.Count)];
            if(randomCustomer != null)
            {
                Vector3 garbagePosition = randomCustomer.transform.position + new Vector3(0, -0.008f, 0);
                garbagePosition += new Vector3(Random.Range(-0.3f, 0.3f), 0, Random.Range(-0.3f, 0.3f));

                Quaternion Rotacja = Random.rotation;
                Rotacja.x = 0;
                Rotacja.z = 0;
                SpawnedGarbage.Add(Instantiate(garbagePrefab[Random.Range(0, garbagePrefab.Count)], garbagePosition, Rotacja));
            }
        }
    }

    public void UpdateCustomerStats(float decorationStatAvg, float crowdednessStatAvg, float checkoutTime)
    {
        float desiredDecoration = decorationStatAvg / 2.5f;
        float desiredOvercrowding = Mathf.Clamp(10 - crowdednessStatAvg, 0, 10);
        if (crowdednessStatAvg < 5)
            desiredOvercrowding = 10;
        float maxChange = .25f;
        float currentDecorationValue = ShopPopularityManager.instance.shopPopularityValues[(int)ShopPopularityCategory.Decorations];
        float currentOvercrowdingValue = ShopPopularityManager.instance.shopPopularityValues[(int)ShopPopularityCategory.Overcrowding];

        float decorationStatChange = Mathf.Clamp(desiredDecoration - currentDecorationValue, -maxChange, maxChange);
        float crowdednessStatChange = Mathf.Clamp(desiredOvercrowding - currentOvercrowdingValue, -maxChange, maxChange);

        if (Mathf.Abs(desiredDecoration - currentDecorationValue) > 3)
            decorationStatChange *= 2;
        if (Mathf.Abs(desiredDecoration - currentDecorationValue) > 6)
            decorationStatChange *= 2;

        if (Mathf.Abs(desiredOvercrowding - currentOvercrowdingValue) > 3)
            crowdednessStatChange *= 2;
        if (Mathf.Abs(desiredOvercrowding - currentOvercrowdingValue) > 6)
            crowdednessStatChange *= 2;

        ShopPopularityManager.instance.UpdatePopularity(ShopPopularityCategory.Decorations, currentDecorationValue + decorationStatChange);
        ShopPopularityManager.instance.UpdatePopularity(ShopPopularityCategory.Overcrowding, currentOvercrowdingValue + crowdednessStatChange);

        if (checkoutTime > 0.01f)
        {
            float maxPointsTime = 15;
            float rating = 10;

            if (checkoutTime > maxPointsTime)
            {
                rating -= 10 * (checkoutTime - maxPointsTime) / 45;
                rating = Mathf.Max(rating, 0);

                float currentRating = ShopPopularityManager.instance.shopPopularityValues[(int)ShopPopularityCategory.Overcrowding];

                float ratingChange = Mathf.Clamp(rating - currentRating, -maxChange, maxChange);

                if (Mathf.Abs(rating - currentRating) > 3)
                    ratingChange *= 2;
                if (Mathf.Abs(rating - currentRating) > 6)
                    ratingChange *= 2;

                ShopPopularityManager.instance.UpdatePopularity(ShopPopularityCategory.CustomerService, currentRating + ratingChange);
            }
        }
    }

    public void DestroyAll()
    {
        UpdateCustomerList();
        for (int i = 0; i < customers.Count; i++)
            Destroy(customers[i].gameObject);
        customers.Clear();
        for(int i=0; i<SpawnedGarbage.Count; i++)
        {
            Destroy(SpawnedGarbage[i].gameObject);
        }
        SpawnedGarbage.Clear();
    }
}
