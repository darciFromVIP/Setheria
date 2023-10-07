using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
[CreateAssetMenu(menuName = "Quest System/Questline")]
public class QuestlineScriptable : ScriptableObject
{
    public int currentQuestIndex = 0;
    public QuestList questList = new();

    [HideInInspector] public UnityEvent<string> Quest_Complete = new();
    [HideInInspector] public UnityEvent<string> New_Quest = new();
    [HideInInspector] public UnityEvent<string> Questline_Complete = new();
    private void OnEnable()
    {
        Debug.Log("Sorting");
        questList.quests.Sort();
        currentQuestIndex = 0;
        questList.quests[currentQuestIndex].Quest_Complete.AddListener(QuestComplete);
    }
    private void QuestComplete()
    {
        Quest_Complete.Invoke(questList.quests[currentQuestIndex].name);
        questList.quests[currentQuestIndex].Quest_Complete.RemoveAllListeners();
        currentQuestIndex++;
        if (currentQuestIndex >= questList.quests.Count)
            Questline_Complete.Invoke(name);
        else
        {
            questList.quests[currentQuestIndex].Quest_Complete.AddListener(QuestComplete);
            New_Quest.Invoke(questList.quests[currentQuestIndex].name);
        }
    }
}
