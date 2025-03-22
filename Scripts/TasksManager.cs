using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum TaskType
{
    EarnMoney,
    SpendMoney,
    SellProducts,
    ServeCustomers,
    HaveDifferentProducts,
    HaveProductsInWarehouse,
    OrderBoxesAtOnce,
    HaveEmployees,
    UnlockVehicle,
    UnlockLand,
    ThrowAwayBoxes,
    SellProductsOfType
}

public class TasksManager : MonoBehaviour
{
    [System.Serializable]
    public struct TaskStartingData {
        public TaskType taskType;
        public int amount;
    }
    [System.Serializable]
    public struct TaskTier
    {
        public string name;
        public List<TaskStartingData> tasks;
        public int reward;
    }
    
    [System.Serializable]
    public struct TaskData
    {
        public int tier;
        public TaskType taskType;

        public string description;
        public int reward;
        public int amount;
        public int[] additionalInfo;
        public int progress;
        public bool wasRewardClaimed;

        public TaskData(int tier, TaskType taskType, string description, int reward, int amount, int[] additionalInfo, int progress = 0, bool isRewardClaimed = false)
        {
            this.taskType = taskType;
            this.tier = tier;
            this.description = description;
            this.reward = reward;
            this.amount = amount;
            this.additionalInfo = additionalInfo;
            this.progress = progress;
            this.wasRewardClaimed = isRewardClaimed;
        }

        public TaskData(TaskSaveData taskSaveData)
        {
            this.taskType = taskSaveData.taskType;
            this.tier = taskSaveData.tier;
            this.additionalInfo = taskSaveData.additionalInfo;
            this.progress = taskSaveData.progress;
            this.wasRewardClaimed = taskSaveData.wasRewardTaken;

            description = instance.tasksDescriptions[(int)taskType];
            if (taskType == TaskType.SellProductsOfType) {
                if (!ProductsData.instance.IsProductsUnlocked(additionalInfo[0])) {
                    additionalInfo[0] = ProductsData.instance.GetRandomProduct();
                }
                description = "Sell " + SOData.productsList[additionalInfo[0]].Name;
            }

            reward = instance.taskTiers[tier].reward;
            this.amount = 0;
            List<TaskStartingData> tasks = instance.taskTiers[tier].tasks;
            for (int i = 0; i < tasks.Count; i++) {
                if (tasks[i].taskType == taskType) {
                    this.amount = tasks[i].amount;
                }
            }
        }

        public bool ShouldBeIgnored => amount == progress && wasRewardClaimed;
    }

    private List<string> tasksDescriptions = new List<string> {
        "Earn money", "Spend money", "Sell products", "Serve customers (without using cashier)", "Have different types of products on shelves",
        "Have products on warehouse shelves (in boxes)", "Order boxes at once", "Hire employees", "Unlock vehicles (forklift or truck)",
        "Unlock land", "Throw away empty boxes", "Sell {ProductName}"
    };

    public static TasksManager instance; 
    public List<TaskTier> taskTiers = new List<TaskTier>();

    [SerializeField] private List<Vector2Int> tasksAlreadyUsed; // x - task tier y - task type enum to int
    [SerializeField] private TaskData[] currentTasksList;

    // Only for testing
    [SerializeField] private bool completeTask0;
    [SerializeField] private bool completeTask1;
    [SerializeField] private bool completeTask2;

    private void Awake()
    {
        if(instance != null) {
            Debug.LogError("Multiple task managers");
            Destroy(this);
            return;
        }
        instance = this;

        //Debug.Log("Test: " + (new Vector2Int(2 ,15) == new Vector2Int(2, 15)));
    }

    private void Update()
    {
        if (completeTask0) {
            completeTask0 = false;
            currentTasksList[0].progress = currentTasksList[0].amount;
            if (UIManager.tasksUI.isOpen)
                UIManager.tasksUI.UpdateUI();
        }
        if (completeTask1) {
            completeTask1 = false;
            currentTasksList[1].progress = currentTasksList[1].amount;
            if (UIManager.tasksUI.isOpen)
                UIManager.tasksUI.UpdateUI();
        }
        if (completeTask2) {
            completeTask2 = false;
            currentTasksList[2].progress = currentTasksList[2].amount;
            if (UIManager.tasksUI.isOpen)
                UIManager.tasksUI.UpdateUI();
        }
    }

    public TaskData GetTaskData(int taskIndex)
    {
        return currentTasksList[taskIndex];
    }

    private void GetNewTask(int taskIndex, int taskTier)
    {
        if (taskTier >= taskTiers.Count)
            return;
        List<TaskStartingData> possibleTasks = new List<TaskStartingData>(taskTiers[taskTier].tasks);
        for(int i = 0; i < possibleTasks.Count; i++) {
            if (possibleTasks[i].amount == 0 || tasksAlreadyUsed.Contains(new Vector2Int(taskTier, (int)possibleTasks[i].taskType))) {
                possibleTasks.RemoveAt(i--);
            }
        }
        int selectedTaskIndex = Random.Range(0, possibleTasks.Count);
        tasksAlreadyUsed.Add(new Vector2Int(taskTier, (int)possibleTasks[selectedTaskIndex].taskType));

        TaskType taskType = possibleTasks[selectedTaskIndex].taskType;
        string description = tasksDescriptions[(int)possibleTasks[selectedTaskIndex].taskType];
        int[] additionalInfo = new int[0];

        if(taskType == TaskType.SellProductsOfType) {
            additionalInfo = new int[1];
            additionalInfo[0] = ProductsData.instance.GetRandomProduct();
            description = "Sell " + SOData.productsList[additionalInfo[0]].Name;
        }        
        currentTasksList[taskIndex] = new TaskData(taskTier, taskType, description, taskTiers[taskTier].reward, possibleTasks[selectedTaskIndex].amount, additionalInfo);
        SetStartingProgress(taskIndex);
    }

