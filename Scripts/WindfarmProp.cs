using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindfarmProp : MonoBehaviour
{
    [SerializeField] private Transform rotatingPart;
    [SerializeField] private float rotationSpeed;
    private void Awake()
    {
        rotationSpeed = rotationSpeed * Random.Range(0.8f, 1.2f);
        DOTween.Init(this);
        Rotate();
    }

    private void Rotate()
    {
        rotatingPart.DOBlendableLocalRotateBy(new Vector3(0, 0, 120), 1 / rotationSpeed).SetLoops(-1).SetEase(Ease.Linear);
    }
}
