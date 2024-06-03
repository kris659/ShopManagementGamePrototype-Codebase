using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour, IVehicle
{
    public int PrefabIndex { get; set; }

    public GameObject ThirdPersonCamera { get { return thirdPersonCamera; } }
    [SerializeField] private GameObject thirdPersonCamera;

    public Transform Transform { get { return transform; } }
    public Transform PlayerPosition { get { return playerPosition; } }
    [SerializeField] private Transform playerPosition;
    public Transform GettingOutPosition { get { return gettingOutPosition; } }
    [SerializeField] private Transform gettingOutPosition;

    public UpdateCarState OnVehicleEnter { get; set; }
    public UpdateCarState OnVehicleLeave { get; set; }

    private CarPhysx carPhysx;

    private void Awake()
    {
        carPhysx = GetComponent<CarPhysx>();
        OnVehicleEnter += () => { carPhysx.isPlayerControlling = true; };
        OnVehicleEnter += () => { carPhysx.OnVehicleEnter?.Invoke(); };
        OnVehicleLeave += () => { carPhysx.isPlayerControlling = false; };
        OnVehicleLeave += () => { carPhysx.OnVehicleLeave?.Invoke(); };
    }

    public void HandleVehicleMovement(InputData inputData)
    {
        carPhysx.HandleVehicleMovement(inputData);
    }
}
