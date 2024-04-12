using Cinemachine;
using UnityEngine;

public class PlayerInteractions : MonoBehaviour
{
    public static PlayerInteractions Instance;

    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private GameObject vehicleFPSCamera;
    [SerializeField] private GameObject playerRigidbody;
    FirstPersonController firstPersonController;
    PlayerVehicleController vehicleController;

    [SerializeField] private float playerCarInteractionRange;
    [SerializeField] private LayerMask carLayerMask;

    [SerializeField] private float playerInteractionRange;
    [SerializeField] private LayerMask interactionsLayerMask;

    bool isDriving = false;
    IVehicle vehicle;

    private CinemachineVirtualCamera playerVirtualCamera;
    private CinemachineVirtualCamera vehicleVirtualCamera;
    private CinemachinePOV playerVirtualCameraPOV;
    private CinemachinePOV vehicleVirtualCameraPOV;

    private float cameraHorizontalSpeed;
    private float cameraVerticalSpeed;

    private bool isCameraLockedForUI = false;

    private int possibleInteractionsCounter = 0;

    private string previousActionText;

    private void Awake()
    {
        if (Instance != null)
            Destroy(this);
        else
            Instance = this;
    }

    void Start()
    {
        firstPersonController = GetComponent<FirstPersonController>();
        vehicleController = GetComponent<PlayerVehicleController>();

        playerVirtualCamera = playerCamera.GetComponent<CinemachineVirtualCamera>();
        vehicleVirtualCamera = vehicleFPSCamera.GetComponent<CinemachineVirtualCamera>();
        playerVirtualCameraPOV = playerVirtualCamera.GetCinemachineComponent<CinemachinePOV>();
        vehicleVirtualCameraPOV = vehicleVirtualCamera.GetCinemachineComponent<CinemachinePOV>();

        cameraVerticalSpeed = playerVirtualCameraPOV.m_VerticalAxis.m_MaxSpeed;
        cameraHorizontalSpeed = playerVirtualCameraPOV.m_HorizontalAxis.m_MaxSpeed;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
            HandleInteractions();
        if (Input.GetKeyDown(KeyCode.LeftControl)){
            Cursor.lockState = CursorLockMode.None;
            LockCamera();
        }
        if (Input.GetKeyUp(KeyCode.LeftControl) && !isCameraLockedForUI) {
            Cursor.lockState = CursorLockMode.Locked;
            UnlockCamera();
        }
        if(isDriving && Input.GetKeyDown(KeyCode.V)){
            Handle3rdPearsonCamera();
        }
        HandlePossibleInteractions();
    }

    private void HandleInteractions()
    {
        if (isDriving) {
            GetOutOfVehicle();
        }
        else {
            IInteractable interactable = Raycast<IInteractable>(playerInteractionRange, interactionsLayerMask, true);
            if(interactable != null) {
                interactable.OnPlayerInteract();
                HandlePossibleInteractions();
                return;
            }
            IVehicle vehicle = Raycast<IVehicle>(playerCarInteractionRange, carLayerMask, false);
            if (vehicle != null) {
                GetInVehicle(vehicle);
            }
        }
    }

