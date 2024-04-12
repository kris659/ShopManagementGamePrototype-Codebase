using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadMapScene : MonoBehaviour
{
    void Start()
    {
        if(SceneManager.loadedSceneCount == 1)
            SceneManager.LoadSceneAsync("malowanko", LoadSceneMode.Additive);
    }

    private void Update()
    {
        //if(Input.GetKeyDown(KeyCode.F)) {
        //    SceneManager.LoadSceneAsync("malowanko", LoadSceneMode.Additive);
        //}
    }
}
