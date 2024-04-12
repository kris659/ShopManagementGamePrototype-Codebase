using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextUI : MonoBehaviour
{
    [SerializeField] private TMP_Text textUI;
    private GameObject textUIGO;
    private string text;
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
        textUI.text = textToDisplay;

        if (duration > 0)
            StartCoroutine(ClearTextAfter(duration));
    }

    IEnumerator ClearTextAfter(float duration)
    {
        yield return new WaitForSeconds(duration);        
        UpdateText(text);
    }
}