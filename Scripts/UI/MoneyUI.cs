using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MoneyUI : MonoBehaviour
{
    [SerializeField] private TMP_Text moneyText;
    void Start()
    {
        PlayerData.instance.OnPlayerMoneyChanged += UpdateUI;
        UpdateUI(PlayerData.instance.playerMoney);
    }

    void UpdateUI(float playerMoney)
    {
        if(playerMoney >= 10000)
            moneyText.text = "$" + playerMoney.ToString("0").Replace(',','.');
        else
            moneyText.text = "$" + playerMoney.ToString("0.00").Replace(',', '.');
    }
}
