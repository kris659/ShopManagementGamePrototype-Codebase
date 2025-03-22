using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticDoor : MonoBehaviour
{
    [SerializeField] private LayerMask peopleLayerMask;
    List<GameObject> peopleInRange = new List<GameObject>();

    [SerializeField] GameObject[] doors;
    [SerializeField] Transform[] doorsOpenTransform;
    [SerializeField] Transform[] doorsClosedTransform;
    [SerializeField] float openingTime = 0.5f;

    [SerializeField] Sound openingSound;
    [SerializeField] Sound closingSound;
    private void Start()
    {
        DOTween.Init();
        TriggerHandler triggerHandler = GetComponentInChildren<TriggerHandler>();

        triggerHandler.triggerEnter = OnDoorTriggerEnter;
        triggerHandler.triggerExit = OnDoorTriggerExit;
    }

    private void OpenDoor()
    {
        AudioManager.PlaySound(openingSound, transform.position);
        for(int i = 0; i < doors.Length; i++) {
            doors[i].transform.DOMove(doorsOpenTransform[i].position, openingTime);
            doors[i].transform.DORotate(doorsOpenTransform[i].eulerAngles, openingTime);
            doors[i].transform.DOScale(doorsOpenTransform[i].localScale, openingTime);            
        }
    }

    private void CloseDoor()
    {
        AudioManager.PlaySound(closingSound, transform.position);
        for (int i = 0; i < doors.Length; i++) {
            doors[i].transform.DOMove(doorsClosedTransform[i].position, openingTime);
            doors[i].transform.DORotate(doorsClosedTransform[i].eulerAngles, openingTime);
            doors[i].transform.DOScale(doorsClosedTransform[i].localScale, openingTime);
        }
    }



    private void OnDoorTriggerEnter(Collider other)
    {
        //Debug.Log(other.name + " " + LayerMask.LayerToName(other.gameObject.layer));
        if(other.gameObject.layer == LayerMask.NameToLayer("Player") || (other.gameObject.layer == LayerMask.NameToLayer("Customer"))) {
            if (peopleInRange.Contains(other.gameObject))
                return;
            peopleInRange.Add(other.gameObject);
            if (peopleInRange.Count == 1)
                OpenDoor();
        }
    }

    private void OnDoorTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player") || (other.gameObject.layer == LayerMask.NameToLayer("Customer"))) {
            peopleInRange.Remove(other.gameObject);
            if (peopleInRange.Count == 0)
                CloseDoor();
        }
    }
}
