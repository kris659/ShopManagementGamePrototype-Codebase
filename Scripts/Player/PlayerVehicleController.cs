using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVehicleController : MonoBehaviour
{
    IVehicle vehicle { get; set; }

    PlayerInputHandler playerInputHandler;
    InputData inputData;

    public void Init(GameObject vehicleObject, GameObject vehicleFPSCamera)
    {
        IVehicle vehicle = vehicleObject.GetComponent<IVehicle>();
        this.vehicle = vehicle;

        vehicleFPSCamera.transform.SetParent(vehicle.PlayerPosition);
        vehicleFPSCamera.transform.position = vehicle.PlayerPosition.position;

        playerInputHandler = GetComponent<PlayerInputHandler>();
    }

    private void FixedUpdate()
    {
        if (playerInputHandler == null)
            return;

        inputData = playerInputHandler.GetPlayerInput();
        vehicle.HandleVehicleMovement(inputData);

        if (inputData.buttonR)
        {
            GetComponent<FirstPersonController>().enabled = true;
            this.enabled = false;
        }            
    }  
}
