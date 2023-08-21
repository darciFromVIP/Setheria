using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
[CreateAssetMenu(menuName = "Quest System/Questline")]
public class QuestlineScriptable : ScriptableObject
{
    public int currentQuestIndex = 0;
    public List<QuestScriptable> quests = new();

    [HideInInspector] public UnityEvent<string> Quest_Complete = new();
    [HideInInspector] public UnityEvent<QuestScriptable> New_Quest = new();
    [HideInInspector] public UnityEvent<string> Questline_Complete = new();
    private void OnEnable()
    {
        currentQuestIndex = 0;
        quests[currentQuestIndex].Quest_Complete.AddListener(QuestComplete);
    }
    private void QuestComplete()
    {
        Quest_Complete.Invoke(quests[currentQuestIndex].name);
        quests[currentQuestIndex].Quest_Complete.RemoveAllListeners();
        currentQuestIndex++;
        if (currentQuestIndex >= quests.Count)
            Questline_Complete.Invoke(name);
        else
        {
            quests[currentQuestIndex].Quest_Complete.AddListener(QuestComplete);
            New_Quest.Invoke(quests[currentQuestIndex]);
        }
    }
}
