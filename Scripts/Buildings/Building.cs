using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Building : MonoBehaviour
{
    [SerializeField] internal BuildingType buildingType = BuildingType.FloorBuilding;

    [SerializeField] internal GameObject buildingModel;
    [SerializeField] internal GameObject buildingCollidersParent;

    internal GameObject buildingPreview;
    internal GameObject buildingPreviewCollision;

    internal GameObject buildingCollisionVisual;

    private BoxCollider[] buildingColliders;
    private List<Building> collidingBuildings = new List<Building>();

    public FurnitureBox sourceFurnitureBox;

    internal bool isReady = true;
    public bool isReadyToBuild => isReady;

    public virtual void StartBuilding()
    {
        CreateModels();
        buildingModel.SetActive(false);
        buildingPreview.SetActive(true);
    }

    private void CreateModels()
    {
        SpawnCollisionVisual();


        buildingPreview = new GameObject("BuildingPreview");
        buildingPreview.transform.SetParent(transform, false);
        buildingPreview.transform.localPosition = Vector3.zero;

        GameObject temp = Instantiate(buildingModel, buildingPreview.transform);
        temp.transform.localPosition = buildingModel.transform.localPosition;
        temp.transform.localScale = buildingModel.transform.localScale;

        SetMaterialToAllRenderers(temp, BuildingManager.instance.buildingPossibleMaterial);
        RemoveAllColliders(temp);


        buildingPreviewCollision = new GameObject("BuildingPreviewCollision");
        buildingPreviewCollision.transform.SetParent(transform, false);
        buildingPreviewCollision.transform.localPosition = Vector3.zero;

        temp = Instantiate(buildingModel, buildingPreviewCollision.transform);
        temp.transform.localPosition = buildingModel.transform.localPosition;
        temp.transform.localScale = buildingModel.transform.localScale;

        SetMaterialToAllRenderers(temp, BuildingManager.instance.buildingNotPossibleMaterial);
        RemoveAllColliders(temp);


        temp = GameObject.Instantiate(buildingCollisionVisual, buildingPreview.transform);
        temp.SetActive(true);
        RemoveAllColliders(temp);
        SetMaterialToAllRenderers(temp, BuildingManager.instance.previewPossibleMaterial);


        temp = GameObject.Instantiate(buildingCollisionVisual, buildingPreviewCollision.transform);
        temp.SetActive(true);
        RemoveAllColliders(temp);

        buildingCollisionVisual.transform.localScale = buildingModel.transform.localScale;
    }

    private void SetMaterialToAllRenderers(GameObject gameObject, Material material)
    {
        Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers) {
            renderer.SetMaterials(Enumerable.Repeat(material, renderer.materials.Length).ToList());
        }
    }

    private void SpawnCollisionVisual()
    {
        buildingColliders = buildingCollidersParent.GetComponentsInChildren<BoxCollider>();
        buildingCollisionVisual = new GameObject("CollisionVisual");
        buildingCollisionVisual.transform.SetParent(transform);
        buildingCollisionVisual.transform.localPosition = Vector3.zero;
        buildingCollisionVisual.transform.localScale = Vector3.one;
        buildingCollisionVisual.SetActive(false);

        GameObject temp;
        for (int i = 0; i < buildingColliders.Length; i++) {
            temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            RemoveAllColliders(temp);

            Vector3 center = buildingColliders[i].transform.position + transform.rotation * Vector3.Scale(buildingColliders[i].center, buildingColliders[i].transform.localScale);
            Vector3 scale = buildingColliders[i].transform.localScale;
            Vector3 size = buildingColliders[i].size;

            Vector3 finalSize = transform.rotation * (new Vector3(scale.x * size.x, scale.y * size.y, scale.z * size.z) + Vector3.one * 0.05f);

            temp.transform.SetParent(buildingCollisionVisual.transform);
            temp.transform.position = center;
            temp.transform.localScale = finalSize;

            temp.GetComponent<Renderer>().material = BuildingManager.instance.previewNotPossibleMaterial;
        }
    }
    private void RemoveAllColliders(GameObject gameObject)
    {
        Collider[] colliders = gameObject.GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders) {
            Destroy(collider);
        }
    }

    public virtual Vector3 GetBuildingPosition()
    {
        return BuildingManager.instance.GetBuildingPosition(buildingType);
    }

    public virtual bool CanBuildHere(Vector3 position, Quaternion rotation)
    {
        if(position == BuildingManager.instance.defaultBuildingPosition)
            return false;
        bool canBuild = true;
        ClearCollidingBuildings();
        for (int i = 0; i < buildingColliders.Length; i++) {
            Vector3 center = buildingColliders[i].transform.position + rotation * Vector3.Scale(buildingColliders[i].center, buildingColliders[i].transform.localScale);
            Vector3 scale = buildingColliders[i].transform.localScale;
            Vector3 size = buildingColliders[i].size;
            Vector3 halfExtents = new Vector3(scale.x * size.x, scale.y * size.y, scale.z * size.z) / 2f;
            Collider[] hitColliders = Physics.OverlapBox(center, halfExtents, rotation, BuildingManager.instance.buildingsPlacingLayer);
            int ownColliders = 0;
            for(int j = 0; j < hitColliders.Length; j++) {
                Building building = hitColliders[j].transform.GetComponentInParent<Building>();
                if(building == this)
                    ownColliders++;
                if (building != null && building != this) {
                    collidingBuildings.Add(building);
                    building.SetCollisionVisualActive(true);
                }
            }

            if(hitColliders.Length - ownColliders > 0) {
                canBuild = false;
            }
        }
        return canBuild;
    }

    private void ClearCollidingBuildings()
    {
        foreach (Building building in collidingBuildings) {
            if (building != null) {
                building.SetCollisionVisualActive(false);
            }
        }
        collidingBuildings.Clear();
    }

    public void SetCollisionVisualActive(bool active)
    {
        buildingCollisionVisual.SetActive(active);
    }       

    public virtual void UpdatePreview(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;

        if(CanBuildHere(position, rotation)) {
            buildingPreview.SetActive(true);
            buildingPreviewCollision.SetActive(false);
        }
        else {
            buildingPreview.SetActive(false);
            buildingPreviewCollision.SetActive(true);
        }
    }

    public virtual int[] GetAdditionalSaveData()
    {
        return new int[0];
    }

    public virtual void LoadAdditionalSaveData(int[] additionalSaveData) { }

    public virtual void Build()
    {
        if (buildingCollisionVisual == null)
            SpawnCollisionVisual();

        buildingModel.SetActive(true);
        ClearCollidingBuildings();
        Destroy(buildingPreview);

        if (sourceFurnitureBox != null)
        {
            PlayerData.instance.playerPickup.RemoveLastPickable();
            sourceFurnitureBox.RemoveFromGame(true);
            BuildingManager.instance.CancelBuilding(true);
        }        
    }

    public void CancelBuilding()
    {
        ClearCollidingBuildings();  
        Destroy(gameObject);        
    }
}
