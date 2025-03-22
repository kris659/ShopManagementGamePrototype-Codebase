using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pallet : MonoBehaviour, IInteractable
{
    public List<GameObject> spawnPositions;
    public GameObject sphere;
    public GameObject sphere2;
    public BoxCollider spawnTrigger;

    public string InteractionText => "LMB click and hold - move the pallet";
    public int InteractionTextSize => 100;

    private Rigidbody rb;
    private bool isPlayerInteracting;
    private float raycastDistance = 3;

    private float maxForce = 80f;
    private float maxForceDistance = 1.3f;
    private float maxVelocity = 2.5f;

    private Vector3 defaultPosition = new Vector3(0, -10, 0);

    private LayerMask defaultLayer;
    private LayerMask interactionLayer;
    //private AnimationCurve forceCurve;
    private void Awake()
    {
        InteractableCollider[] interactableColliders = GetComponentsInChildren<InteractableCollider>();
        foreach(InteractableCollider interactableCollider in interactableColliders) {
            interactableCollider.Init(this);
        }
        rb = GetComponent<Rigidbody>();
        sphere.transform.localPosition = defaultPosition;

        defaultLayer = LayerMask.NameToLayer("Pallet");
        interactionLayer = LayerMask.NameToLayer("Ignore Raycast");
    }

    private void Update()
    {
        if (isPlayerInteracting) {            
            GameObject mainCamera = PlayerInteractions.Instance.mainCamera;
            Vector3 origin = mainCamera.transform.position;
            Vector3 direction = mainCamera.transform.TransformDirection(Vector3.forward);
            RaycastHit hit;
            Vector3 currentPosition = mainCamera.transform.position + mainCamera.transform.forward * raycastDistance;
            if (Physics.Raycast(origin, direction, out hit, raycastDistance)) {
                if (sphere.transform.localPosition == defaultPosition){
                    sphere.transform.position = hit.point;
                    SetLayer(interactionLayer);
                }
                currentPosition = hit.point;
            }
            sphere2.transform.position = currentPosition;
            Vector3 interactionPoint = sphere.transform.position;
            Vector3 offset = currentPosition - interactionPoint;
            Vector3 forceDirection = offset.normalized;
            Vector3 force = forceDirection * Mathf.Clamp01(Mathf.Log(Mathf.Clamp01(offset.magnitude / maxForceDistance) + 1, 2)) * maxForce; // forceCurve.Evaluate(Mathf.Clamp01(offset.magnitude / maxForceDistance)) 
            if (Vector3.Dot(rb.GetPointVelocity(interactionPoint), -forceDirection) > 0) {
                force += force * Vector3.Dot(rb.GetPointVelocity(interactionPoint), -forceDirection) / 4;
            }
            if (Vector3.Dot(rb.GetPointVelocity(interactionPoint), forceDirection) > maxVelocity) {
                force = Vector3.zero;
            }
            force.y /= 6f;
            rb.AddForceAtPosition(force, interactionPoint);
            //Debug.Log("Distance: " + offset.magnitude + " force: " + force + " log: " + Mathf.Log(Mathf.Clamp01(offset.magnitude / maxForceDistance) + 1, 2));
            //Debug.Log("Dot: " + Vector3.Dot(rb.GetPointVelocity(interactionPoint), -forceDirection));
        }
    }

    public void OnMouseButtoDown()
    {
        //Debug.Log("Down");
        isPlayerInteracting = true;
    }

    public void OnMouseButton() { }

    public void OnMouseButtonUp()
    {
        //Debug.Log("Up");
        SetLayer(defaultLayer);
        isPlayerInteracting = false;
        sphere.transform.localPosition = defaultPosition;
    }

    public void OnPlayerButtonInteract() { }

    void SetLayer(LayerMask layer)
    {
        gameObject.layer = layer;
        Transform[] children = transform.GetComponentsInChildren<Transform>(includeInactive: true);
        foreach (Transform child in children) {
            child.gameObject.layer = layer;
        }
    }
}
