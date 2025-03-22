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
        if (MainMenu.isMainMenuOpen) {
            this.enabled = false;
            return;
        }
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
    private void Update()
    {
        if (MainMenu.isMainMenuOpen) {
            this.enabled = false;
            return;
        }
        if (playerInputHandler == null)
            return;

        if (Input.GetKeyDown(KeyCode.H)){
            UIManager.confirmUI.OpenUI("If your vehicle is stuck, you can pay $500 to relocate it. Do you want to do it?", () => vehicle.OnVehicleHelpSubmit(), null, PlayerData.instance.CanAfford(500));
        }
    }
}
