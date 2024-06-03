using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextUI : MonoBehaviour
{
    [SerializeField] private TMP_Text textUI;
    private GameObject textUIGO;
    private string text;
    private IEnumerator coroutine;
    private float currentDuration;
    void Start()
    {
        textUIGO = transform.GetChild(0).gameObject;
        UpdateText(string.Empty);
    }
    public void UpdateText(string textToDisplay, float duration = 0)
    {
        this.text = textToDisplay;
        if(textToDisplay == string.Empty)
            textUIGO.SetActive(false);
        else 
            textUIGO.SetActive(true);

        
        if(coroutine != null) {
            if (textToDisplay != textUI.text)
                StopCoroutine(coroutine);            
        }
        currentDuration = duration;
        textUI.text = textToDisplay;
        if(duration > 0 && coroutine == null) {
            coroutine = ClearTextAfter();
            StartCoroutine(coroutine);
        }        
    }

    IEnumerator ClearTextAfter()
    {
        while(currentDuration > 0) {
            float duration = Mathf.Min(currentDuration, 0.1f);
            currentDuration -= duration;
            yield return new WaitForSeconds(duration);
        }        
        UpdateText("");
        coroutine = null;
    }
}