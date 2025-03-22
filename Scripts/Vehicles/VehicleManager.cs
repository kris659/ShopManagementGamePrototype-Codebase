using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class VehicleManager : MonoBehaviour
{
    public static VehicleManager instance;

    [SerializeField] private GameObject[] vehiclesPrefabs = new GameObject[0];
    [SerializeField] private GameObject vehicleCamera;
    [SerializeField] private Transform helpPointsParent;

    public List<IVehicle> vehiclesSpawned = new List<IVehicle>();

    public List<int> startingVehicles = new List<int>();
    public List<Transform> startingVehiclesPoints = new List<Transform>();
    public List<bool> vehiclesUnlocked = new List<bool>();
    public int vehiclesUnlockedCount => vehiclesUnlocked.Count(x => x == true);
    public List<int> vehiclesUnlockPrices= new List<int>();

    public int vehicleToSpawn;
    public bool shouldSpawn = false;

    private void Awake()
    {
        if (instance != null)
            Destroy(this);
        instance = this;
    }


    private void Start()
    {
        for (int i = 0; i < startingVehicles.Count; i++)
            SpawnVehicle(startingVehicles[i], startingVehiclesPoints[i].position, startingVehiclesPoints[i].rotation);
    }

    private void Update()
    {
        //if (shouldSpawn) {
        //    shouldSpawn = false;
        //    SpawnVehicle(startingVehicles[vehicleToSpawn], startingVehiclesPoints[vehicleToSpawn].position, startingVehiclesPoints[vehicleToSpawn].rotation);
        //}
    }

    public void TryToUnlockVehicle(IVehicle vehicle)
    {
        int vehicleIndex = GetVehicleIndex(vehicle);
        string text = "Buy for $" + vehiclesUnlockPrices[vehicleIndex] + "?";
        UIManager.confirmUI.OpenUI(text, () => OnConfirmButtonClicked(vehicleIndex), null, PlayerData.instance.CanAfford(vehiclesUnlockPrices[vehicleIndex]));        
    }

    void OnConfirmButtonClicked(int vehicleIndex)
    {
        if (!PlayerData.instance.CanAfford(vehiclesUnlockPrices[vehicleIndex]))
            return;
        PlayerData.instance.TakeMoney(vehiclesUnlockPrices[vehicleIndex]);
        TasksManager.instance.ProgressTasks(TaskType.UnlockVehicle, 1);
        AudioManager.PlaySound(Sound.PlayerOrder);
        vehiclesUnlocked[vehicleIndex] = true;
    }

    public VehicleSaveData[] GetVehiclesSaveData()
    {
        VehicleSaveData[] vehiclesSaveData = new VehicleSaveData[vehiclesSpawned.Count];
        for(int i = 0; i < vehiclesSaveData.Length; i++) {
            vehiclesSaveData[i] = new VehicleSaveData(vehiclesSpawned[i].Transform.position, vehiclesSpawned[i].Transform.rotation , vehiclesSpawned[i].PrefabIndex, vehiclesUnlocked[i]);
        }
        return vehiclesSaveData;
    }

    public void LoadFromSaveData(VehicleSaveData[] vehiclesSaveData)
    {
        //for (int i = 0; i < startingVehicles.Count; i++)
        //    SpawnVehicle(startingVehicles[i], startingVehiclesPoints[i].position, startingVehiclesPoints[i].rotation);
        for (int i = 0; i < vehiclesSaveData.Length; i++) {
            SpawnVehicle(vehiclesSaveData[i].prefabIndex, vehiclesSaveData[i].position, vehiclesSaveData[i].rotation);
            vehiclesUnlocked[i] = vehiclesSaveData[i].isUnlocked;
        }
    }

    public void SpawnVehicle(int prefabIndex, Vector3 position, Quaternion rotation)
    {
        GameObject vehicle = Instantiate(vehiclesPrefabs[prefabIndex], position, rotation);
        IVehicle vehicleScript = vehicle.GetComponent<IVehicle>();
        vehicleScript.PrefabIndex = prefabIndex;
        vehiclesSpawned.Add(vehicleScript);
    }

    public void DestroyAll()
    {
        for(int i = 0; i < vehiclesSpawned.Count; i++) {
            Destroy(vehiclesSpawned[i].Transform.gameObject);
        }
        vehiclesSpawned.Clear();
        vehicleCamera.transform.SetParent(null);
    }

    public bool IsVehicleUnlocked(IVehicle vehicle)
    {
        int vehicleIndex = GetVehicleIndex(vehicle);
        if(vehicleIndex < 0) {
            Debug.LogWarning("Vehicle not found");
            return false;
        } 
        return vehiclesUnlocked[GetVehicleIndex(vehicle)];
    }

    public int GetVehicleIndex(IVehicle vehicle)
    {
        return vehiclesSpawned.IndexOf(vehicle);
    }

    public Transform GetClosestHelpPoint(Vector3 position)
    {
        Transform closest = null;
        float distance = float.PositiveInfinity;
        for(int i = 0; i < helpPointsParent.childCount; i++) {
            if(Vector3.Distance(position, helpPointsParent.GetChild(i).position) < distance) {
                distance = Vector3.Distance(position, helpPointsParent.GetChild(i).position);
                closest = helpPointsParent.GetChild(i);
            }
        }
        return closest;
    }
}
