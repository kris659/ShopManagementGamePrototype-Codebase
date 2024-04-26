using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleManager : MonoBehaviour
{
    public static VehicleManager instance;

    [SerializeField] private GameObject[] vehiclesPrefabs = new GameObject[0];
    [SerializeField] private GameObject vehicleCamera;

    public List<IVehicle> vehiclesSpawned = new List<IVehicle>();

    public List<int> startingVehicles = new List<int>();
    public List<Transform> startingVehiclesPoints = new List<Transform>();

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
        if (shouldSpawn) {
            shouldSpawn = false;
            SpawnVehicle(startingVehicles[vehicleToSpawn], startingVehiclesPoints[vehicleToSpawn].position, startingVehiclesPoints[vehicleToSpawn].rotation);
        }
    }

    public VehicleSaveData[] GetVehiclesSaveData()
    {
        VehicleSaveData[] vehiclesSaveData = new VehicleSaveData[vehiclesSpawned.Count];
        for(int i = 0; i < vehiclesSaveData.Length; i++) {
            vehiclesSaveData[i] = new VehicleSaveData(vehiclesSpawned[i].Transform.position, vehiclesSpawned[i].Transform.rotation , vehiclesSpawned[i].PrefabIndex);
        }
        return vehiclesSaveData;
    }

    public void LoadFromSaveData(VehicleSaveData[] vehiclesSaveData)
    {
        for(int i = 0;i < vehiclesSaveData.Length; i++) {
            SpawnVehicle(vehiclesSaveData[i].prefabIndex, vehiclesSaveData[i].position, vehiclesSaveData[i].rotation);
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
}
