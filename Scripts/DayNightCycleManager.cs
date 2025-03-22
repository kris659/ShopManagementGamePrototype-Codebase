using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycleManager : MonoBehaviour
{
    [SerializeField] private bool overrideCurrentTime;
    [Range(0,24)][SerializeField] private float currentTime;

    [SerializeField] private GameObject sunGameObject;
    [SerializeField] private float sunStartingRotationX;
    [SerializeField] private float sunStartingRotationY;
    [SerializeField] private float sunRotationXChange;

    [SerializeField] private AnimationCurve sunIntensity;

    [SerializeField] private Gradient ambientColor;
    [SerializeField] private AnimationCurve ambientIntensity;
    [SerializeField] private AnimationCurve environmentReflectionsIntensity;
    [SerializeField] private Gradient fogColor;
    [SerializeField] private AnimationCurve fogDensity;

    private Light sunLight;
    float sunRotationX;
    float sunRotationY;

    private void Awake()
    {
        sunLight = sunGameObject.GetComponent<Light>();
    }

    private void Start()
    {
        StartCoroutine(UpdateCoroutine());
        TimeManager.instance.OnHourChanged += UpdateLighting;
    }

    private void Update()
    {
        UpdateSunPosition();
    }

    private IEnumerator UpdateCoroutine()
    {
        UpdateLighting();
        yield return new WaitForSeconds(1);
        StartCoroutine(UpdateCoroutine());
    }

    private float GetCurrentTime()
    {
        if (overrideCurrentTime)
            return currentTime / 24;        
        return (TimeManager.instance.Hour + TimeManager.instance.MinuteFloat / 60f) / 24f;
    }

    private void UpdateSunPosition()
    {
        float time = GetCurrentTime();
        if (time < 0.5f)
            sunRotationX = 2 * sunRotationXChange * time / 0.5f - sunRotationXChange;
        else
            sunRotationX = 2 * sunRotationXChange * Mathf.InverseLerp(1, .5f, time) - sunRotationXChange;
        sunRotationY = sunStartingRotationY + time * 360;
        sunRotationX += sunStartingRotationX;
        sunGameObject.transform.eulerAngles = new Vector3(sunRotationX, sunRotationY, 0);
    }

    private void UpdateLighting()
    {
        float time = GetCurrentTime();
        RenderSettings.ambientLight = ambientColor.Evaluate(time);
        RenderSettings.ambientIntensity = ambientIntensity.Evaluate(time);
        RenderSettings.reflectionIntensity = environmentReflectionsIntensity.Evaluate(time);
        RenderSettings.fogColor = fogColor.Evaluate(time);
        RenderSettings.fogDensity = 0.005f * fogDensity.Evaluate(time);
        sunLight.intensity = sunIntensity.Evaluate(time) * 2;
        if (sunRotationX < 10)
            sunLight.intensity = 0;
    }
}
