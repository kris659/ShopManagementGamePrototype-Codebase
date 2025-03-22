using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class WorkersManager : MonoBehaviour
{
    public static WorkersManager instance;
    public List<Worker> workers = new List<Worker>();
    [SerializeField] private GameObject maleWorkerPrefab;
    [SerializeField] private GameObject femaleWorkerPrefab;

    public List<CleaningMachine> CleaningMachines = new();

    public int hirePrice => (workers.Count + 1) * 1000;

    private List<Container> usedContainers = new List<Container>();
    private List<PlacingTriggerArea> usedShelfTriggers = new List<PlacingTriggerArea>();

    public LayerMask pickableLayerMask;
    private void Awake()
    {
        if(instance != null) {
            Debug.LogError("Multiple WorkerManagers");
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void Start()
    {
        TimeManager.instance.OnHourChanged += PayWorkers;
    }

    public void GetActionForRestocker(Vector3 workerPosition, out Container containerToHandle, out PlacingTriggerArea shelfTrigger, out bool placeOnlyContainer)
    {
        placeOnlyContainer = false;
        List<Container> containersInShop = ProductsData.instance.ContainersInShop;
        List<PlacingTriggerArea> shelfTriggers = GetShelfTriggers(51);
        //Debug.Log(shelfTriggers.Count);
        foreach (PlacingTriggerArea placingTriggerArea in shelfTriggers) {
            if(placingTriggerArea == null) continue;
            Container container = GetContainerForShelfTrigger(placingTriggerArea, containersInShop);
            if(container != null) {
                containerToHandle = container;
                shelfTrigger = placingTriggerArea;
                return;
            }
        }
        foreach (Container container in containersInShop) {
            if (!usedContainers.Contains(container) && !container.IsContainerEmpty() && !container.isContainerOnStorageShelf) {
                shelfTrigger = GetWarehouseShelfTrigger(workerPosition, container);
                if (shelfTrigger != null) {
                    placeOnlyContainer = true;
                    containerToHandle = container;
                    return;
                }
            }
        }
        foreach (Container container in containersInShop) {
            if (!usedContainers.Contains(container) && container.IsContainerEmpty()) {
                containerToHandle = container;
                shelfTrigger = null;
                return;
            }
        }
        shelfTriggers = GetShelfTriggers(100);
        foreach (PlacingTriggerArea placingTriggerArea in shelfTriggers) {
            if (placingTriggerArea == null) continue;
            Container container = GetContainerForShelfTrigger(placingTriggerArea, containersInShop);
            if (container != null) {
                containerToHandle = container;
                shelfTrigger = placingTriggerArea;
                return;
            }
        }
        containerToHandle = null;
        shelfTrigger = null;
    }

    public void GetActionForRestocker(Vector3 workerPosition, Container containerToHandle, out PlacingTriggerArea shelfTrigger, out bool placeOnlyContainer)
    {
        containerToHandle.GetProductsInContainerData(out List<Product> productsInContainer, out List<Vector3> _, out List<Vector3> _);
        shelfTrigger = GetShelfTrigger(productsInContainer[0].productType);
        if(shelfTrigger != null) {
            placeOnlyContainer = false;
            return;
        }
        shelfTrigger = GetWarehouseShelfTrigger(workerPosition, containerToHandle);
        placeOnlyContainer = true;
    }

    private List<PlacingTriggerArea> GetShelfTriggers(int maxPercentageUsed)
    {
        List<PlacingTriggerArea> shelfTriggers = ShopData.instance.GetShelfTriggersList();
        List<PlacingTriggerArea> resultShelfTriggers = new List<PlacingTriggerArea>();
        foreach (PlacingTriggerArea trigger in shelfTriggers) {            
            if(!usedShelfTriggers.Contains(trigger) && trigger.currentProduct != null && trigger.GetPercentageUsed() < maxPercentageUsed) {
                resultShelfTriggers.Add(trigger);
            }
        }
        //List<Order> SortedList = objListOrder.OrderBy(o => o.OrderDate).ToList();
        //Debug.Log(resultShelfTriggers.Count);
        return resultShelfTriggers.OrderBy(t => t.GetPercentageUsed()).ToList();
    }
    private PlacingTriggerArea GetShelfTrigger(ProductSO productSO)
    {
        List<PlacingTriggerArea> resultShelfTriggers = new List<PlacingTriggerArea>();
        List<PlacingTriggerArea> emptyShelfTriggers = ShopData.instance.GetShelfTriggersList();
        foreach (PlacingTriggerArea trigger in emptyShelfTriggers) {
            if (!usedShelfTriggers.Contains(trigger) && trigger.currentProduct == productSO && trigger.GetPercentageUsed() <= 80) {
                resultShelfTriggers.Add(trigger);
            }
        }
        if(resultShelfTriggers.Count == 0)
            return null;
        return resultShelfTriggers.OrderBy(t => t.GetPercentageUsed()).ToList() [0];
    }

    private Container GetContainerForShelfTrigger(PlacingTriggerArea shelfTrigger, List<Container> containersInShop)
    {
        Container containerOnWarehouseShelf = null;
        foreach (Container container in containersInShop) {
            if (usedContainers.Contains(container))
                continue;
            Vector3 containerPosition = container.GetTakingPosition().position;
            containerPosition.y = 24;
            if (!NavMesh.SamplePosition(containerPosition, out _, 0.1f, 1))
                continue;
            container.GetProductsInContainerData(out List<Product> productsInContainer, out _, out _);
            if (productsInContainer.Count > 0 && productsInContainer[0].productType == shelfTrigger.currentProduct) {
                if (container.isContainerOnStorageShelf && containerOnWarehouseShelf == null)
                    containerOnWarehouseShelf = container;
                else
                    return container;
            }
        }        
        return containerOnWarehouseShelf;
    }

    private PlacingTriggerArea GetWarehouseShelfTrigger(Vector3 workerPosition, Container container)
    {
        List<PlacingTriggerArea> shelfTriggers = ShopData.instance.GetWarehouseShelfTriggersList();

        // DODAÆ SORTOWANIE PO ODLEG£OŒCI OD PRACOWNIKA

        foreach (PlacingTriggerArea trigger in shelfTriggers) {
            if (!usedShelfTriggers.Contains(trigger) && trigger.CanRestockerPlaceContainer(container)) {
                return trigger;
            }
        }
        return null;
    }

    public void BlockUsage(Container container, PlacingTriggerArea placingArea)
    {
        if(container != null) {
            usedContainers.Add(container);
        }
        if(placingArea != null) {
            usedShelfTriggers.Add(placingArea);
        }
    }
    public void UnblockUsage(Container container, PlacingTriggerArea placingArea)
    {
        usedContainers.Remove(container);
        usedShelfTriggers.Remove(placingArea);
    }

    private void PayWorkers()
    {
        foreach(Worker worker in workers) {
            PlayerData.instance.TakeMoney(worker.workerWage);
        }
    }

    public void HireWorker()
    {
        if (!PlayerData.instance.CanAfford(hirePrice))
            return;

        PlayerData.instance.TakeMoney(hirePrice);
        bool isFemale = Random.Range(0, 2) == 0;
        SpawnWorker(new Vector3(0,24,0), GetNewName(isFemale), 0, 10, isFemale, -1);
        AudioManager.PlaySound(Sound.PlayerOrder);
        TasksManager.instance.ProgressTasks(TaskType.HaveEmployees, 1);
    }

    public void ChangeWorkerAction(int workerIndex, WorkerTask workerTask)
    {
        workers[workerIndex].ChangeWorkerTask(workerTask);
    }

    public void GetWorkerData(int workerIndex, out string name, out WorkerTask workerTask, out int wage)
    {
        workers[workerIndex].GetWorkerData(out name, out workerTask, out wage);
    }

    private void SpawnWorker(Vector3 position, string name, int workerTask, int wage, bool isFemale, int containerIndex)
    {
        GameObject workerGO;
        if(isFemale)
            workerGO = Instantiate(femaleWorkerPrefab, position, Quaternion.identity);
        else
            workerGO = Instantiate(maleWorkerPrefab, position, Quaternion.identity);

        Worker worker = workerGO.GetComponent<Worker>();
        worker.Init(name, workerTask, wage, isFemale, containerIndex);
        workers.Add(worker);
    }

    public void FireWorker(Worker worker)
    {
        worker.ChangeWorkerTask(WorkerTask.Fired);
        workers.Remove(worker);
    }

    public void DestroyAll()
    {
        for(int i = 0; i < workers.Count; i++) {
            Destroy(workers[i].gameObject);
        }
        workers.Clear();
        usedContainers.Clear();
        usedShelfTriggers.Clear();
    }

    public WorkerSaveData[] GetSaveData()
    {
        WorkerSaveData[] workersSaveData = new WorkerSaveData[workers.Count];
        for(int i = 0; i < workersSaveData.Length; i++) {
            workersSaveData[i] = workers[i].GetWorkerSaveData();
        }
        return workersSaveData;
    }
    public void LoadFromSaveData(WorkerSaveData[] workersSaveData)
    {
        for(int i = 0; i < workersSaveData.Length; i++) {
            SpawnWorker(workersSaveData[i].position, workersSaveData[i].name, workersSaveData[i].task, workersSaveData[i].wage, workersSaveData[i].isFemale, workersSaveData[i].containerIndex);
        }
    }
    private string GetNewName(bool isFemale)
    {
        string maleFirstNamesFile = Path.Combine(Application.streamingAssetsPath, "Workers", "male-first-names.txt");
        string femaleFirstNamesFile = Path.Combine(Application.streamingAssetsPath, "Workers", "female-first-names.txt");
        string lastNamesFile = Path.Combine(Application.streamingAssetsPath, "Workers", "last-names.txt");

        int nameIndex = Random.Range(0, 1000);
        int lastNameIndex = Random.Range(0, 2000);

        string firstName = "";
        string lastName = "";

        if (!isFemale && File.Exists(maleFirstNamesFile)) {
            firstName = File.ReadLines(maleFirstNamesFile).Skip(nameIndex).Take(1).First();
        }
        if (isFemale && File.Exists(femaleFirstNamesFile)) {
            firstName = File.ReadLines(femaleFirstNamesFile).Skip(nameIndex).Take(1).First();
        }
        if (File.Exists(lastNamesFile)) {
            lastName = File.ReadLines(lastNamesFile).Skip(lastNameIndex).Take(1).First();
        }
        firstName = firstName.Substring(0, 1) + firstName.Substring(1).ToLower();
        lastName = lastName.Substring(0, 1) + lastName.Substring(1).ToLower();
        return firstName + " " + lastName;
    }
}
