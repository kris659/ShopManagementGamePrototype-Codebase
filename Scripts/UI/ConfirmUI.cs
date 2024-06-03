using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmUI : WindowUI
{
    [SerializeField] private TMP_Text mainText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    Action confirmAction;
    Action cancelAction;

    internal override void Awake()
    {
        base.Awake();
        confirmButton.onClick.AddListener(OnConfirmButtonPressed);
        cancelButton.onClick.AddListener(OnCancelButtonPressed);
    }

    public void OpenUI(string text, Action confirmAction, Action cancelAction, bool isConfirmEnabled)
    {
        mainText.text = text;
        confirmButton.interactable = isConfirmEnabled;
        this.confirmAction = confirmAction;
        this.cancelAction = cancelAction;
        OpenUI();
    }
    
    private void OnConfirmButtonPressed()
    {
        CloseUI();
        confirmAction?.Invoke();        
    }

    private void OnCancelButtonPressed()
    {
        CloseUI();
        cancelAction?.Invoke();        
    }
}
