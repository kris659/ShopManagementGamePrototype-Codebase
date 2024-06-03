using Cinemachine;
using System.Collections;
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
    private CinemachineFreeLook vehicleThirdPersonCamera;
    private CinemachinePOV playerVirtualCameraPOV;
    private CinemachinePOV vehicleVirtualCameraPOV;

    private float mouseSensitivity;

    private float cameraHorizontalSpeed;
    private float cameraVerticalSpeed;

    private float vehicleThirdPersonHorizontalSpeed;
    private float vehicleThirdPersonVerticalSpeed;

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
        SettingsUI.OnMouseSensitivityChanged += OnMouseSensitivityChanged;
        mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 0.5f) + 0.5f;

        firstPersonController = GetComponent<FirstPersonController>();
        vehicleController = GetComponent<PlayerVehicleController>();

        playerVirtualCamera = playerCamera.GetComponent<CinemachineVirtualCamera>();
        vehicleVirtualCamera = vehicleFPSCamera.GetComponent<CinemachineVirtualCamera>();
        playerVirtualCameraPOV = playerVirtualCamera.GetCinemachineComponent<CinemachinePOV>();
        vehicleVirtualCameraPOV = vehicleVirtualCamera.GetCinemachineComponent<CinemachinePOV>();

        cameraVerticalSpeed = playerVirtualCameraPOV.m_VerticalAxis.m_MaxSpeed;
        cameraHorizontalSpeed = playerVirtualCameraPOV.m_HorizontalAxis.m_MaxSpeed;

        playerVirtualCameraPOV.m_VerticalAxis.m_MaxSpeed = cameraVerticalSpeed * mouseSensitivity;
        playerVirtualCameraPOV.m_HorizontalAxis.m_MaxSpeed = cameraHorizontalSpeed * mouseSensitivity;

        vehicleVirtualCameraPOV.m_VerticalAxis.m_MaxSpeed = cameraVerticalSpeed * mouseSensitivity;
        vehicleVirtualCameraPOV.m_HorizontalAxis.m_MaxSpeed = cameraHorizontalSpeed * mouseSensitivity;
    }

    void Update()
    {
        if (UIManager.BlockInput)
            return;
        if(Input.GetKeyDown(KeyCode.E))
            HandleInteractions();
        if (Input.GetKeyDown(KeyCode.F))
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
        //if (isDriving) {
        //    GetOutOfVehicle();
        //}
        //else {
        //    IInteractable interactable = Raycast<IInteractable>(playerInteractionRange, interactionsLayerMask, true);
        //    if(interactable != null) {
        //        interactable.OnPlayerInteract();
        //        HandlePossibleInteractions();
        //        return;
        //    }
        //    IVehicle vehicle = Raycast<IVehicle>(playerCarInteractionRange, carLayerMask, false);
        //    if (vehicle != null) {
        //        if (VehicleManager.instance.IsVehicleUnlocked(vehicle))
        //            GetInVehicle(vehicle);
        //        else {
        //            VehicleManager.instance.TryToUnlockVehicle(vehicle);
        //        }
        //    }
        //}
        if (Input.GetKeyDown(KeyCode.E)) {
            IInteractable interactable = Raycast<IInteractable>(playerInteractionRange, interactionsLayerMask, true);
            if (interactable != null) {
                interactable.OnPlayerInteract();
                HandlePossibleInteractions();
                return;
            }
        }
        if(Input.GetKeyDown(KeyCode.F)) {
            if (isDriving) {
                GetOutOfVehicle();
                return;
            }
            IVehicle vehicle = Raycast<IVehicle>(playerCarInteractionRange, carLayerMask, false);
            if (vehicle != null) {
                if (VehicleManager.instance.IsVehicleUnlocked(vehicle))
                    GetInVehicle(vehicle);
                else {
                    VehicleManager.instance.TryToUnlockVehicle(vehicle);
                }
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
        UIManager.possibleActionsUI.RemoveAction("F - enter the vehicle");
        UIManager.possibleActionsUI.AddAction("F - leave the vehicle");
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
        mainCamera.transform.GetChild(0).gameObject.SetActive(false);

        vehicleThirdPersonCamera = vehicle.ThirdPersonCamera.GetComponentInChildren<CinemachineFreeLook>();

        vehicleThirdPersonHorizontalSpeed = vehicleThirdPersonCamera.m_XAxis.m_MaxSpeed;
        vehicleThirdPersonVerticalSpeed = vehicleThirdPersonCamera.m_YAxis.m_MaxSpeed;
        vehicleThirdPersonCamera.m_XAxis.m_MaxSpeed = vehicleThirdPersonHorizontalSpeed * mouseSensitivity;
        vehicleThirdPersonCamera.m_YAxis.m_MaxSpeed = vehicleThirdPersonVerticalSpeed * mouseSensitivity;

        StartCoroutine(FixGetInVehicle());
    }

    IEnumerator FixGetInVehicle()
    {
        yield return new WaitForSeconds(0.1f);
        if (isDriving) {
            vehicleController.enabled = true;
            firstPersonController.enabled = false;
        }
    }

    private void GetOutOfVehicle()
    {
        UIManager.possibleActionsUI.RemoveAction("F - leave the vehicle");
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

        vehicleThirdPersonCamera.m_XAxis.m_MaxSpeed = vehicleThirdPersonHorizontalSpeed;
        vehicleThirdPersonCamera.m_YAxis.m_MaxSpeed = vehicleThirdPersonVerticalSpeed;

        playerCamera.SetActive(true);
        vehicleFPSCamera.SetActive(false);
        mainCamera.transform.GetChild(0).gameObject.SetActive(true);

        if (vehicle != null && vehicle.ThirdPersonCamera != null)
            vehicle.ThirdPersonCamera.SetActive(false);
        vehicle = null;
    }

    public int GetVehicleIndex()
    {
        return VehicleManager.instance.GetVehicleIndex(vehicle);
    }

    public void SetPlayerPosition(Vector3 position)
    {
        playerRigidbody.transform.position = position;
        Physics.SyncTransforms();
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

        if(vehicle != null && vehicle.ThirdPersonCamera.activeSelf) {            
            vehicleThirdPersonCamera.m_XAxis.m_MaxSpeed = 0;
            vehicleThirdPersonCamera.m_YAxis.m_MaxSpeed = 0;
        }
    }
    private void UnlockCamera()
    {
        if (playerVirtualCamera == null)
            return;
        playerVirtualCameraPOV.m_VerticalAxis.m_MaxSpeed = cameraVerticalSpeed * mouseSensitivity;
        playerVirtualCameraPOV.m_HorizontalAxis.m_MaxSpeed = cameraHorizontalSpeed * mouseSensitivity;
        vehicleVirtualCameraPOV.m_VerticalAxis.m_MaxSpeed = cameraVerticalSpeed * mouseSensitivity;
        vehicleVirtualCameraPOV.m_HorizontalAxis.m_MaxSpeed = cameraHorizontalSpeed * mouseSensitivity;

        if (vehicle != null && vehicle.ThirdPersonCamera.activeSelf) {
            vehicleThirdPersonCamera.m_XAxis.m_MaxSpeed = vehicleThirdPersonHorizontalSpeed * mouseSensitivity;
            vehicleThirdPersonCamera.m_YAxis.m_MaxSpeed = vehicleThirdPersonVerticalSpeed * mouseSensitivity;
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
        UIManager.possibleActionsUI.RemoveAction("F - enter the vehicle"); 
        UIManager.possibleActionsUI.RemoveAction("F - unlock the vehicle");
        if (interactable != null) {
            previousActionText = interactable.textToDisplay;
            UIManager.possibleActionsUI.AddAction(previousActionText);
        }
        else {
            IVehicle vehicle = Raycast<IVehicle>(playerCarInteractionRange, carLayerMask, false);
            if (vehicle != null) {
                if(VehicleManager.instance.IsVehicleUnlocked(vehicle))
                    UIManager.possibleActionsUI.AddAction("F - enter the vehicle");
                else {
                    UIManager.possibleActionsUI.AddAction("F - unlock the vehicle");
                }
            }
        }
    }

    private void OnMouseSensitivityChanged(float value)
    {
        mouseSensitivity = value + 0.5f;

        playerVirtualCameraPOV.m_VerticalAxis.m_MaxSpeed = cameraVerticalSpeed * mouseSensitivity;
        playerVirtualCameraPOV.m_HorizontalAxis.m_MaxSpeed = cameraHorizontalSpeed * mouseSensitivity;
        vehicleVirtualCameraPOV.m_VerticalAxis.m_MaxSpeed = cameraVerticalSpeed * mouseSensitivity;
        vehicleVirtualCameraPOV.m_HorizontalAxis.m_MaxSpeed = cameraHorizontalSpeed * mouseSensitivity;

        if (vehicle != null && vehicle.ThirdPersonCamera.activeSelf) {
            vehicleThirdPersonCamera.m_XAxis.m_MaxSpeed = vehicleThirdPersonHorizontalSpeed * mouseSensitivity;
            vehicleThirdPersonCamera.m_YAxis.m_MaxSpeed = vehicleThirdPersonVerticalSpeed * mouseSensitivity;
        }
    }
}
