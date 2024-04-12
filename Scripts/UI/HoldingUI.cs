using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HoldingUI : MonoBehaviour
{
    private GameObject UIGameObject;
    [SerializeField] private TMP_Text text;

    private void Start()
    {
        UIGameObject = transform.GetChild(0).gameObject;
        UIGameObject.SetActive(false);
    }
    
    public void UpdateText(int productAmount, ProductSO productType)
    {
        if(productAmount == 0 || productType.holdingLimit <= 1) {
            UIGameObject.SetActive(false);
            return;
        }
        text.text = productAmount + " / " + productType.holdingLimit;
        UIGameObject.SetActive(true);
    }
}
