using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftPanelUI : WindowsUIManager
{
    [SerializeField] private ConfirmUI confirmUI;
    [SerializeField] private PauseMenuUI pauseMenuUI;
    [SerializeField] private NextDayUI nextDayUI;
    [SerializeField] private InfoUI infoUI;
    [SerializeField] private InputFieldUI inputFieldUI;
    [SerializeField] private FurnitureShopUI furnitureShopUI;

    internal override void Awake()
    {
        base.Awake();
        confirmUI.Init(this);
        pauseMenuUI.Init(this);
        nextDayUI.Init(this);
        infoUI.Init(this);
        inputFieldUI.Init(this);
        furnitureShopUI.Init(this);
    }

    internal override void Update()
    {
        base.Update();
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if(currentlyOpenWindow != null) {
                currentlyOpenWindow.CloseUI();
            }
            else {
                if(!MainMenu.isMainMenuOpen && !BuildingManager.instance.IsBuilding && !BuildingManager.instance.isRemovingBuildings)
                    pauseMenuUI.OpenUI();
            }
        }
    }
}
