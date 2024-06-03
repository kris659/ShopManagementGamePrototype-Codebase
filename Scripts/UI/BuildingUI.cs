using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BuildingUI : WindowUI
{
    private BuildingManager buildingManager;

    [SerializeField] private Transform categoryListParent;
    [SerializeField] private GameObject categoryListElementPrefab;

    [SerializeField] private Transform selectionListParent;
    [SerializeField] private GameObject selectionListRowPrefab;
    [SerializeField] private GameObject selectionListElementPrefab;

    [SerializeField] private Button destroyButton;

    private List<GameObject> categoryList = new List<GameObject>();
    private List<GameObject> selectionListParents = new List<GameObject>();
    private List<GameObject> selectionList = new List<GameObject>();

    private int currentlySelectedCategory = 0;

    public void Init(BuildingManager buildingManager)
    {
        this.buildingManager = buildingManager;
        destroyButton.onClick.AddListener(() =>
        {
            CloseUI();
            buildingManager.OnDestroyButtonClicked();
        });
    }
    private void CreateCategoryList()
    {
        DestroyAllElements(categoryList);
        string[] buildingTypeNames = SOData.GetBuildingTypesNames();
        for (int i = 0; i < buildingTypeNames.Length; i++) {
            GameObject gameObject = Instantiate(categoryListElementPrefab, categoryListParent);            
            TMP_Text text = gameObject.transform.GetChild(1).GetComponent<TMP_Text>();
            Button button = gameObject.GetComponent<Button>();
            int buttonIndex = i;
            button.onClick.AddListener(() => OnCategoryButtonClicked(buttonIndex));
            text.text = buildingTypeNames[i];
            gameObject.SetActive(true);
            categoryList.Add(gameObject);
        }
    }

    private void OnCategoryButtonClicked(int buttonIndex)
    {
        currentlySelectedCategory = buttonIndex;
        DestroyAllElements(selectionList);
        DestroyAllElements(selectionListParents);
        IListable[] listables = SOData.GetListables(buttonIndex);

        int maxElementsInRow = 4;
        GameObject rowParent = null;
        for (int i = 0; i < listables.Length; i++) {
            if (i % maxElementsInRow == 0) {
                rowParent = Instantiate(selectionListRowPrefab, selectionListParent);
                rowParent.SetActive(true);
                selectionListParents.Add(rowParent);
            }
            GameObject listElement = Instantiate(selectionListElementPrefab, rowParent.transform);            
            Button button = listElement.GetComponent<Button>();
            TMP_Text nameText = listElement.transform.GetChild(1).GetComponent<TMP_Text>();
            TMP_Text priceText = listElement.transform.GetChild(2).GetComponent<TMP_Text>();
            nameText.text = listables[i].Name;
            priceText.text = listables[i].Price + "$";
            selectionList.Add(listElement);
            listElement.SetActive(true);
            int buildingTypeButtonIndex = i;
            button.onClick.AddListener(() => OnSelectionButtonClicked(buildingTypeButtonIndex));
        }
    }

    private void OnSelectionButtonClicked(int buttonIndex)
    {
        buildingManager.SelectBuilding(currentlySelectedCategory, buttonIndex);
        CloseUI();
    }

    public override void OpenUI()
    {
        base.OpenUI();
        CreateCategoryList();
        OnCategoryButtonClicked(currentlySelectedCategory);
    }

    private void DestroyAllElements(List<GameObject> elements)
    {
        for(int i = 0; i < elements.Count; i++) {
            Destroy(elements[i]);
        }
        elements.Clear();
    }

    private IListable[] GetListables(int buttonIndex)
    {
        switch (buttonIndex) {
            case 0:
                return SOData.shelvesList;
            case 1:
                return SOData.wallsList;
            case 2:
                return SOData.registersList;
            default:
                return null;
        }
    }
}
