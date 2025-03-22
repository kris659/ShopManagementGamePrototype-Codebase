using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadGameUI : MonoBehaviour
{
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Transform listParent;

    public bool CanLoadGame => saveNames.Count > 0;

    public List<string> saveNames = new List<string>();
    private List<DateTime> lastWriteTime = new List<DateTime>();
    private MainMenu mainMenu;

    private void Awake()
    {
        mainMenu = GetComponentInParent<MainMenu>(includeInactive: true);
    }


    public void UpdateSavesList()
    {
        SaveFilesManager.GetSaveFilesData(out saveNames, out lastWriteTime);
        CloseUI();
        for (int i = 0; i < listParent.childCount; i++) {
            listParent.GetChild(i).gameObject.SetActive(false);
        }
    }

    public void OpenUI()
    {
        CreateList();
        transform.GetChild(0).gameObject.SetActive(true);    
    }

    private void CreateList()
    {
        SaveFilesManager.GetSaveFilesData(out saveNames, out lastWriteTime);
        for (int j = 0; j < listParent.childCount; j++) {
            if (listParent.GetChild(j).gameObject.activeSelf) {
                Destroy(listParent.GetChild(j).gameObject);
            }
        }
        for (int i = 0; i < saveNames.Count; i++) {
            GameObject buttonGO = Instantiate(buttonPrefab, listParent);
            buttonGO.SetActive(true);
            TMP_Text saveNameText = buttonGO.transform.GetChild(0).GetComponent<TMP_Text>();
            TMP_Text saveWriteTimeText = buttonGO.transform.GetChild(1).GetComponent<TMP_Text>();
            Button button = buttonGO.GetComponent<Button>();

            saveNameText.text = saveNames[i];
            if (lastWriteTime[i].Date == DateTime.Today)
                saveWriteTimeText.text = lastWriteTime[i].ToString("HH:mm");
            else
                saveWriteTimeText.text = lastWriteTime[i].ToString("d");

            int index = i;
            button.onClick.AddListener(() => OnButtonPressed(index));
        }

        RectTransform rectTransform = listParent.GetComponent<RectTransform>();
        float width = rectTransform.sizeDelta.x;
        float height = Mathf.Max(saveNames.Count * 80, 529);
        rectTransform.sizeDelta = new Vector2(width, height);
        rectTransform.localPosition = new Vector2 (rectTransform.localPosition.x, -(height) / 2);
        if(saveNames.Count > 6) {
            listParent.GetComponent<LayoutGroup>().childAlignment = TextAnchor.UpperCenter;
        }
        else {
            listParent.GetComponent<LayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
        }
    }

    private void OnButtonPressed(int buttonIndex)
    {
        CloseUI();
        mainMenu.OnSaveSelected(saveNames[buttonIndex]);
    }
    public void CloseUI()
    {
        transform.GetChild(0).gameObject.SetActive(false);
    }
}
