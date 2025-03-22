using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WarningsUI : MonoBehaviour
{
    [SerializeField] private GameObject customersWarning;

    private int customersCount = 0;

    private void Awake()
    {
        UpdateCustomersWarning();
    }

    public void AddCustomer()
    {
        if(customersCount == 0)
            AudioManager.PlaySound(Sound.NegativeNotification);

        customersCount++;
        UpdateCustomersWarning();
    }

    public void RemoveCustomer()
    {
        customersCount--;
        UpdateCustomersWarning();
    }

    private void UpdateCustomersWarning()
    {
        if(customersCount == 0) {
            customersWarning.SetActive(false);
            return;
        }
        customersWarning.SetActive(true);
        TMP_Text countText = customersWarning.transform.GetChild(1).GetComponent<TMP_Text>();
        TMP_Text warningText = customersWarning.transform.GetChild(2).GetComponent<TMP_Text>();

        countText.text = customersCount.ToString();
        if (customersCount == 1) {
            warningText.text = "customer is waiting for checkout";
        }
        else {
            warningText.text = "customers are waiting for checkout";
        }
    }

    public void ClearWarnings()
    {
        customersCount = 0;
        UpdateCustomersWarning();
    }
}
