using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void UpdateCarState();

public interface IVehicle
{
    public GameObject PlayerControlling { get; set; }
    public Transform PlayerPosition { get;}
    public Transform GettingOutPosition { get;}
    public Transform Transform { get; }
    public int PrefabIndex { get; set; }

    public bool IsThirdPersonCamera { get;}
    public GameObject ThirdPersonCamera { get;}

    public void HandleVehicleMovement(InputData inputData);

    public UpdateCarState OnVehicleEnter { get; set; }
    public UpdateCarState OnVehicleLeave { get; set; }
}
