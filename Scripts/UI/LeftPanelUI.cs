using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftPanelUI : WindowsUIManager
{
    [SerializeField] private ConfirmUI confirmUI;
    [SerializeField] private PauseMenuUI pauseMenuUI;

    internal override void Awake()
    {
        base.Awake();
        confirmUI.Init(this);
        pauseMenuUI.Init(this);
    }

    internal override void Update()
    {
        base.Update();
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if(currentlyOpenWindow != null) {
                currentlyOpenWindow.CloseUI();
            }
            else {
                pauseMenuUI.OpenUI();
            }
        }
    }
}
