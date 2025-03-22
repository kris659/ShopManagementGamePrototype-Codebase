using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicFacility : Facility
{
    public int timeOfUse;
    public GameObject item;

    private void Start() => DOTween.Init();

    public override void UseFacility() => StartCoroutine(UseFacilityRoutine());

    private IEnumerator UseFacilityRoutine()
    {
        yield return new WaitForSeconds(timeOfUse);
        Instantiate(item, myCustomer.GetComponent<Customer>().grabPlace);
        ExitFacility();
    }

    public override void ExitFacility()
    {
        base.ExitFacility();
        int amount = Random.Range(0, 2);
        PlayerData.instance.AddMoney(amount, true);
    }
}
