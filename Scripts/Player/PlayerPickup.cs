using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPickup : MonoBehaviour
{
    [SerializeField] private LayerMask _pickableLayerMask;
    [SerializeField] private LayerMask _placingLayerMask;
    [SerializeField] private LayerMask _snappingPointsLayerMask;
    [SerializeField] private GameObject playerCamera;

    [SerializeField] private Material placingPreviewMaterial;
    [SerializeField] private Material placingWrongPreviewMaterial;

    [SerializeField] private Transform productParent;

    public bool IsHoldingObject => pickableScriptsList.Count > 0;
    public bool IsPlacingObject { get; set; }

    private GameObject previewGO;
    private GameObject emptyGO;

    private float pickupRange = 3;
    private float placingRange = 4;

    private Vector3 defaultSpawnPosition = new Vector3(0, -10, 0);

    Quaternion placingRotation { get; set; }

    private List<IPickable> pickableScriptsList = new ();
    public int PickablesAmount => pickableScriptsList.Count;

    public IPickable LastPickable => pickableScriptsList[PickablesAmount - 1];
    public IPickable CurrentPickable => GetCurrentPickable();

    private string currentInteractionText;
    private void Update()
    {
        if (UIManager.leftPanelUI == null || UIManager.leftPanelUI.currentlyOpenWindow != null || BuildingManager.instance.IsBuilding || BuildingManager.instance.isRemovingBuildings)
            return;
        
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return))
        {
            if (IsHoldingObject && IsPlacingObject){
                DropObject();
            }
            else
                if (Input.GetMouseButtonDown(0) && !Input.GetMouseButton(1))
                    TryToPickup();
        }
        else
        {
            if (Input.GetMouseButtonDown(1) && IsHoldingObject){
                IsPlacingObject = true;
                previewGO.SetActive(IsPlacingObject);
            }

            if (Input.GetMouseButtonUp(1) && IsPlacingObject && IsHoldingObject){
                IsPlacingObject = false;                
                previewGO.SetActive(IsPlacingObject);
            }
        }
        HandleInteractionsUI();

        float mouseScrollY = Input.mouseScrollDelta.y;
        //if (data.buttonZ) mouseScrollY = -1;
        //if (data.buttonX) mouseScrollY = 1;
        if (Input.GetKeyDown(KeyCode.R))
            mouseScrollY = 1;
        if (mouseScrollY != 0) placingRotation = Quaternion.Euler(placingRotation.eulerAngles + new Vector3(0, 90 * mouseScrollY, 0));

        MovePlacingPreview();
    }

    private IPickable GetCurrentPickable()
    {
        IPickable pickable = pickableScriptsList[PickablesAmount - 1];
        if (pickable.AdditionalPickables.Count > 0) {
            return pickable.AdditionalPickables[pickable.AdditionalPickables.Count - 1];
        }
        return pickable;
    }

    private void TryToPickup()
    {        
        if (pickableScriptsList.Count > 0 && PickablesAmount >= pickableScriptsList[pickableScriptsList.Count - 1].HoldingLimit) return;

        IPickable product = RaycastPickable();
        if(product != null) {
            Pickup(product);
        }
    }
    private IPickable RaycastPickable()
    {
        Vector3 origin = playerCamera.transform.position;
        Vector3 direction = playerCamera.transform.TransformDirection(Vector3.forward);

        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, pickupRange, _pickableLayerMask, QueryTriggerInteraction.Ignore)) {
            if (hit.transform.TryGetComponent(out IPickableGO pickableScript)){
                return pickableScript.pickable;
            }

            if (hit.transform.parent != null && hit.transform.parent.TryGetComponent(out pickableScript)) {
                return pickableScript.pickable;
            }
        }
        return null;
    }

    private void Pickup(IPickable pickableScript)
    {
        if (pickableScriptsList.Count > 0 && pickableScriptsList[0].PickableTypeID != pickableScript.PickableTypeID)
            return;
        BuildingManager.instance.CancelRemoving();
        BuildingManager.instance.CancelMoving();
        pickableScriptsList.Add(pickableScript);
        UIManager.holdingUI.UpdateText(PickablesAmount, pickableScriptsList[PickablesAmount - 1].HoldingLimit);
        pickableScript.OnPlayerTake(PickablesAmount == 1, productParent);
        if (PickablesAmount == 1) {
            UpdatePreview();
            if (LastPickable.CanPickableInteract) {
                UIManager.possibleActionsUI.AddAction(LastPickable.PickableInteractionText);
                currentInteractionText = LastPickable.PickableInteractionText;
            }
        }
    }


    private void DropObject()
    {
        GetPlacingPosition(out Vector3 position, out Quaternion rotation);
        if (CanPlace(position, rotation))
        {            
            if (CurrentPickable != LastPickable) {
                CurrentPickable.Place(position, rotation, null, true);
                LastPickable.RemoveLastAdditionalPickable();
                if (LastPickable.AdditionalPickables.Count == 0)
                    IsPlacingObject = false;
                UpdatePreview();
            }
            else {
                if (PickablesAmount == 1 && LastPickable.CanPickableInteract) {
                    UIManager.possibleActionsUI.RemoveAction(LastPickable.PickableInteractionText);
                }

                LastPickable.Place(position, rotation, null, true);
                RemoveLastPickable();
            }
        }
    }

    public void UpdatePreview()
    {
        if (previewGO != null) 
            Destroy(previewGO);

        if (PickablesAmount > 0){
            placingRotation = Quaternion.identity;

            previewGO = CurrentPickable.PreviewGameObject;            

            Destroy(previewGO.GetComponent<ProductGO>());
            Destroy(previewGO.GetComponent<FurnitureBoxGO>());

            SetPreviewMaterial(placingPreviewMaterial);
            previewGO.name = "Placing_Preview";
            previewGO.SetActive(IsPlacingObject);
        }
    }

    public void UpdatePossibleAction()
    {
        UIManager.possibleActionsUI.RemoveAction(currentInteractionText);
        if (LastPickable.CanPickableInteract) {
            UIManager.possibleActionsUI.AddAction(LastPickable.PickableInteractionText);
            currentInteractionText = LastPickable.PickableInteractionText;
        }
    }

    private void MovePlacingPreview()
    {
        if (previewGO == null || !IsPlacingObject)
            return;
        
        GetPlacingPosition(out Vector3 placingPosition, out Quaternion placingRotation);
        previewGO.transform.position = placingPosition;
        previewGO.transform.rotation = placingRotation;

        if (CanPlace(placingPosition, placingRotation))
            SetPreviewMaterial(placingPreviewMaterial);            
        else
            SetPreviewMaterial(placingWrongPreviewMaterial);        
    }

    private void SetPreviewMaterial(Material material)
    {
        Renderer[] renderers = previewGO.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
            renderer.material = material;
    }

    private Vector3 CalculateAdditionalOffset(Vector3 origin, Vector3 axisToCheck, float checkLength, Vector3 axisThatCancels, float cancelLength){
        RaycastHit hit, hit1;
        if (Physics.Raycast(origin, axisThatCancels, out hit, cancelLength, _placingLayerMask))
        {
            return Vector3.zero;
        }
        bool didHit = false;
        if (Physics.Raycast(origin, axisToCheck, out hit, checkLength, _placingLayerMask))
        {
            didHit = true; 
        }
        if (Physics.Raycast(origin, -axisToCheck, out hit1, checkLength, _placingLayerMask))
        {
            if(didHit) return Vector3.zero;
            return axisToCheck * (checkLength - hit1.distance);
        }
        if(didHit) return -axisToCheck * (checkLength - hit.distance);
        return Vector3.zero;
    }

    private void GetPlacingPosition(out Vector3 spawnPosition, out Quaternion spawnRotation)
    {
        if (emptyGO == null) emptyGO = new GameObject("Placing Empty(Calculations)");
        GameObject previewGO = emptyGO;
        BoxCollider collider = CurrentPickable.BoxCollider;

        Vector3 colliderSize = new Vector3(collider.size.x * collider.transform.localScale.x, collider.size.y * collider.transform.localScale.y, collider.size.z * collider.transform.localScale.z);

        spawnPosition = defaultSpawnPosition;
        Vector3 offset = Vector3.zero;
        Vector3 origin = playerCamera.transform.position;
        Vector3 direction = playerCamera.transform.TransformDirection(Vector3.forward);
        previewGO.transform.rotation = placingRotation; //Quaternion.Euler(transform.GetChild(0).eulerAngles + placingRotation.eulerAngles);
        emptyGO.transform.rotation = placingRotation; //Quaternion.Euler(transform.GetChild(0).eulerAngles + placingRotation.eulerAngles);
        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, placingRange, _placingLayerMask)) {
            int i = 0;
            float maxAngleCorrection = 60;
            float angle;

            float surfaceRotationAdjustment = (previewGO.transform.eulerAngles.y - hit.transform.eulerAngles.y) % 90;
            if (surfaceRotationAdjustment < -45)
                surfaceRotationAdjustment += 90;
            if (surfaceRotationAdjustment > 45)
                surfaceRotationAdjustment -= 90;

            previewGO.transform.eulerAngles -= new Vector3(0, surfaceRotationAdjustment, 0); 
            //Front
            angle = Vector3.Angle(previewGO.transform.forward, hit.normal);
            //Debug.Log(angle);
            if (angle >= 180 - maxAngleCorrection) {
                i++;
                //Debug.Log("Forward");
                previewGO.transform.forward = -hit.normal;
                offset = -previewGO.transform.forward * colliderSize.z / 2;
                offset += CalculateAdditionalOffset(hit.point + offset, previewGO.transform.right, colliderSize.x / 2,
                    -previewGO.transform.forward, colliderSize.z / 2);
                offset += CalculateAdditionalOffset(hit.point + offset, previewGO.transform.up, colliderSize.y / 2,
                    -previewGO.transform.forward, colliderSize.z / 2);
            }
            //Back
            if (angle < maxAngleCorrection) {
                i++;
                //Debug.Log("Back");
                previewGO.transform.forward = hit.normal;
                offset = previewGO.transform.forward * colliderSize.z / 2;
                offset += CalculateAdditionalOffset(hit.point + offset, -previewGO.transform.right, colliderSize.x / 2,
                    previewGO.transform.forward, colliderSize.z / 2);
                offset += CalculateAdditionalOffset(hit.point + offset, -previewGO.transform.up, colliderSize.y / 2,
                    previewGO.transform.forward, colliderSize.z / 2);
            }
            //Right
            angle = Vector3.Angle(previewGO.transform.right, hit.normal);
            if (angle >= 180 - maxAngleCorrection) {
                i++;
                //Debug.Log("Right");
                previewGO.transform.right = -hit.normal;
                offset = -previewGO.transform.right * colliderSize.x / 2;
                offset += CalculateAdditionalOffset(hit.point + offset, previewGO.transform.forward, colliderSize.z / 2,
                    -previewGO.transform.right, colliderSize.x / 2);
                offset += CalculateAdditionalOffset(hit.point + offset, previewGO.transform.up, colliderSize.y / 2,
                    -previewGO.transform.right, colliderSize.x / 2);
            }
            //Left
            if (angle < maxAngleCorrection) {
                i++;
                //Debug.Log("Left");
                previewGO.transform.right = hit.normal;
                offset = previewGO.transform.right * colliderSize.x / 2;
                offset += CalculateAdditionalOffset(hit.point + offset, previewGO.transform.forward, colliderSize.z / 2,
                    previewGO.transform.right, colliderSize.x / 2);
                offset += CalculateAdditionalOffset(hit.point + offset, previewGO.transform.up, colliderSize.y / 2,
                    previewGO.transform.right, colliderSize.x / 2);
            }

            //Up
            angle = Vector3.Angle(previewGO.transform.up, hit.normal);
            if (angle >= 180 - maxAngleCorrection) {
                i++;
                //Debug.Log("Up");                
                float rotationY = previewGO.transform.eulerAngles.y;
                previewGO.transform.up = -hit.normal;
                previewGO.transform.Rotate(transform.up, rotationY - previewGO.transform.eulerAngles.y);

                offset = -previewGO.transform.up * colliderSize.y / 2;
                offset += CalculateAdditionalOffset(hit.point + offset, previewGO.transform.forward, colliderSize.z / 2,
                    -previewGO.transform.up, colliderSize.y / 2);
                offset += CalculateAdditionalOffset(hit.point + offset, previewGO.transform.right, colliderSize.x / 2,
                    -previewGO.transform.up, colliderSize.y / 2);
            }
            //Down
            if (angle < maxAngleCorrection) {
                i++;
                //Debug.Log("Down");
                float rotationY = previewGO.transform.eulerAngles.y;
                previewGO.transform.up = hit.normal;
                previewGO.transform.Rotate(transform.up, rotationY - previewGO.transform.eulerAngles.y);

                offset = previewGO.transform.up * colliderSize.y / 2;
                offset += CalculateAdditionalOffset(hit.point + offset, previewGO.transform.forward, colliderSize.z / 2,
                    previewGO.transform.up, colliderSize.y / 2);
                offset += CalculateAdditionalOffset(hit.point + offset, previewGO.transform.right, colliderSize.x / 2,
                    previewGO.transform.up, colliderSize.y / 2);
            }


            //Debug.Log(i);
            offset -= previewGO.transform.up * colliderSize.y / 2;
            spawnPosition = hit.point;
        }
        spawnPosition += offset;
        spawnRotation = previewGO.transform.rotation;


        SnappingArea snappingArea = GetSnappingArea(spawnPosition);
        if (snappingArea != null && snappingArea.CanPlacePickable(CurrentPickable)) {
            Transform snappedTransform = snappingArea.GetSnappedTransform(spawnPosition);
            spawnPosition = snappedTransform.position;
            spawnRotation = snappedTransform.rotation;
        }
    }

    private bool CanPlace(Vector3 spawnPosition, Quaternion rotation)
    {
        if(spawnPosition == defaultSpawnPosition) 
            return false;
        BoxCollider collider = CurrentPickable.BoxCollider;
        Vector3 colliderCenter = collider.center;
        colliderCenter.Scale(collider.transform.localScale);
        Vector3 center = spawnPosition + rotation * (colliderCenter + collider.transform.localPosition);
        Vector3 colliderSize = new Vector3(collider.size.x * collider.transform.localScale.x, collider.size.y * collider.transform.localScale.y, collider.size.z * collider.transform.localScale.z);

        Vector3 halfExtents = colliderSize / 2.1f;
        Collider[] hitColliders = Physics.OverlapBox(center, halfExtents, rotation, _placingLayerMask);
        SnappingArea snappingArea = GetSnappingArea(spawnPosition);

        //for(int i = 0; i < hitColliders.Length; i++) {
        //    Debug.Log(hitColliders[i].name);
        //}

        return (hitColliders.Length == 0) && (snappingArea == null || snappingArea.CanPlacePickable(CurrentPickable));
    }

    private SnappingArea GetSnappingArea(Vector3 spawnPosition)
    {
        Vector3 cameraOrigin = Camera.main.transform.position;
        Vector3 rayDirection = Camera.main.transform.TransformDirection(Vector3.forward);
        RaycastHit snappingAreaHit;
        if (Physics.Raycast(cameraOrigin, rayDirection, out snappingAreaHit, placingRange, _snappingPointsLayerMask, QueryTriggerInteraction.Collide)) {
            if (snappingAreaHit.collider.TryGetComponent(out SnappingArea snappingArea)) {
                return snappingArea;
            }
        }
        return null;
    }

    int interactionsCounter = 0;
    private void HandleInteractionsUI()
    {
        interactionsCounter++;
        if (interactionsCounter != 6) return;
        interactionsCounter = 0;

        
        IPickable product = RaycastPickable();
        if (product != null) {
            if (pickableScriptsList.Count > 0 && (pickableScriptsList[0].PickableTypeID != product.PickableTypeID || PickablesAmount >= LastPickable.HoldingLimit)) {
                UIManager.possibleActionsUI.RemoveAction("LMB - pickup object");
            }
            else {
                UIManager.possibleActionsUI.AddAction("LMB - pickup object");
            }
        }
        else {
            UIManager.possibleActionsUI.RemoveAction("LMB - pickup object");
        }
        if (pickableScriptsList.Count > 0) {
            UIManager.possibleActionsUI.AddAction("Hold RMB and click LMB - place object", 100);
        }
        else {
            UIManager.possibleActionsUI.RemoveAction("Hold RMB and click LMB - place object");
        }
    }

    public void ClearPickupList()
    {
        pickableScriptsList.Clear();
    }
    
    public void RemoveLastPickable()
    {
        if (PickablesAmount == 1 && LastPickable.CanPickableInteract) {
            UIManager.possibleActionsUI.RemoveAction(LastPickable.PickableInteractionText);
        }

        pickableScriptsList.RemoveAt(PickablesAmount - 1);

        if (PickablesAmount == 0) {
            IsPlacingObject = false;
            UIManager.holdingUI.UpdateText(0, 0);
        }
        else
            UIManager.holdingUI.UpdateText(PickablesAmount, LastPickable.HoldingLimit);

        UpdatePreview();
    }

    public void OnDumpsterUsed()
    {
        if(PickablesAmount > 0) {
            AudioManager.PlaySound(Sound.ThrowAway);
        }
        if (PickablesAmount > 0 && LastPickable.CanPickableInteract) {
            UIManager.possibleActionsUI.RemoveAction(LastPickable.PickableInteractionText);
        }
        foreach (IPickable pickable in pickableScriptsList) {
            pickable.RemoveFromGame(true);
            if(pickable.PickableTypeID < 0)
                TasksManager.instance.ProgressTasks(TaskType.ThrowAwayBoxes, 1);
        }
        IsPlacingObject = false;
        if(previewGO != null)
            Destroy(previewGO);
        pickableScriptsList.Clear();
        UIManager.holdingUI.UpdateText(0, 0);
    }

    public void GetPickupSaveData(out int[] pickablesTypeID, out int[] pickableID)
    {
        pickablesTypeID = new int[PickablesAmount];
        pickableID = new int[PickablesAmount];

        for (int i = 0; i < PickablesAmount; i++) {
            pickablesTypeID[i] = pickableScriptsList[i].PickableTypeID;
            pickableID[i] = pickableScriptsList[i].PickableID;
        }
    }

    public void LoadFromSaveData(int[] pickablesTypeID, int[] pickablesID)
    {
        if(pickablesTypeID == null || pickablesID == null) {
            pickablesTypeID = new int[0];
            pickablesID = new int[0];
            return;
        }

        for (int i = 0; i < pickablesTypeID.Length; i++) {
            IPickable pickableScript = ProductsData.instance.GetPickableByID(pickablesTypeID[i], pickablesID[i]);
            if(pickableScript == null)
                continue;
            pickableScriptsList.Add(pickableScript);
            if(PickablesAmount == 1) {
                pickableScript.OnPlayerTake(true, productParent);
                UpdatePreview();
            }            
        }
        if (PickablesAmount > 0) {
            UIManager.holdingUI.UpdateText(PickablesAmount, CurrentPickable.HoldingLimit);
            if (CurrentPickable.CanPickableInteract) {
                UIManager.possibleActionsUI.AddAction(CurrentPickable.PickableInteractionText);
            }
        }        
    }
}
