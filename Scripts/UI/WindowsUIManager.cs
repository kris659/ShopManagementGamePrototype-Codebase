using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WindowsUIManager : WindowUI
{
    [SerializeField] Transform buttonsParent;
    [SerializeField] GameObject buttonPrefab;

    [SerializeField] internal List<WindowUI> windowsUI;
    [SerializeField] List<Button> buttons;
    [SerializeField] List<KeyCode> windowsUIKeyCode;
    [SerializeField] List<string> windowsUINames;
    
    public WindowUI currentlyOpenWindow;
    public bool blockInput => currentlyOpenWindow != null && currentlyOpenWindow.isOpen;

    internal virtual void Start()
    {
        CreateButtons();
        foreach(WindowUI window in windowsUI) {
            window.Init(this);
        }
    }

    internal virtual void Update()
    {
        if (MainMenu.isMainMenuOpen)
            return;
        for (int i = 0; i < windowsUI.Count; i++) {
            if (Input.GetKeyDown(windowsUIKeyCode[i]) && (currentlyOpenWindow == null || currentlyOpenWindow.canClose)) {
                OpenWindow(windowsUI[i]);
            }
        }        
    }

    public override void OpenUI()
    {
        base.OpenUI();
        if (currentlyOpenWindow != null) {
            currentlyOpenWindow.OpenUI();
        }
    }

    public Button GetWindowUIButton(WindowUI windowUI)
    {
        for(int i = 0;i < windowsUI.Count;i++) {
            if (windowsUI[i] == windowUI && buttons.Count > i) {
                return buttons[i];
            }
        }
        return null;
    }

    internal virtual void OpenWindow(WindowUI windowUI)
    {
        if(windowUI.isOpen) {
            windowUI.CloseUI();
            return;
        }
        if(currentlyOpenWindow != null) {
            currentlyOpenWindow.CloseUI();
        }
        windowUI.OpenUI();
        currentlyOpenWindow = windowUI;
    }
    private void CreateButtons()
    {
        DestroyChildren();
        for (int i = 0; i < windowsUI.Count; i++) {
            GameObject buttonGO = Instantiate(buttonPrefab, buttonsParent);
            buttonGO.SetActive(true);
            TMP_Text text = buttonGO.GetComponentInChildren<TMP_Text>();
            Button button = buttonGO.GetComponent<Button>();
            int index = i;
            buttons.Add(button);
            button.onClick.AddListener( () => { OpenWindow(windowsUI[index]); EventSystem.current.SetSelectedGameObject(null); });
            text.text = windowsUINames[i];
        }
    }

    void DestroyChildren()
    {
        for(int i = 0; i < buttonsParent.childCount; i++) {
            if(buttonsParent.GetChild(i).gameObject.activeSelf)
                Destroy(buttonsParent.GetChild(i).gameObject);
        }
    }
}
