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
    [SerializeField] private Button moveButton;

    public List<GameObject> categoryList = new List<GameObject>();
    public List<GameObject> selectionListParents = new List<GameObject>();
    public List<GameObject> selectionList = new List<GameObject>();

    private int currentlySelectedCategory = 1;

    public void Init(BuildingManager buildingManager)
    {
        this.buildingManager = buildingManager;
        destroyButton.onClick.AddListener(() =>
        {
            CloseUI();
            buildingManager.OnDestroyButtonClicked();
        });
        moveButton.onClick.AddListener(() =>
        {
            CloseUI();
            buildingManager.OnMoveButtonClicked();
        });
    }
    private void CreateCategoryList()
    {
        DestroyAllElements(categoryList);
        for (int i = 1; i < buildingManager.buildingCategories.Count; i++) {
            GameObject gameObject = Instantiate(categoryListElementPrefab, categoryListParent);            
            TMP_Text text = gameObject.transform.GetChild(1).GetComponent<TMP_Text>();
            Button button = gameObject.GetComponent<Button>();
            int buttonIndex = i;
            button.onClick.AddListener(() => OnCategoryButtonClicked(buttonIndex));
            text.text = buildingManager.buildingCategories[i].name;
            gameObject.SetActive(true);
            categoryList.Add(gameObject);
        }

        RectTransform categoryListTransform = categoryListParent.GetComponent<RectTransform>();
        float width = categoryListTransform.sizeDelta.x;
        float height = Mathf.Max(8 + (buildingManager.buildingCategories.Count - 1) * 76.5f, 0);
        categoryListTransform.sizeDelta = new Vector2(width, height);
        categoryListTransform.localPosition = new Vector2(categoryListTransform.localPosition.x, 0);
    }

    private void OnCategoryButtonClicked(int buttonIndex)
    {
        currentlySelectedCategory = buttonIndex;
        DestroyAllElements(selectionList);
        DestroyAllElements(selectionListParents);
        IListable[] listables = buildingManager.buildingCategories[buttonIndex].buildings;

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
            Image image = listElement.transform.GetChild(1).GetComponent<Image>();
            TMP_Text nameText = listElement.transform.GetChild(2).GetComponent<TMP_Text>();
            TMP_Text priceText = listElement.transform.GetChild(3).GetComponent<TMP_Text>();
            image.sprite = listables[i].Icon;
            nameText.text = listables[i].Name;
            priceText.text = listables[i].Price + "$";
            selectionList.Add(listElement);
            listElement.SetActive(true);
            int buildingTypeButtonIndex = i;
            button.onClick.AddListener(() => OnSelectionButtonClicked(buildingTypeButtonIndex));
        }
        RectTransform selectionListTransform = selectionListParent.GetComponent<RectTransform>();
        float width = selectionListTransform.sizeDelta.x;
        float height = Mathf.Max(selectionListParents.Count * 210, 0);
        selectionListTransform.sizeDelta = new Vector2(width, height);
        selectionListTransform.localPosition = new Vector2(selectionListTransform.localPosition.x, -height / 4);        

        //rectTransform.offsetMin = new Vector2(0, rectTransform.offsetMin.y);
        //rectTransform.offsetMax = new Vector2(0, rectTransform.offsetMin.y);
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
}
