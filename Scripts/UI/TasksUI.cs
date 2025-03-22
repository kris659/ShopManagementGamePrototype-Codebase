using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TasksUI : WindowUI
{
    [SerializeField] List<GameObject> taskGameObjects = new List<GameObject>();

    internal override void Awake()
    {
        base.Awake();
        for (int i = 0; i < taskGameObjects.Count; i++) {
            int index = i;
            taskGameObjects[i].transform.GetChild(1).GetChild(5).GetComponent<Button>().onClick.AddListener(() => OnRewardButton(index));
        }
    }

    public override void OpenUI()
    {
        base.OpenUI();
        UpdateUI();
    }

    public void UpdateUI()
    {
        for(int i = 0; i < taskGameObjects.Count; i++) {
            TasksManager.TaskData taskData = TasksManager.instance.GetTaskData(i);

            if (taskData.ShouldBeIgnored) {
                taskGameObjects[i].transform.GetChild(1).gameObject.SetActive(false);
                taskGameObjects[i].transform.GetChild(2).gameObject.SetActive(true);
                continue;
            }
            taskGameObjects[i].transform.GetChild(1).gameObject.SetActive(true);
            taskGameObjects[i].transform.GetChild(2).gameObject.SetActive(false);
            Transform taskElementsParent = taskGameObjects[i].transform.GetChild(1);


            TMP_Text taskDescription = taskElementsParent.GetChild(0).GetComponent<TMP_Text>();
            TMP_Text taskProgress = taskElementsParent.GetChild(1).GetComponent<TMP_Text>();
            Image taskProgressImage = taskElementsParent.GetChild(2).GetChild(0).GetComponent<Image>();
            TMP_Text taskReward = taskElementsParent.GetChild(4).GetComponent<TMP_Text>();

            taskDescription.text = taskData.description;
            taskProgress.text = taskData.progress + " / " + taskData.amount;
            taskProgressImage.fillAmount = taskData.progress / (float)taskData.amount;
            taskReward.text = "$" + taskData.reward;

            taskElementsParent.GetChild(5).gameObject.SetActive(taskData.progress == taskData.amount); // Set 'claim reward' button active
        }
    }

    private void OnRewardButton(int index)
    {
        TasksManager.instance.OnClaimRewardButton(index);
    }

    public void ShowExclamationMark()
    {
        windowsManager.GetWindowUIButton(this).transform.GetChild(1).gameObject.SetActive(true);
    }
    public void HideExclamationMark()
    {
        windowsManager.GetWindowUIButton(this).transform.GetChild(1).gameObject.SetActive(false);
    }
}
