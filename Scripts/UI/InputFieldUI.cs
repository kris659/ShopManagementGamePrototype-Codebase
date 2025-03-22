using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputFieldUI : WindowUI
{
    public delegate bool ErrorCheck(string message);

    [SerializeField] TMP_InputField inputField;
    [SerializeField] TMP_Text titleText;
    [SerializeField] TMP_Text errorText;
    [SerializeField] Button submitButton;

    ErrorCheck errorCheck;
    Action<string> submitAction;

    public override bool canClose => false;

    internal override void Awake()
    {
        base.Awake();
        submitButton.onClick.AddListener(OnSubmitButton);
        inputField.onValueChanged.AddListener(OnInputFieldValueChanged);
    }

    public void OpenUI(string titleText, string startingText, string errorText, ErrorCheck errorCheck, Action<string> submitAction)
    {
        base.OpenUI();
        inputField.text = startingText;
        this.errorText.text = errorText;
        this.errorCheck = errorCheck;
        this.submitAction = submitAction;
        this.titleText.text = titleText;
        inputField.Select();    
    }

    private void OnSubmitButton()
    {
        submitAction?.Invoke(inputField.text);
        CloseUI();
    }
    private void OnInputFieldValueChanged(string value)
    {
        if (value == "") {
            submitButton.interactable = false;
            errorText.gameObject.SetActive(false);
            return;
        }
        if (errorCheck != null && errorCheck(value)) {
            submitButton.interactable = false;
            if(errorText.text != "")
                errorText.gameObject.SetActive(true);
            return;
        }
        errorText.gameObject.SetActive(false);
        submitButton.interactable = true;
    }
}
