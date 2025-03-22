using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoUI : WindowUI 
{
    [SerializeField] private Button closeButton;

    internal override void Awake()
    {
        base.Awake();
        closeButton.onClick.AddListener(CloseUI);
    }
}
