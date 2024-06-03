using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorkersUI : WindowUI
{
    [SerializeField] private Transform workersListParent;
    [SerializeField] private GameObject workersListElementPrefab;
    [SerializeField] private Button hireButton;

    private List<GameObject> workersList = new List<GameObject>();
    private List<string> workersActions;
    internal override void Awake()
    {
        base.Awake();
        hireButton.onClick.AddListener(OnHireButtonClicked);

        workersActions = new List<string>(); // { "Select action" };
        foreach(int value in Enum.GetValues(typeof(WorkerTask))) {
            workersActions.Add(((WorkerTask)value).ToString());
        }
    }


    public override void OpenUI()
    {
        base.OpenUI();
        UpdateList();
    }

    void UpdateList()
    {
        DestroyChildren(workersListParent);
        workersList.Clear();

        for (int i = 0; i < WorkersManager.instance.workers.Count; i++) {
            GameObject buttonGO = Instantiate(workersListElementPrefab, workersListParent);
            buttonGO.SetActive(true);
            TMP_Text nameText = buttonGO.transform.GetChild(0).GetComponent<TMP_Text>();
            TMP_Dropdown dropdown = buttonGO.transform.GetChild(1).GetComponent<TMP_Dropdown>();
            TMP_Text wageText = buttonGO.transform.GetChild(2).GetComponent<TMP_Text>();
            Button button = buttonGO.transform.GetChild(3).GetComponent<Button>();

            dropdown.ClearOptions();
            dropdown.AddOptions(workersActions);

            WorkersManager.instance.GetWorkerData(i, out string name, out WorkerTask workerTask, out int wage);
            nameText.text = name;
            wageText.text = "$" + wage + " per hour";
            dropdown.value = (int)workerTask;

            int index = i;
            button.onClick.AddListener(() => { OnFireButtonClicked(index); });
            dropdown.onValueChanged.AddListener((int val) => { OnDropdownValueChanged(index, val); });
        }
    }

    void OnHireButtonClicked()
    {
        WorkersManager.instance.HireWorker();
        UpdateList();
    }

    void OnFireButtonClicked(int index)
    {
        WorkersManager.instance.FireWorker(WorkersManager.instance.workers[index]);
        UpdateList();
    }

    private void OnDropdownValueChanged(int workerIndex, int taskIndex)
    {
        WorkersManager.instance.ChangeWorkerAction(workerIndex, (WorkerTask)taskIndex);
    }
}
