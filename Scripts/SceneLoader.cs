using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public static Action OnUISceneLoaded;
    public static Action OnWorldSceneLoaded;
    public static Action OnAudioSceneLoaded;

    public static bool isUISceneLoaded;
    public static bool isWorldSceneLoaded;

    private void Awake()
    {
        HandleStartingImage();
        OnUISceneLoaded += () => isUISceneLoaded = true;
        OnWorldSceneLoaded += () => isWorldSceneLoaded = true;
    }

    void Start()
    {
        LoadScenes();
    }

    private void LoadScenes()
    {
        if (!SceneManager.GetSceneByName("UI").isLoaded) {
            SceneManager.LoadSceneAsync("UI", LoadSceneMode.Additive).completed += (AsyncOperation a) => OnUISceneLoaded?.Invoke();
        }
        else {
            OnUISceneLoaded?.Invoke();
        }
        if (!SceneManager.GetSceneByName("dwalowanko").isLoaded) {
            SceneManager.LoadSceneAsync("dwalowanko", LoadSceneMode.Additive).completed += (AsyncOperation a) => OnWorldSceneLoaded?.Invoke();
        }
        else {
            OnWorldSceneLoaded?.Invoke();
        }
        if (!SceneManager.GetSceneByName("Audio").isLoaded) {
            SceneManager.LoadSceneAsync("Audio", LoadSceneMode.Additive).completed += (AsyncOperation a) => OnAudioSceneLoaded?.Invoke();
        }
        else {
            OnAudioSceneLoaded?.Invoke();
        }
    }

    private void HandleStartingImage()
    {
        DOTween.Init(this);
        Image image = GetComponentInChildren<Image>(includeInactive: true);
        transform.GetChild(0).gameObject.SetActive(true);
        Color color = image.color;
        color.a = 0;
        OnWorldSceneLoaded += () => image.DOColor(color, 1.5f).onComplete += () => Destroy(transform.GetChild(0).gameObject);
    }
}