    private T Raycast<T>(float interactionRange, LayerMask layerMask, bool checkCollider)
    {
        Vector3 origin = mainCamera.transform.position;
        Vector3 direction = mainCamera.transform.TransformDirection(Vector3.forward);
        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, interactionRange, layerMask)) {
            if (checkCollider) {
                if (hit.collider.transform.TryGetComponent(out T component))
                    return component;
            }
            else {
                if (hit.transform.TryGetComponent(out T component))
                    return component;
            }
        }
        return default;
    }

    public void GetInVehicle(IVehicle vehicle)
    {
        UIManager.possibleActionsUI.RemoveAction("E - enter the vehicle");
        UIManager.possibleActionsUI.AddAction("E - leave the vehicle");
        UIManager.possibleActionsUI.AddAction("V - change camera");

        isDriving = true;
        this.vehicle = vehicle;
        vehicleController.enabled = true;
        vehicleController.Init(vehicle.Transform.gameObject, vehicleFPSCamera);
        vehicle.OnVehicleEnter?.Invoke();
        vehicleVirtualCameraPOV.m_HorizontalAxis.Value = 0;
        vehicleVirtualCameraPOV.m_VerticalAxis.Value = 0;

        vehicleFPSCamera.SetActive(true);
        playerCamera.SetActive(false);
        firstPersonController.enabled = false;
        playerRigidbody.SetActive(false);
    }

    private void GetOutOfVehicle()
    {
        UIManager.possibleActionsUI.RemoveAction("E - leave the vehicle");
        UIManager.possibleActionsUI.RemoveAction("V - change camera");
        isDriving = false;
        vehicleController.enabled = false;
        firstPersonController.enabled = true;
        transform.localScale = Vector3.one;
        vehicle.OnVehicleLeave?.Invoke();
        playerRigidbody.transform.position = vehicle.GettingOutPosition.position;
        playerRigidbody.SetActive(true);
        playerCamera.transform.rotation = vehicleFPSCamera.transform.rotation;

        playerVirtualCameraPOV.m_HorizontalAxis = vehicleVirtualCameraPOV.m_HorizontalAxis;
        playerVirtualCameraPOV.m_VerticalAxis = vehicleVirtualCameraPOV.m_VerticalAxis;

        playerCamera.SetActive(true);
        vehicleFPSCamera.SetActive(false);

        if(vehicle != null && vehicle.ThirdPersonCamera != null)
            vehicle.ThirdPersonCamera.SetActive(false);
    }

    public int GetVehicleIndex()
    {
        if (!isDriving)
            return -1;
        return VehicleManager.instance.vehiclesSpawned.IndexOf(vehicle);
    }

    public void SetPlayerPosition(Vector3 position)
    {
        playerRigidbody.transform.position = position;
    }
    public Vector3 GetPlayerPosition()
    {
        return playerRigidbody.transform.position;
    }

    private void Handle3rdPearsonCamera()
    {
        GameObject camera = vehicle.ThirdPersonCamera;
        camera.SetActive(!camera.activeSelf);
        vehicleFPSCamera.SetActive(!camera.activeSelf);
    }

    public void LockCameraForUI()
    {
        isCameraLockedForUI = true;
        LockCamera();
    }

    public void UnlockCameraForUI()
    {
        isCameraLockedForUI = false;
        UnlockCamera();
    }

    private void LockCamera()
    {
        playerVirtualCameraPOV.m_VerticalAxis.m_MaxSpeed = 0;
        playerVirtualCameraPOV.m_HorizontalAxis.m_MaxSpeed = 0;
        vehicleVirtualCameraPOV.m_VerticalAxis.m_MaxSpeed = 0;
        vehicleVirtualCameraPOV.m_HorizontalAxis.m_MaxSpeed = 0;

        if(vehicle != null && vehicle.ThirdPersonCamera != null) {

        }
    }
    private void UnlockCamera()
    {
        if (playerVirtualCamera == null)
            return;
        playerVirtualCameraPOV.m_VerticalAxis.m_MaxSpeed = cameraVerticalSpeed;
        playerVirtualCameraPOV.m_HorizontalAxis.m_MaxSpeed = cameraHorizontalSpeed;
        vehicleVirtualCameraPOV.m_VerticalAxis.m_MaxSpeed = cameraVerticalSpeed;
        vehicleVirtualCameraPOV.m_HorizontalAxis.m_MaxSpeed = cameraHorizontalSpeed;

        if (vehicle != null && vehicle.ThirdPersonCamera != null) {

        }
    }

    private void HandlePossibleInteractions()
    {
        if(isDriving) 
            return;
        possibleInteractionsCounter++;
        if (possibleInteractionsCounter != 6)
            return;
        
        possibleInteractionsCounter = 0;
        IInteractable interactable = Raycast<IInteractable>(playerInteractionRange, interactionsLayerMask, true);
        UIManager.possibleActionsUI.RemoveAction(previousActionText);
        UIManager.possibleActionsUI.RemoveAction("E - enter the vehicle");
        if (interactable != null) {

            previousActionText = interactable.textToDisplay;
            UIManager.possibleActionsUI.AddAction(previousActionText);
        }
        else {
            IVehicle vehicle = Raycast<IVehicle>(playerCarInteractionRange, carLayerMask, false);
            if (vehicle != null) {
                UIManager.possibleActionsUI.AddAction("E - enter the vehicle");
            }
        }
    }
}
