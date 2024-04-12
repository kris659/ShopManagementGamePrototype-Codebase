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
    }

    void UpdateUI(int playerMoney)
    {
        moneyText.text = playerMoney.ToString() + "$";
    }
}
