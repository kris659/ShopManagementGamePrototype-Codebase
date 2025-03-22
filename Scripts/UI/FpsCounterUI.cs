using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class FpsCounterUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _fpsText;    
    [SerializeField] private float _hudRefreshRate = 1f;

    private int _counter;

    private void Start()
    {
        StartCoroutine(FPSDisplay());
    }

    private void Update()
    {
        _counter++;
    }

    IEnumerator FPSDisplay()
    {
        _counter = 0;
        yield return new WaitForSecondsRealtime(_hudRefreshRate);
        _fpsText.text = "FPS: " + (_counter / _hudRefreshRate).ToString("0.0").Replace(",", ".");
        StartCoroutine(FPSDisplay());
    }
}
