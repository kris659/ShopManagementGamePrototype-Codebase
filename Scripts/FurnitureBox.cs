using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FurnitureBox : IPickable
{
    public FurnitureBoxGO furnitureBoxGO;
    public FurnitureBoxSO furnitureBoxSO;

    //public BuildingSO buildingSO;

    public BuildingSaveData buildingSaveData;

    public bool isTakenByCustomer;
    public bool isInClosedContainer;
    public List<PlacingTriggerArea> placingTriggerAreas = new();

    public int HoldingLimit => 1;
    public int PickableTypeID => 999;
    public int PickableID => ProductsData.instance.furnitureBoxesSpawned.IndexOf(this);

    public BoxCollider BoxCollider => furnitureBoxSO.prefab.GetComponentInChildren<BoxCollider>();

    public GameObject PreviewGameObject => SpawnVisual();
    public GameObject GameObject => furnitureBoxGO.gameObject;

    public bool CanPickableInteract => true;
    public string PickableInteractionText => "E - Place building";
    public void OnPickableInteract() {
        BuildingManager.instance.StartBuildingFromBox(buildingSaveData.buildingIndex, this);
    }
    public List<IPickable> AdditionalPickables => new();
    public void RemoveLastAdditionalPickable() { }

    private int vehicleAreasCount = 0;

    public FurnitureBox(BuildingSO buildingSO, Vector3 position, Quaternion rotation)
    {
        this.buildingSaveData =  new BuildingSaveData(Vector3.zero, Quaternion.identity, SOData.GetBuildingIndex(buildingSO), new int[0]);
        //this.buildingSO = buildingSO;
        this.furnitureBoxSO = SOData.furnitureBoxList[SOData.GetFurnitureBoxIndex(SOData.buildingsList[buildingSaveData.buildingIndex].furnitureBoxSO)]; //Rodzaj pud³a
        ProductsData.instance.furnitureBoxesSpawned.Add(this);
        Place(position, rotation, null, true);
    }

    public FurnitureBox(BuildingSaveData buildingSaveData, bool isPhysxSpawned, Vector3 position, Quaternion rotation)
    {
        this.buildingSaveData = buildingSaveData;
        //this.buildingSO = SOData.buildingsList[buildingSaveData.buildingIndex];
        int furnitureBoxTypeIndex = SOData.GetFurnitureBoxIndex(SOData.buildingsList[buildingSaveData.buildingIndex].furnitureBoxSO);
        if (furnitureBoxTypeIndex == -1) {
            Debug.LogWarning("Box type not found!");
            furnitureBoxTypeIndex = 0;
        }
        this.furnitureBoxSO = SOData.furnitureBoxList[furnitureBoxTypeIndex]; //Rodzaj pud³a
        ProductsData.instance.furnitureBoxesSpawned.Add(this);
        if (isPhysxSpawned)
            Place(position, rotation, null, false);
    }

    private GameObject SpawnVisual()
    {
        GameObject visual = GameObject.Instantiate(furnitureBoxSO.prefab);
        //visual.GetComponent<FurnitureBoxGO>().productIcon.sprite = buildingSO.Icon;
        GameObject.Destroy(visual.GetComponent<Rigidbody>());
        visual.layer = LayerMask.NameToLayer("IInteractable");
        visual.GetComponent<BoxCollider>().isTrigger = true;
        return visual;
    }

    public void Place(Vector3 position, Quaternion rotation, Transform parent, bool playSound)
    {
        DestroyGameObject();
        furnitureBoxGO = FurnitureBoxGO.Spawn(false, position, rotation, parent, this);

        //UpdatePlacingTriggerAreas(position, rotation);
        if (playSound)
            AudioManager.PlaySound(Sound.ProductDrop, position, furnitureBoxGO.transform);
    }

    public void SpawnVisual(Vector3 position, Quaternion rotation, Transform parent)
    {
        DestroyGameObject();
        furnitureBoxGO = FurnitureBoxGO.Spawn(true, position, rotation, parent, this);
    }

    public void OnPlayerTake(bool shoudSpawnVisual, Transform parent)
    {
        DestroyGameObject();
        if (shoudSpawnVisual)
        {
            SpawnVisual(parent);
        }
        AudioManager.PlaySound(Sound.ProductPickup, parent.position);
    }

    private void SpawnVisual(Transform parent)
    {
        furnitureBoxGO = FurnitureBoxGO.Spawn(true, Vector3.zero, Quaternion.identity, parent, this);
        furnitureBoxGO.transform.localPosition = furnitureBoxSO.offset;
        furnitureBoxGO.transform.localRotation = Quaternion.identity;
        furnitureBoxGO.transform.localScale = Vector3.one;
    }

    public FurnitureBoxSaveData CreateSaveData()
    {
        Vector3 position = Vector3.zero;
        Quaternion rotation = Quaternion.identity;

        if (furnitureBoxGO != null)
        {
            position = furnitureBoxGO.transform.position;
            rotation = furnitureBoxGO.transform.rotation;
        }
        bool isPhysxSpawned = furnitureBoxGO != null;

        return new FurnitureBoxSaveData(buildingSaveData, isPhysxSpawned, position, rotation);
    }

    public void DestroyGameObject()
    {
        vehicleAreasCount = 0;
        foreach (PlacingTriggerArea triggerArea in placingTriggerAreas) {
            triggerArea.OnFurnitureBoxTakenFromArea(this);
        }
        placingTriggerAreas.Clear();
        if (furnitureBoxGO != null)
        {
            GameObject.Destroy(furnitureBoxGO.gameObject);
        }
    }

    public void RemoveFromGame(bool shouldRemoveFromList)
    {
        DestroyGameObject();

        if (shouldRemoveFromList && ProductsData.instance.furnitureBoxesSpawned.Contains(this))
            ProductsData.instance.furnitureBoxesSpawned.Remove(this);
    }

    public void SpawnVisualSavePickup(Transform parent)
    {
        SpawnVisual(parent);
    }

    public void OnVehicleAreaEnter()
    {
        if (vehicleAreasCount == 0 && furnitureBoxGO != null && furnitureBoxGO.transform.TryGetComponent(out Rigidbody rb))
        {
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
        vehicleAreasCount++;
    }
    public void OnVehicleAreaExit()
    {
        vehicleAreasCount--;
        if (vehicleAreasCount == 0 && furnitureBoxGO != null && furnitureBoxGO.transform.TryGetComponent(out Rigidbody rb))
        {
            rb.interpolation = RigidbodyInterpolation.None;
        }
    }
}