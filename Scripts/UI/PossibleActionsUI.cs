using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PossibleActionsUI : MonoBehaviour
{
    [SerializeField] private GameObject elementPrefab;
    [SerializeField] private GameObject elementsParent;
    private List<TMP_Text> activeElements = new List<TMP_Text>();


    public void AddAction(string actionText, int height=60)
    {
        for (int i = 0; i < activeElements.Count; i++) {
            if (activeElements[i].text == actionText) {
                return;
            }
        }
        GameObject newElement = Instantiate(elementPrefab, elementsParent.transform);
        RectTransform rectTransform = newElement.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, height);
        newElement.SetActive(true);
        TMP_Text elementText = newElement.GetComponentInChildren<TMP_Text>();
        elementText.text = actionText;
        activeElements.Add(elementText);
    }

    public void RemoveAction(string actionText)
    {
        for(int i = 0; i  < activeElements.Count; i++) {
            if (activeElements[i].text == actionText) {
                Destroy(activeElements[i].transform.parent.gameObject);
                activeElements.RemoveAt(i);
                return;
            }
        }
    }
}
