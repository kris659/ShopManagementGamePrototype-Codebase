using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LandUnlockUI : WindowUI
{
    [SerializeField] private List<Button> unlockButtons;
    [SerializeField] private List<int> unlockPrices;

    [SerializeField] private GameObject uiCamera;

    internal override void Awake()
    {
        base.Awake();
        for (int i = 0; i < unlockButtons.Count; i++) {
            int index = i;
            unlockButtons[index].onClick.AddListener(() => OnButtonClicked(index));
        }
    }

    public override void OpenUI()
    {
        uiCamera.SetActive(true);
        UpdateUI();
        base.OpenUI();        
    }

    public override void CloseUI()
    {
        uiCamera.SetActive(false);
        base.CloseUI();
    }

    private void UpdateUI()
    {
        for(int i = 0;i < unlockButtons.Count;i++) {
            TMP_Text buttonText = unlockButtons[i].GetComponentInChildren<TMP_Text>();

            if (ShopData.instance.floorsToUnlock[i].activeSelf) {
                buttonText.text = "Unlocked";
                unlockButtons[i].interactable = false;
            }
            else {
                buttonText.text = "$" + unlockPrices[i];
                unlockButtons[i].interactable =  PlayerData.instance.CanAfford(unlockPrices[i]);
            }
        }
    }

    private void OnButtonClicked(int buttonIndex)
    {
        if (ShopData.instance.floorsToUnlock[buttonIndex].activeSelf) {
            return;
        }
        string text = "Buy for $" + unlockPrices[buttonIndex];
        CloseUI();
        UIManager.confirmUI.OpenUI(text, ()  => OnSubmitButtonClicked(buttonIndex), null, true);
    }

    private void OnSubmitButtonClicked(int buttonIndex)
    {
        PlayerData.instance.TakeMoney(unlockPrices[buttonIndex]);
        ShopData.instance.floorsToUnlock[buttonIndex].SetActive(true);
        ShopPopularityManager.instance.UpdatePopularity(ShopPopularityCategory.ShopSize, ShopData.instance.GetUnlockedLandsCount() + 1);
        AudioManager.PlaySound(Sound.PlayerOrder);
        TasksManager.instance.ProgressTasks(TaskType.UnlockLand, 1);
    }
}
