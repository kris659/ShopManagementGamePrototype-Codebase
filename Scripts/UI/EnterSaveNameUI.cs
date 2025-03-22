using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnterSaveNameUI : MonoBehaviour
{
    [SerializeField] TMP_InputField inputField;
    [SerializeField] GameObject aleadyExistsGO;
    [SerializeField] Button submitButton;
    GameObject UIGameObject;
    List<string> saveNames = new List<string>();
    private void Awake()
    {
        UIGameObject = transform.GetChild(0).gameObject;
        submitButton.onClick.AddListener(OnSubmitButton);
        inputField.onValueChanged.AddListener(OnInputFieldValueChanged);
        CloseUI();
    }

    public void OpenUI(List<string> saveNames)
    {
        this.saveNames = saveNames;
        UIGameObject.SetActive(true);
        inputField.text = "";
        OnInputFieldValueChanged(string.Empty);
    }

    public void CloseUI()
    {
        UIGameObject.SetActive(false);
    }

    private void OnSubmitButton()
    {
        MainMenu.instance.OnNewGameNameSelected(inputField.text);
    }
    private void OnInputFieldValueChanged(string value)
    {
        if(value == string.Empty) {
            submitButton.interactable = false;
            aleadyExistsGO.SetActive(false);
            return;
        }
        if (saveNames.Contains(value)){
            submitButton.interactable = false;
            aleadyExistsGO.SetActive(true);
            return;
        }
        aleadyExistsGO.SetActive(false);
        submitButton.interactable = true;
    }
}
