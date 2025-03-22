using System.Collections;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System.Drawing;

public class ArcadeMachine : Facility
{
    [Header("Claw Settings")]
    [SerializeField] private Transform claw;
    [SerializeField] private Transform clawBar;
    private float moveDuration = 1.5f;
    private float returnDuration = 1f;
    private float xMoveRange = 0.37f;
    private Vector2 clawYPositions = new(-0.64f, -0.065f);
    private Vector2 clawBarYPositions = new(0.32f, 0.02f);
    private Vector2 clawBarScales = new(0.61f, 0.04f);

    [Header("Prizes")]
    [SerializeField] private List<GameObject> prizes = new();

    private GameObject currentPrize;
    private Vector3 basicPrizePosition;

    private void Start() => DOTween.Init();

    public override void UseFacility() => StartCoroutine(UseFacilityRoutine());

    private IEnumerator UseFacilityRoutine()
    {
        yield return claw.DOLocalMoveX(Random.Range(-xMoveRange, xMoveRange), moveDuration).WaitForCompletion();

        var downSequence = DOTween.Sequence()
            .Join(claw.DOLocalMoveY(clawYPositions.x, moveDuration))
            .Join(clawBar.DOLocalMoveY(clawBarYPositions.x, moveDuration))
            .Join(clawBar.DOScaleY(clawBarScales.x, moveDuration));

        yield return downSequence.WaitForCompletion();

        currentPrize = prizes[Random.Range(0, prizes.Count)];

        if (currentPrize != null)
        {
            currentPrize.SetActive(true);
            basicPrizePosition = currentPrize.transform.localPosition;
            yield return new WaitForSeconds(1);
        }

        var upSequence = DOTween.Sequence()
            .Join(claw.DOLocalMoveY(clawYPositions.y, moveDuration))
            .Join(clawBar.DOLocalMoveY(clawBarYPositions.y, moveDuration))
            .Join(clawBar.DOScaleY(clawBarScales.y, moveDuration));

        yield return upSequence.WaitForCompletion();

        if (currentPrize != null)
        {
            yield return claw.DOLocalMoveX(0.2f, returnDuration).WaitForCompletion();
            yield return claw.DOLocalMoveZ(0.28f, returnDuration).WaitForCompletion();
            currentPrize.AddComponent<Rigidbody>();
            yield return new WaitForSeconds(2);
            yield return claw.DOLocalMoveZ(0, returnDuration).WaitForCompletion();

            GameObject NewPrize = Instantiate(currentPrize);
            Destroy(NewPrize.GetComponent<Rigidbody>());
            NewPrize.transform.SetParent(myCustomer.GetComponent<Customer>().grabPlace);
            NewPrize.transform.localPosition = Vector3.zero;
            currentPrize.SetActive(false);

            currentPrize.transform.localPosition = basicPrizePosition;
            Destroy(currentPrize.GetComponent<Rigidbody>());
        }
        yield return claw.DOLocalMoveX(0, returnDuration).WaitForCompletion();

        yield return new WaitForSeconds(2);
        ExitFacility();
    }

    public override void ExitFacility()
    {
        base.ExitFacility();
        int amount = Random.Range(1, 3);
        PlayerData.instance.AddMoney(amount, true);
    }
}