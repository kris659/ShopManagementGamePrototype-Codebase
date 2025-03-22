using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CarController : MonoBehaviour, IInteractable
{
    public bool areDoorsOpen = false;

    [SerializeField] GameObject leftDoor;
    [SerializeField] Collider leftDoorCollider;
    [SerializeField] GameObject rightDoor;
    [SerializeField] Collider rightDoorCollider;

    [SerializeField] Vector3 leftDoorRotation;
    [SerializeField] Vector3 rightDoorRotation;

    public string InteractionText { get; } = "F - Open doors";
    public int InteractionTextSize => 60;

    public void OnMouseButtoDown() { }
    public void OnMouseButton() { }
    public void OnMouseButtonUp() { }

    private void Start()
    {
        leftDoorCollider = leftDoor.GetComponentInChildren<Collider>();
        rightDoorCollider = rightDoor.GetComponentInChildren<Collider>();

        DOTween.Init();
    }

    private void OpenDoors()
    {
        leftDoor.transform.DOLocalRotate(leftDoorRotation, 0.5f);
        rightDoor.transform.DOLocalRotate(rightDoorRotation, 0.5f);

        leftDoorCollider.enabled = false;
        rightDoorCollider.enabled = false;
    }

    private void CloseDoors()
    {
        leftDoor.transform.DOLocalRotate(Vector3.zero, 0.5f);
        rightDoor.transform.DOLocalRotate(Vector3.zero, 0.5f);

        leftDoorCollider.enabled = true;
        rightDoorCollider.enabled = true;
    }

    public void OnPlayerButtonInteract()
    {
        if(areDoorsOpen)
            CloseDoors();
        else
            OpenDoors();
    }
}
