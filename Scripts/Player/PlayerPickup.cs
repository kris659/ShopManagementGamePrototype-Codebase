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
    private GameObject productGO;
  
    public bool IsHoldingObject { get { return productScriptList.Count > 0; } }
    public bool IsPlacingObject { get; set; }

    private GameObject previewGO;
    private GameObject emptyGO;

    private float pickupRange = 6;
    private float placingRange = 4;

    Quaternion placingRotation { get; set; }

    private List<IPickable> productScriptList = new List<IPickable>();
    private int productsAmount { get { return productScriptList.Count; } }

    private void Update()
    {
        MovePlacingPreview();
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
        //float mouseScrollY = -data.mouseScrollY;
        //if (data.buttonZ) mouseScrollY = -1;
        //if (data.buttonX) mouseScrollY = 1;
        //if (mouseScrollY != 0) placingRotation = Quaternion.Euler(placingRotation.eulerAngles + new Vector3(0, 15 * mouseScrollY, 0));        
    }

    private void TryToPickup()
    {        
        if (productScriptList.Count > 0 && productsAmount >= productScriptList[productScriptList.Count - 1].HoldingLimit) return;

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
        if (Physics.Raycast(origin, direction, out hit, pickupRange, _pickableLayerMask)) {
            if (hit.transform.TryGetComponent(out IPickableGO pickableScript)) {
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
        if (productScriptList.Count > 0 && productScriptList[0].HoldingLimitID != pickableScript.HoldingLimitID)
            return;
        productScriptList.Add(pickableScript);
        UIManager.holdingUI.UpdateText(productsAmount, productScriptList[productsAmount - 1].HoldingLimit);
        pickableScript.OnPlayerTake(productsAmount == 1, productParent);
        if (productsAmount == 1)
            SetPreviewActive();
    }


    public void DropObject()
    {
        GetSpawnPosition(out Vector3 position, out Quaternion rotation);
        if (CanPlace(position, rotation))
        {
            UIManager.holdingUI.UpdateText(productsAmount - 1, productScriptList[productsAmount - 1].HoldingLimit);
            productScriptList[productsAmount - 1].Place(position, rotation, null);
            productScriptList.RemoveAt(productsAmount - 1);
            
            if (productsAmount == 0) {
                IsPlacingObject = false;
                SetPreviewActive();
            }
        }
    }

    public void SetPreviewActive()
    {
        if (productsAmount > 0)
        {
            placingRotation = Quaternion.identity;

            previewGO = productScriptList[0].PreviewGameObject;
            previewGO.transform.GetChild(0).GetComponent<Renderer>().material = placingPreviewMaterial;
            previewGO.name = "Placing Preview";
            previewGO.SetActive(false);
        }
        else{
            if (previewGO != null) Destroy(previewGO);
        }
    }

    public void MovePlacingPreview()
    {        
        if (previewGO != null && previewGO.activeSelf)
        {   
            GetSpawnPosition(out Vector3 placingPosition, out Quaternion placingRot);

            previewGO.transform.position = placingPosition;
            previewGO.transform.rotation = placingRot;

            if (CanPlace(placingPosition, previewGO.transform.rotation))
                previewGO.transform.GetChild(0).GetComponent<Renderer>().material = placingPreviewMaterial;
            else            
                previewGO.transform.GetChild(0).GetComponent<Renderer>().material = placingWrongPreviewMaterial;
        }
    }

    Vector3 CalculateAdditionalOffset(Vector3 origin, Vector3 axisToCheck, float checkLength, Vector3 axisThatCancels, float cancelLength){
        RaycastHit hit, hit1;
        if (Physics.Raycast(origin, axisThatCancels, out hit, checkLength, _placingLayerMask))
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
    
    void GetSpawnPosition(out Vector3 spawnPosition, out Quaternion spawnRotation){

        if (emptyGO == null) emptyGO = new GameObject("Placing Empty(Calculations)");
        GameObject previewGO = emptyGO;
        BoxCollider collider = productScriptList[productsAmount - 1].BoxCollider;
        //Debug.Log(collider.name);
        Vector3 colliderSize = collider.size;
        //Debug.Log(colliderSize);

        spawnPosition = transform.position;
        Vector3 offset = Vector3.zero;
        Vector3 origin = playerCamera.transform.position;
        Vector3 direction = playerCamera.transform.TransformDirection(Vector3.forward);
        //previewGO.transform.rotation = Quaternion.Euler(transform.GetChild(0).eulerAngles + placingRotation.eulerAngles);
        //emptyGO.transform.rotation = Quaternion.Euler(transform.GetChild(0).eulerAngles + placingRotation.eulerAngles);
        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, placingRange, _placingLayerMask)) {
            int i = 0;
            float maxAngleCorrection = 60;
            float angle;

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
                offset -= CalculateAdditionalOffset(hit.point + offset, previewGO.transform.right, colliderSize.x / 2,
                    previewGO.transform.forward, colliderSize.z / 2);
                offset -= CalculateAdditionalOffset(hit.point + offset, previewGO.transform.up, colliderSize.y / 2,
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
                previewGO.transform.up = -hit.normal;
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
                previewGO.transform.up = hit.normal;
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
    }

    bool CanPlace(Vector3 spawnPosition, Quaternion rotation)
    {
        BoxCollider collider = productScriptList[productsAmount - 1].BoxCollider;
        //Quaternion rotation = Quaternion.Euler(transform.GetChild(0).eulerAngles + collider.transform.localEulerAngles + placingRotation.eulerAngles);
        Vector3 center = spawnPosition + collider.center + collider.transform.localPosition;
        Vector3 halfExtents = collider.size / 2.2f;
        Collider[] hitColliders = Physics.OverlapBox(center, halfExtents, rotation, _placingLayerMask);
        //if(hitColliders.Length > 0) { Debug.Log(hitColliders[0].transform.parent.name); }
        return (hitColliders.Length == 0);
    }

    int interactionsCounter = 0;
    void HandleInteractionsUI()
    {
        interactionsCounter++;
        if (interactionsCounter != 6) return;
        interactionsCounter = 0;

        
        IPickable product = RaycastPickable();
        if (product != null) {
            if (productScriptList.Count > 0 && (productScriptList[0].HoldingLimitID != product.HoldingLimitID || productsAmount >= productScriptList[productScriptList.Count - 1].HoldingLimit)) {
                UIManager.possibleActionsUI.RemoveAction("LMB - pickup object");
            }
            else {
                UIManager.possibleActionsUI.AddAction("LMB - pickup object");
            }
        }
        else {
            UIManager.possibleActionsUI.RemoveAction("LMB - pickup object");
        }
        if (productScriptList.Count > 0) {
            UIManager.possibleActionsUI.AddAction("Hold RMB and click LMB - place object");
        }
        else {
            UIManager.possibleActionsUI.RemoveAction("Hold RMB and click LMB - place object");
        }
    }
}
