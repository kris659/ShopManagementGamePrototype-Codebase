using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InformationDisplayUI : MonoBehaviour
{
    private bool wasUpdatedThisFrame = false;
    private bool isOpen = false;
    private GameObject UIGameObject;
    [SerializeField] private TMP_Text displayText;
    RectTransform tr;
    private void Awake()
    {
        UIGameObject = transform.GetChild(0).gameObject;
        tr = UIGameObject.GetComponent<RectTransform>();
        CloseUI();
    }
    private void Update()
    {
        if (!wasUpdatedThisFrame && isOpen) {
            CloseUI();
        }
        wasUpdatedThisFrame = false;
    }

    public void UpdateText(string text)
    {
        wasUpdatedThisFrame = true;
        displayText.text = text;
        displayText.ForceMeshUpdate();
        tr.sizeDelta = displayText.textBounds.extents * 2 + new Vector3(20, 20);
        tr.position = new Vector3(tr.sizeDelta.x / 2, tr.sizeDelta.y / 2) + new Vector3(5, 5);

        if (text != "" && !isOpen) {
            OpenUI();
        }
        if(text == "" && isOpen) {
            CloseUI();
        }
    }

    private void OpenUI()
    {
        isOpen = true;
        UIGameObject.SetActive(true);
    }

    private void CloseUI()
    {
        isOpen = false;
        UIGameObject.SetActive(false);
    }
}
