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
    public Vector3 GettingOutPosition { get { return GetGettingOutPosition(); } }
    [SerializeField] private Transform[] gettingOutPositions;

    public UpdateCarState OnVehicleEnter { get; set; }
    public UpdateCarState OnVehicleLeave { get; set; }

    [SerializeField] private Transform workersTakingPosition;
    private Light[] carLights;

    [SerializeField] private LayerMask gettingOutLayerMask;

    private Renderer carRenderer;

    private CarPhysx carPhysx;
    private bool areLightsOn;

    private void Awake()
    {
        carPhysx = GetComponent<CarPhysx>();
        carRenderer = GetComponentInChildren<Renderer>();
        carLights = GetComponentsInChildren<Light>();
        TurnOffLights();

        OnVehicleEnter += () => {
            carPhysx.isPlayerControlling = true;
            carPhysx.OnVehicleEnter?.Invoke();
            UIManager.possibleActionsUI.AddAction("H - relocate vehicle");
        };
        OnVehicleLeave += () => { 
            carPhysx.isPlayerControlling = false;
            carPhysx.OnVehicleLeave?.Invoke();
            UIManager.possibleActionsUI.RemoveAction("H - relocate vehicle");
            TurnOffLights();
        };

        PlacingTriggerAreaParent placingTriggerAreaParent = GetComponentInChildren<PlacingTriggerAreaParent>();
        if(placingTriggerAreaParent != null) {
            placingTriggerAreaParent.OnProductTriggerEnterEvent += OnProductPlacedInArea;
            placingTriggerAreaParent.OnProductTriggerExitEvent += OnProductTakenFromArea;
            placingTriggerAreaParent.OnContainerTriggerEnterEvent += OnContainerPlacedInArea;
            placingTriggerAreaParent.OnContainerTriggerExitEvent += OnContainerTakenFromArea;
            placingTriggerAreaParent.OnFurnitureBoxTriggerEnterEvent += OnFurnitureBoxPlacedInArea;
            placingTriggerAreaParent.OnFurnitureBoxTriggerExitEvent += OnFurnitureBoxTakenFromArea;
        }
    }

    public void HandleVehicleMovement(InputData inputData)
    {
        carPhysx.HandleVehicleMovement(inputData);

        if (inputData.buttonL) {
            if (areLightsOn)
                TurnOffLights();
            else
                TurnOnLights();
        }
    }

    public void OnVehicleHelpSubmit()
    {
        PlayerData.instance.TakeMoney(500);
        Transform closestHelpPoint = VehicleManager.instance.GetClosestHelpPoint(transform.position);
        transform.position = closestHelpPoint.position;
        transform.rotation = closestHelpPoint.rotation;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        Physics.SyncTransforms();
    }

    private void TurnOffLights()
    {
        areLightsOn = false;
        carRenderer.material.DisableKeyword("_EMISSION");
        foreach (Light light in carLights) {
            light.enabled = false;
        }
    }

    private void TurnOnLights()
    {
        areLightsOn = true;
        carRenderer.material.EnableKeyword("_EMISSION");
        foreach (Light light in carLights) {
            light.enabled = true;
        }
    }

    public void OnProductPlacedInArea(Product product) {
        product.OnVehicleAreaEnter();
    }
    public void OnProductTakenFromArea(Product product) {
        product.OnVehicleAreaExit();
    }
    public void OnContainerPlacedInArea(Container container) {
        container.OnVehicleAreaEnter();
    }
    public void OnContainerTakenFromArea(Container container) {
        container.OnVehicleAreaExit();
    }
    public void OnFurnitureBoxPlacedInArea(FurnitureBox furnitureBox)
    {
        furnitureBox.OnVehicleAreaEnter();
    }
    public void OnFurnitureBoxTakenFromArea(FurnitureBox furnitureBox)
    {
        furnitureBox.OnVehicleAreaExit();
    }

    public Vector3 GetGettingOutPosition()
    {
        Vector3 halfExtents = new Vector3(0.5f, 1.8f, 0.5f) / 2;
        for (int i = 0; i < gettingOutPositions.Length - 1; i++) {
            if(!Physics.CheckBox(gettingOutPositions[i].position + (gettingOutPositions[i].up * halfExtents.y), halfExtents, transform.rotation, gettingOutLayerMask)) {
                return gettingOutPositions[i].position;
            }
        }
        return gettingOutPositions[gettingOutPositions.Length - 1].position;
    }
}
