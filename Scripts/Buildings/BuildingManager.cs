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

    private bool isBuilding;
    private bool isBuildingWall;
    private bool isRemovingBuildings = false;


    private GameObject buildingPreview;

    private Vector3 buildingPosition;
    private Quaternion buildingRotation;
    private int rotationIndex = 0;

    private int buildingIndex;
    [SerializeField] private NavMeshSurface surface;
    private IBuildableSO buildableSO;

    private void Start()
    {
        UIManager.buildingUI.Init(this);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B)){
            CancelBuilding();
            isRemovingBuildings = false;
            if (UIManager.buildingUI.isOpen)
                UIManager.buildingUI.CloseUI();
            else
                UIManager.buildingUI.OpenUI();
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
        if(Input.GetKeyDown (KeyCode.K)) {
            isRemovingBuildings = !isRemovingBuildings;
        }
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

        return true;
    }

    private Vector3 GetBuildingPosition()
    {
        Vector3 origin = playerCamera.transform.position;
        Vector3 direction = playerCamera.transform.TransformDirection(Vector3.forward);
        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, buildingRange, buildingLayer)){
            Vector3 position = hit.point;
            if (position.x % gridSize <= gridSize / 2)
                position.x -= position.x % gridSize;
            else
                position.x += gridSize - position.x % gridSize;
            if (position.z % gridSize <= gridSize / 2)
                position.z -= position.z % gridSize;
            else
                position.z += gridSize - position.z % gridSize;
            return position;
        }
        return Vector3.zero;
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
        isBuildingWall = false;
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
