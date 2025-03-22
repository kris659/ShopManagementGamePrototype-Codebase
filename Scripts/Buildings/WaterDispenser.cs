using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterDispenser : Facility
{
    [Header("Water Tank")]
    [SerializeField] private Transform WaterTank;
    public GameObject CupOfWater;

    private void Start() => DOTween.Init();

    public override void UseFacility() => StartCoroutine(UseFacilityRoutine());

    private IEnumerator UseFacilityRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        yield return WaterTank.DOLocalRotate(
            new Vector3(0, WaterTank.transform.localEulerAngles.y + 360, 0),
            1,
            RotateMode.FastBeyond360
        ).WaitForCompletion();
        yield return new WaitForSeconds(1);
        Instantiate(CupOfWater, myCustomer.GetComponent<Customer>().grabPlace);
        ExitFacility();
    }

    public override void ExitFacility()
    {
        base.ExitFacility();
        PlayerData.instance.AddMoney(1, true);
    }
}
