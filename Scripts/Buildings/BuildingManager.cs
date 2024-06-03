using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private float gridSize;

    [SerializeField] private LayerMask buildingLayer;
    [SerializeField] private float buildingRange;
    [SerializeField] private Transform originPoint;

    private bool isBuilding;
    private bool isRemovingBuildings = false;


    private GameObject buildingPreview;

    private Vector3 buildingPosition;
    private Quaternion buildingRotation;
    private int rotationIndex = 0;

    private int buildingIndex;
    [SerializeField] private NavMeshSurface surface;
    private IBuildableSO buildableSO;

    private Vector3 defaultBuildingPosition = new Vector3(0, -10, 0);

    private void Awake()
    {
        SceneLoader.OnUISceneLoaded += () => UIManager.buildingUI.Init(this);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B)){
            CancelBuilding();
            isRemovingBuildings = false;
        }

        if (isBuilding && (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1)))
            CancelBuilding();
        if (isRemovingBuildings && (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1)))
            CancelRemoving();    
        if (isBuilding && Input.GetKeyDown(KeyCode.R)) {
            Rotate();
        }

        if (isBuilding){
            MoveBuildingPreview();
        }
        if(isBuilding && Input.GetMouseButtonDown(0))
            PlaceBuilding();
        if(isRemovingBuildings && Input.GetMouseButtonDown(0)) {
            TryRemoveBuilding();
        }
    }

    public void SelectBuilding(int buildingTypeIndex, int buildingIndex)
    {
        CancelBuilding();
        buildableSO = SOData.GetBuildableSO(buildingTypeIndex, buildingIndex);
        buildingPreview = Instantiate(buildableSO.PreviewPrefab);
        isBuilding = true;
        this.buildingIndex = buildingIndex;
    }
    private void Rotate()
    {
        rotationIndex++;
        rotationIndex %= 4;

        buildingRotation = Quaternion.Euler(new Vector3(0,90,0) * rotationIndex);
    }

    private bool CanBuildHere()
    {        
        return buildingPosition != defaultBuildingPosition;
    }

    private Vector3 GetBuildingPosition()
    {
        Vector3 origin = playerCamera.transform.position;
        Vector3 direction = playerCamera.transform.TransformDirection(Vector3.forward);
        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, buildingRange, buildingLayer)){
            Vector3 position = hit.point - originPoint.transform.position;

            if(position.x >= 0) {
                if (position.x % gridSize <= gridSize / 2)
                    position.x -= position.x % gridSize;
                else
                    position.x += gridSize - position.x % gridSize;
            }
            else {
                if (Mathf.Abs(position.x) % gridSize <= gridSize / 2)
                    position.x += Mathf.Abs(position.x) % gridSize;
                else
                    position.x -= gridSize - Mathf.Abs(position.x) % gridSize;
            }
            

            if (position.z % gridSize <= gridSize / 2)
                position.z -= position.z % gridSize;
            else
                position.z += gridSize - position.z % gridSize;
            Debug.Log(hit.point.x - originPoint.transform.position.x + "   " + position.x);
            return position + originPoint.transform.position;
        }
        return defaultBuildingPosition;
    }

    private void MoveBuildingPreview()
    {
        buildingPosition = GetBuildingPosition();
        buildingPreview.transform.position = buildingPosition;
        buildingPreview.transform.rotation = buildingRotation;
    }

    private void PlaceBuilding()
    {
        if (!CanBuildHere())
            return;
        if (!PlayerData.instance.CanAfford(buildableSO.Price)){
            UIManager.textUI.UpdateText("Can't afford", 3f);
            return;
        }
        PlayerData.instance.TakeMoney(buildableSO.Price);
        IBuildable buildable = buildingPreview.GetComponent<IBuildable>();
        if(!buildable.CanBuildHere(buildingPosition, buildingRotation)) 
            return;
        buildable.Build(buildingIndex, buildingPosition, buildingRotation);

        surface.BuildNavMesh();
    }

    private void CancelBuilding()
    {
        if(buildingPreview != null)
            Destroy(buildingPreview);
        isBuilding = false;
        buildingIndex = 0;
        UIManager.textUI.UpdateText(string.Empty);
    }

    private void CancelRemoving()
    {
        isRemovingBuildings = false;
        UIManager.textUI.UpdateText("");
    }

    private void TryRemoveBuilding()
    {
        Vector3 origin = playerCamera.transform.position;
        Vector3 direction = playerCamera.transform.TransformDirection(Vector3.forward);

        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, buildingRange)) {
            if (hit.transform.parent && hit.transform.parent.TryGetComponent(out IBuildable building)){
                building.Destroy();
                surface.BuildNavMesh();
            }
            else {
                if (hit.transform.TryGetComponent(out building)) {
                    building.Destroy();
                    surface.BuildNavMesh();
                }
            }
        }
    }

    public void OnDestroyButtonClicked()
    {
        isRemovingBuildings = true;
        if (isRemovingBuildings) {
            UIManager.textUI.UpdateText("Destroy mode (right click to cancel)");
        }        
    }
}