    private void SetStartingProgress(int taskIndex)
    {
        switch (currentTasksList[taskIndex].taskType) {
            case TaskType.HaveDifferentProducts:
                currentTasksList[taskIndex].progress = ProductsData.instance.differentProductTypesOnShelvesCount;
                break;
            case TaskType.UnlockLand:
                currentTasksList[taskIndex].progress = ShopData.instance.GetUnlockedLandsCount();
                break;
            case TaskType.UnlockVehicle:
                currentTasksList[taskIndex].progress = VehicleManager.instance.vehiclesUnlockedCount;
                break;
            case TaskType.HaveProductsInWarehouse:
                currentTasksList[taskIndex].progress = StatsManager.instance.productsOnWarehouseShelves;
                break;
            case TaskType.HaveEmployees:
                currentTasksList[taskIndex].progress = WorkersManager.instance.workers.Count;
                break;
            default:
                break;
        }
        currentTasksList[taskIndex].progress = Mathf.Min(currentTasksList[taskIndex].amount, currentTasksList[taskIndex].progress);
    }

    public void ProgressTasks(TaskType taskType, int amount)
    {
        ProgressTasks(taskType, amount, new int[0]);
    }
    public void ProgressTasks(TaskType taskType, int progress, int[] additionalInfo)
    {
        for(int i = 0; i < currentTasksList.Length; i++) {
            if (currentTasksList[i].taskType == taskType && currentTasksList[i].additionalInfo.SequenceEqual(additionalInfo) && currentTasksList[i].progress != currentTasksList[i].amount) {
                switch (currentTasksList[i].taskType) {
                    case TaskType.OrderBoxesAtOnce:
                        currentTasksList[i].progress = Mathf.Min(currentTasksList[i].amount, Mathf.Max(currentTasksList[i].progress, progress));
                        break;
                    case TaskType.HaveDifferentProducts:
                        currentTasksList[i].progress = Mathf.Min(currentTasksList[i].amount, progress);
                        break;
                    default:
                        currentTasksList[i].progress = Mathf.Min(currentTasksList[i].amount, currentTasksList[i].progress + progress);
                        break;
                }
                if(currentTasksList[i].progress == currentTasksList[i].amount) {
                    AudioManager.PlaySound(Sound.PositiveNotification);
                    UIManager.tasksUI.ShowExclamationMark();
                }                    
            }
        }
        if(UIManager.tasksUI.isOpen)
            UIManager.tasksUI.UpdateUI();
    }

    public void OnClaimRewardButton(int taskIndex)
    {
        PlayerData.instance.AddMoney(currentTasksList[taskIndex].reward, false);
        currentTasksList[taskIndex].wasRewardClaimed = true;
        int nextTier = currentTasksList[taskIndex].tier + 1;
        GetNewTask(taskIndex, nextTier);
        UIManager.tasksUI.UpdateUI();
        AudioManager.PlaySound(Sound.RewardClaim);

        UIManager.tasksUI.HideExclamationMark();
        for (int i = 0; i < currentTasksList.Length; i++) {
            if (currentTasksList[i].progress == currentTasksList[i].amount) {
                UIManager.tasksUI.ShowExclamationMark();
            }
        }
    }

    private void CreateStartingTasks()
    {
        tasksAlreadyUsed.Clear();
        currentTasksList = new TaskData[3];
        GetNewTask(0, 0);
        GetNewTask(1, 0);
        GetNewTask(2, 0);
    }

    public void LoadFromSaveData(TasksSaveData taskSaveData)
    {
        if(taskSaveData.tasksAlreadyUsed.Length == 0) {
            CreateStartingTasks();
            return;
        }
        tasksAlreadyUsed = taskSaveData.tasksAlreadyUsed.ToList();
        currentTasksList = new TaskData[taskSaveData.currentTasksList.Length];
        for (int i = 0; i < taskSaveData.currentTasksList.Length; i++) {
            currentTasksList[i] = new TaskData(taskSaveData.currentTasksList[i]);
        }

        UIManager.tasksUI.HideExclamationMark();
        for (int i = 0; i < currentTasksList.Length; i++) {
            if (currentTasksList[i].progress == currentTasksList[i].amount) {
                UIManager.tasksUI.ShowExclamationMark();
            }
        }
    }

    public TasksSaveData GetSaveData()
    {
        TaskSaveData[] tasksSaveData = new TaskSaveData[currentTasksList.Length];
        for(int i = 0; i < tasksSaveData.Length; i++) {
            tasksSaveData[i] = new TaskSaveData(currentTasksList[i].tier, currentTasksList[i].taskType, currentTasksList[i].progress, currentTasksList[i].additionalInfo, currentTasksList[i].wasRewardClaimed);
        } 
        return new TasksSaveData(tasksAlreadyUsed.ToArray(), tasksSaveData);
    }
}
