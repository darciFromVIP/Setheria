using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
[CreateAssetMenu(menuName = "Quest System/Questline")]
public class QuestlineScriptable : ScriptableObject
{
    public int currentQuestIndex = 0;
    public List<QuestScriptable> questList = new();
    public string questsPath;

    [HideInInspector] public UnityEvent<string> Quest_Complete = new();
    [HideInInspector] public UnityEvent<string> New_Quest = new();
    [HideInInspector] public UnityEvent<string> Questline_Complete = new();
    private void OnEnable()
    {
#if UNITY_EDITOR
        if (questsPath != "")
            LoadQuestsIntoQuestline();
#endif
        currentQuestIndex = 0;
        questList[currentQuestIndex].Quest_Complete.AddListener(QuestComplete);
    }
    private void QuestComplete()
    {
        Quest_Complete.Invoke(questList[currentQuestIndex].name);
        questList[currentQuestIndex].Quest_Complete.RemoveAllListeners();
        currentQuestIndex++;
        if (currentQuestIndex >= questList.Count)
            Questline_Complete.Invoke(name);
        else
        {
            questList[currentQuestIndex].Quest_Complete.AddListener(QuestComplete);
            New_Quest.Invoke(questList[currentQuestIndex].name);
        }
    }
#if UNITY_EDITOR
    [ContextMenu("Load Quests Into Questline")]
    public void LoadQuestsIntoQuestline()
    {
        List<QuestScriptable> tempEvents = new();
        tempEvents.Clear();
        string[] assetNames = AssetDatabase.FindAssets("", new[] { questsPath });
        foreach (string SOName in assetNames)
        {
            var SOpath = AssetDatabase.GUIDToAssetPath(SOName);
            var character = AssetDatabase.LoadAssetAtPath<QuestScriptable>(SOpath);
            tempEvents.Add(character);
        }
        questList.Clear();
        foreach (var item in tempEvents)
        {
            questList.Add(item);
        }
    }
#endif
}
