using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public enum BuildingType { Floor, FloorBuilding, Ceiling, CeilingBuilding, WallBuilding, FloorDecorationOnly }

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager instance; 

    [System.Serializable]
    public struct BuildingsCategory
    {
        public string name;
        public BuildingSO[] buildings;
    }

    public class BuildingData
    {
        public GameObject buildingGO;
        public int buildingTypeIndex;

        public BuildingData(GameObject buildingGO, int buildingTypeIndex)
        {
            this.buildingGO = buildingGO;
            this.buildingTypeIndex = buildingTypeIndex;
        }
    }
    public List<BuildingsCategory> buildingCategories;
    public List<BuildingData> spawnedBuildings = new List<BuildingData>();
    [SerializeField] private GameObject playerCamera;
    private const float normalGridSize = 0.625f;

    [SerializeField] private LayerMask buildingLayer;
    [SerializeField] private LayerMask destroyingLayer;
    public LayerMask buildingsPlacingLayer;
    [SerializeField] private float buildingRange;
    [SerializeField] private Transform originPoint;

    public Material buildingPossibleMaterial;
    public Material buildingNotPossibleMaterial;
    public Material previewPossibleMaterial;
    public Material previewNotPossibleMaterial;

    public bool IsBuilding => building != null;
    public bool isRemovingBuildings = false;
    public bool isMovingBuildings = false;

    private GameObject buildingGO;

    private Vector3 buildingPosition;
    private Quaternion buildingRotation;
    private int rotationIndex = 0;

    private int currentBuildingIndex;

    [SerializeField] private NavMeshSurface surface;
    private BuildingSO buildingSO; 
    private Building building;

    [HideInInspector]
    public Vector3 defaultBuildingPosition = new Vector3(0, -10, 0);

    

    private Building buildingToDestroy;
    private Building buildingToMove;

    private void Awake()
    {
        if(instance != null) {
            Debug.LogError("Multiple buildingManagers");
            Destroy(gameObject);
            return;
        }
        instance = this;
        SceneLoader.OnUISceneLoaded += () => UIManager.buildingUI.Init(this);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B)) {
            CancelBuilding(false);
            CancelRemoving();
            CancelMoving();
        }

        if (UIManager.leftPanelUI == null || UIManager.leftPanelUI.currentlyOpenWindow != null)
            return;        

        if (IsBuilding && (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1)))
            CancelBuilding(false);
        if (isRemovingBuildings && (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1)))
            CancelRemoving();
        if (isMovingBuildings && (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1)))
            CancelMoving();
        if (IsBuilding && Input.GetKeyDown(KeyCode.R)) {
            Rotate();
        }

        if (IsBuilding) {
            MoveBuildingPreview();
        }
        if(IsBuilding && Input.GetMouseButtonDown(0))
            PlaceBuilding();
        if(isRemovingBuildings) {
            UpdateDestroying();
        }
        if (isMovingBuildings)
        {
            UpdateMoving();
        }
    }

    public void StartBuildingFromBox(int buildingIndex, FurnitureBox sourceBox)
    {
        SelectBuilding(buildingIndex);
        building.sourceFurnitureBox = sourceBox;
    }

    public void SelectBuilding(int buildingTypeIndex, int buildingIndex)
    {
        SelectBuilding(SOData.GetBuildingIndex(buildingCategories[buildingTypeIndex].buildings[buildingIndex]));
    }

    public void SelectBuilding(int buildingIndex)
    {
        CancelBuilding(false);

        UIManager.possibleActionsUI.AddAction("R - Rotate building");
        UIManager.possibleActionsUI.AddAction("LMB - Place building");
        UIManager.possibleActionsUI.AddAction("RMB - Cancel building");

        buildingSO = SOData.buildingsList[buildingIndex];
        buildingGO = Instantiate(buildingSO.Prefab);
        building = buildingGO.GetComponent<Building>();
        building.StartBuilding();
        this.currentBuildingIndex = buildingIndex;
    }
    private void Rotate()
    {
        rotationIndex++;
        rotationIndex %= 4;
        buildingRotation = Quaternion.Euler(new Vector3(0,90,0) * rotationIndex);
    }

    public Vector3 GetBuildingPosition(BuildingType buildingType, float gridSize = normalGridSize)
    {
        Vector3 origin = playerCamera.transform.position;
        Vector3 direction = playerCamera.transform.TransformDirection(Vector3.forward);
        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, buildingRange, buildingLayer) && hit.collider.TryGetComponent(out BuildingSurface buildingSurface) && buildingSurface.allowedBuildingType == buildingType) {
            Vector3 position = hit.point - originPoint.transform.position;
            
            if(buildingType != BuildingType.WallBuilding) {
                position = AlignToGrid(position, hit.transform.right, gridSize);
                position = AlignToGrid(position, hit.transform.forward, gridSize);
            }
            else {
                position = AlignToGrid(position, hit.transform.right, gridSize / 2);
                position = AlignToGrid(position, hit.transform.up, gridSize / 2);
                buildingRotation = Quaternion.LookRotation(hit.normal, Vector3.up);
            }
            return position + originPoint.transform.position;
        }
        return defaultBuildingPosition;
    }

    private Vector3 AlignToGrid(Vector3 position, Vector3 direction, float gridSize)
    {
        float dot = Vector3.Dot(position, direction);
        if (dot >= 0) {
            if (dot % gridSize <= gridSize / 2)
                dot -= dot % gridSize;
            else
                dot += gridSize - dot % gridSize;
        }
        else {
            if (Mathf.Abs(dot) % gridSize <= gridSize / 2)
                dot += Mathf.Abs(dot) % gridSize;
            else
                dot -= gridSize - Mathf.Abs(dot) % gridSize;
        }

        return position - Vector3.Project(position, direction) + (dot * direction);
    }

    private void MoveBuildingPreview()
    {
        buildingPosition = building.GetBuildingPosition();
        building.UpdatePreview(buildingPosition, buildingRotation);
    }

    private void PlaceBuilding()
    {
        if (!building.CanBuildHere(buildingPosition, buildingRotation) || !building.isReadyToBuild)
            return;
        if (!PlayerData.instance.CanAfford(buildingSO.Price) && building.sourceFurnitureBox == null)
        {
            UIManager.textUI.UpdateText("Can't afford", 3f);
            return;
        }
        AudioManager.PlaySound(Sound.Build, buildingPosition);
        spawnedBuildings.Add(new BuildingData(building.gameObject, currentBuildingIndex));
        building.Build();
        if (building != null)
        {   building = null;
            SelectBuilding(currentBuildingIndex);
            PlayerData.instance.TakeMoney(buildingSO.Price);
        }
        StartCoroutine(UpdateNavMesh());
    }

    public void CancelBuilding(bool BuildingFromBox) //If true it doesnt cancell building cause it does it by itself
    {
        if(building != null && !BuildingFromBox)
            building.CancelBuilding();
        building = null;
        buildingGO = null;
        buildingSO = null;
        UIManager.possibleActionsUI.RemoveAction("R - Rotate building");
        UIManager.possibleActionsUI.RemoveAction("LMB - Place building");
        UIManager.possibleActionsUI.RemoveAction("RMB - Cancel building");
    }

    public void CancelRemoving()
    {
        isRemovingBuildings = false;
        UIManager.textUI.UpdateText("");
        UIManager.possibleActionsUI.RemoveAction("LMB - Destroy building");
        UIManager.possibleActionsUI.RemoveAction("RMB - Cancel destroying");
        if(buildingToDestroy != null)
            buildingToDestroy.SetCollisionVisualActive(false);
    }

    public void CancelMoving()
    {
        isMovingBuildings = false;
        UIManager.textUI.UpdateText("");
        UIManager.possibleActionsUI.RemoveAction("LMB - Pack building");
        UIManager.possibleActionsUI.RemoveAction("RMB - Cancel packing");
        if (buildingToMove != null)
            buildingToMove.SetCollisionVisualActive(false);
    }

    private void UpdateDestroying()
    {
        Vector3 origin = playerCamera.transform.position;
        Vector3 direction = playerCamera.transform.TransformDirection(Vector3.forward);
        RaycastHit hit;
        if(buildingToDestroy != null)
            buildingToDestroy.SetCollisionVisualActive(false);
        if (Physics.Raycast(origin, direction, out hit, buildingRange, destroyingLayer)) {
            buildingToDestroy = hit.transform.GetComponentInParent<Building>();
            if (buildingToDestroy != null) {
                buildingToDestroy.SetCollisionVisualActive(true);
                if (Input.GetMouseButtonDown(0)) {
                    for (int i = 0; i < spawnedBuildings.Count; i++) {
                        if (spawnedBuildings[i].buildingGO == buildingToDestroy.gameObject) {
                            PlayerData.instance.AddMoney(SOData.buildingsList[spawnedBuildings[i].buildingTypeIndex].Price * 0.8f, false);
                            spawnedBuildings.RemoveAt(i);
                            break;
                        }
                    }
                    AudioManager.PlaySound(Sound.Destroy, buildingToDestroy.transform.position);
                    Destroy(buildingToDestroy.gameObject);

                    StartCoroutine(UpdateNavMesh());
                }
            }
        }
    }
    private void UpdateMoving()
    {
        Vector3 origin = playerCamera.transform.position;
        Vector3 direction = playerCamera.transform.TransformDirection(Vector3.forward);
        RaycastHit hit;
        if (buildingToMove != null)
            buildingToMove.SetCollisionVisualActive(false);
        if (Physics.Raycast(origin, direction, out hit, buildingRange, destroyingLayer))
        {
            buildingToMove = hit.transform.GetComponentInParent<Building>();
            if (buildingToMove != null)
            {
                buildingToMove.SetCollisionVisualActive(true);
                if (Input.GetMouseButtonDown(0))
                {
                    for (int i = 0; i < spawnedBuildings.Count; i++)
                    {
                        if (spawnedBuildings[i].buildingGO == buildingToMove.gameObject)
                        {
                            FurnitureBox newBox = new FurnitureBox(GetBuildingSaveData(i), true, buildingToMove.transform.localPosition + new Vector3(0, 0.5f, 0), buildingToMove.transform.localRotation);
                            //PlayerData.instance.AddMoney(SOData.buildingsList[spawnedBuildings[i].buildingTypeIndex].Price * 0.8f, false);
                            spawnedBuildings.RemoveAt(i);
                            break;
                        }
                    }
                    AudioManager.PlaySound(Sound.Destroy, buildingToMove.transform.position);
                    Destroy(buildingToMove.gameObject);

                    StartCoroutine(UpdateNavMesh());
                }
            }
        }
    }
    private IEnumerator UpdateNavMesh()
    {
        yield return new WaitForEndOfFrame();
        surface.BuildNavMesh();
    }

    public GameObject SpawnProp(int propIndex, Vector3 position, Quaternion rotation)
    {
        GameObject propGO = Instantiate(buildingCategories[0].buildings[propIndex].Prefab, position, rotation);
        int buildingIndex = SOData.GetBuildingIndex(buildingCategories[0].buildings[propIndex]);
        spawnedBuildings.Add(new BuildingData(propGO, buildingIndex));
        return propGO;
    }

    public BuildingSaveData[] GetSaveData()
    {
        BuildingSaveData[] buildlingSaveData = new BuildingSaveData[spawnedBuildings.Count];
        for(int i = 0; i < spawnedBuildings.Count; i++) {
            buildlingSaveData[i] = GetBuildingSaveData(i);
        }
        return buildlingSaveData;
    }

    public BuildingSaveData GetBuildingSaveData(int i)
    {
        int[] additionalSaveData = new int[0];

        if (spawnedBuildings[i].buildingGO == null)
            return new BuildingSaveData(Vector3.zero, Quaternion.identity, 0, new int[0]);
        if (spawnedBuildings[i].buildingGO.TryGetComponent(out Building building))
            additionalSaveData = building.GetAdditionalSaveData();

        return new BuildingSaveData(
            spawnedBuildings[i].buildingGO.transform.position, spawnedBuildings[i].buildingGO.transform.rotation,
            spawnedBuildings[i].buildingTypeIndex, additionalSaveData);        
    }

    public void LoadFromSaveData(BuildingSaveData[] buildlingSaveData)
    {
        for (int i = 0; i < buildlingSaveData.Length; i++) {
            if(buildlingSaveData[i].position == Vector3.zero) {
                continue;
            }
            buildingSO = SOData.buildingsList[buildlingSaveData[i].buildingIndex];
            buildingGO = Instantiate(buildingSO.Prefab);
            buildingGO.transform.position = buildlingSaveData[i].position;
            buildingGO.transform.rotation = buildlingSaveData[i].rotation;
            if(buildingGO.TryGetComponent(out Building building)) {
                building.Build();
                building.LoadAdditionalSaveData(buildlingSaveData[i].additionalBuildingData);
            }
            spawnedBuildings.Add(new BuildingData(buildingGO, buildlingSaveData[i].buildingIndex));
        }
        CancelBuilding(false);
    }

    public void DestroyAllBuildings()
    {
        for(int i = 0; i < spawnedBuildings.Count; i++) {
            Destroy(spawnedBuildings[i].buildingGO);
        }
        spawnedBuildings.Clear();
    }

    public void OnDestroyButtonClicked()
    {
        isRemovingBuildings = true;        
        UIManager.textUI.UpdateText("Destroy mode (right click to cancel)");
        UIManager.possibleActionsUI.AddAction("LMB - Destroy building");
        UIManager.possibleActionsUI.AddAction("RMB - Cancel destroying", 100);        
    }

    public void OnMoveButtonClicked()
    {
        isMovingBuildings = true;
        UIManager.textUI.UpdateText("Packing mode (right click to cancel)");
        UIManager.possibleActionsUI.AddAction("LMB - Pack building");
        UIManager.possibleActionsUI.AddAction("RMB - Cancel packing", 100);
    }
}
