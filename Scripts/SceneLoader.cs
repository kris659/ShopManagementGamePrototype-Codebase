using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static Action OnUISceneLoaded;
    public static Action OnWorldSceneLoaded;
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
    }
}
