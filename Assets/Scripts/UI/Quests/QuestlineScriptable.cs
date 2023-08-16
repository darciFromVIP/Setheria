using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
[CreateAssetMenu(menuName = "Quest System/Questline")]
public class QuestlineScriptable : ScriptableObject
{
    public int currentQuestIndex = 0;
    public List<QuestScriptable> quests = new();

    [HideInInspector] public UnityEvent<QuestScriptable> Quest_Complete = new();
    [HideInInspector] public UnityEvent<QuestScriptable> New_Quest = new();
    [HideInInspector] public UnityEvent<QuestlineScriptable> Questline_Complete = new();
    private void OnEnable()
    {
        quests[currentQuestIndex].Quest_Complete.AddListener(QuestComplete);
        currentQuestIndex = 0;
    }
    private void QuestComplete()
    {
        Quest_Complete.Invoke(quests[currentQuestIndex]);
        quests[currentQuestIndex].Quest_Complete.RemoveAllListeners();
        currentQuestIndex++;
        if (currentQuestIndex >= quests.Count)
            Questline_Complete.Invoke(this);
        else
        {
            quests[currentQuestIndex].Quest_Complete.AddListener(QuestComplete);
            New_Quest.Invoke(quests[currentQuestIndex]);
        }
    }
}
