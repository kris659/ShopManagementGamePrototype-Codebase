using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class WorkersManager : MonoBehaviour
{
    public static WorkersManager instance;
    public List<Worker> workers = new List<Worker>();
    public GameObject workerPrefab;

    private int hirePrice = 100;
    private int workerWage = 10;

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

    private void PayWorkers()
    {
        foreach(Worker worker in workers) {
            PlayerData.instance.TakeMoney(workerWage);
        }
    }


    public void HireWorker()
    {
        if (workers.Count >= 4 || !PlayerData.instance.CanAfford(hirePrice))
            return;

        PlayerData.instance.TakeMoney(hirePrice);
        SpawnWorker(GetNewName(), 0, 10);
    }

    public void ChangeWorkerAction(int workerIndex, WorkerTask workerTask)
    {
        workers[workerIndex].ChangeWorkerAction(workerTask);
    }

    public void GetWorkerData(int workerIndex, out string name, out WorkerTask workerTask, out int wage)
    {
        workers[workerIndex].GetWorkerData(out name, out workerTask, out wage);
    }

    private void SpawnWorker(string name, int workerTask, int wage)
    {
        GameObject workerGO = Instantiate(workerPrefab);
        Worker worker = workerGO.GetComponent<Worker>();
        worker.Init(name, workerTask, wage);
        workers.Add(worker);
    }

    public void FireWorker(Worker worker)
    {
        workers.Remove(worker);
        Destroy(worker.gameObject);
    }

    public void DestroyAll()
    {
        while(workers.Count > 0) {
            FireWorker(workers[0]);
        }
    }

    public WorkerSaveData[] GetSaveData()
    {
        WorkerSaveData[] workersSaveData = new WorkerSaveData[workers.Count];
        for(int i = 0; i < workersSaveData.Length; i++) {
            workers[i].GetWorkerData(out string name, out WorkerTask workerTask, out int wage);
            workersSaveData[i] = new WorkerSaveData(name, (int)workerTask, wage);
        }
        return workersSaveData;
    }
    public void LoadFromSaveData(WorkerSaveData[] workersSaveData)
    {
        for(int i = 0; i < workersSaveData.Length; i++) {
            SpawnWorker(workersSaveData[i].name, workersSaveData[i].task, workersSaveData[i].wage);
        }
    }

    private string GetNewName()
    {
        string firstNamesFile = Path.Combine(Application.streamingAssetsPath, "Workers", "male-first-names.txt");
        string lastNamesFile = Path.Combine(Application.streamingAssetsPath, "Workers", "last-names.txt");

        int nameIndex = Random.Range(0, 1000);
        int lastNameIndex = Random.Range(0, 2000);

        string firstName = "";
        string lastName = "";

        if (File.Exists(firstNamesFile)) {
            firstName = File.ReadLines(firstNamesFile).Skip(nameIndex).Take(1).First();
        }
        if (File.Exists(lastNamesFile)) {
            lastName = File.ReadLines(lastNamesFile).Skip(lastNameIndex).Take(1).First();
        }
        firstName = firstName.Substring(0, 1) + firstName.Substring(1).ToLower();
        lastName = lastName.Substring(0, 1) + lastName.Substring(1).ToLower();
        return firstName + " " + lastName;
    }
}
