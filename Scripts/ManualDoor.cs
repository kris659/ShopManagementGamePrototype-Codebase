using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualDoor : MonoBehaviour, IInteractable
{
    public float openingDuration = 0.5f;
    public bool areDoorsOpen = false;
    public bool areDoorsOpeningOrClosing = false;

    [SerializeField] GameObject[] doors;
    Collider[] doorsColliders;
    [SerializeField] Vector3[] doorsRotations;

    public string textToDisplay
    {
        get {
            if (!areDoorsOpen)
                return "E - open doors";
            else
                return "E - close doors";
        }
    }

    private void Start()
    {
        doorsColliders = new Collider[doors.Length];
        for (int i = 0; i < doors.Length; i++) {
            doorsColliders[i] = doors[i].GetComponentInChildren<Collider>();
        }

        InteractableCollider[] interactableColliders = GetComponentsInChildren<InteractableCollider>();
        for (int i = 0; i < interactableColliders.Length; i++)
            interactableColliders[i].Init(this);

        DOTween.Init();
    }

    private IEnumerator OpenDoors()
    {
        for(int i = 0; i < doors.Length; i++) {
            doors[i].transform.DOLocalRotate(doorsRotations[i], openingDuration);
            doorsColliders[i].isTrigger = true;
        }
        yield return new WaitForSeconds(openingDuration);
        areDoorsOpen = true;
        areDoorsOpeningOrClosing = false;
    }

    private IEnumerator CloseDoors()
    {
        for (int i = 0; i < doors.Length; i++) {
            doors[i].transform.DOLocalRotate(Vector3.zero, openingDuration);
            doorsColliders[i].isTrigger = true;
        }
        yield return new WaitForSeconds(openingDuration);
        for (int i = 0; i < doors.Length; i++) {
                doorsColliders[i].isTrigger = false;                
        }
        areDoorsOpen = false;
        areDoorsOpeningOrClosing = false;
    }

    public void OnPlayerInteract()
    {
        if (areDoorsOpeningOrClosing)
            return;
        areDoorsOpeningOrClosing = true;
        if (areDoorsOpen)
            StartCoroutine(CloseDoors());
        else
            StartCoroutine(OpenDoors());        
    }
}
