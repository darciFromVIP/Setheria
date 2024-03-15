using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestLog : MonoBehaviour
{
    public TextMeshProUGUI label, description, objectives, rewards;
    public QuestLogElement questPrefab;
    public GameObject questLog;

    private void OnEnable()
    {
        for (int i = 0; i < questLog.transform.childCount; i++)
        {
            Destroy(questLog.transform.GetChild(i).gameObject);
        }
        var questManager = FindObjectOfType<QuestManager>(true);
        if (questManager)
        {
            foreach (var item in questManager.questlines)
            {
                var instance = Instantiate(questPrefab, questLog.transform);
                instance.Initialize(item.questList[item.currentQuestIndex], questManager.GetQuestTracking(item.questList[item.currentQuestIndex]), this);
            }
        }
        if (description.text == "")
            ShowQuestDetails(questLog.transform.GetChild(0).GetComponent<QuestLogElement>().questData);
    }
    public void ShowQuestDetails(QuestScriptable questData)
    {
        label.text = questData.label;
        description.text = questData.description;
        objectives.text = questData.GetObjectivesText();
        rewards.text = questData.GetRewardsText(false);
    }
}
