using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
[CreateAssetMenu(menuName = "Quest System/Questline")]
public class QuestlineScriptable : ScriptableObject
{
    public int currentQuestIndex = 0;
    public bool synchronized = false;
    public List<QuestScriptable> questList = new();
    public string questsPath;

    [HideInInspector] public UnityEvent<string> Quest_Complete = new();
    [HideInInspector] public UnityEvent<string> New_Quest = new();
    [HideInInspector] public UnityEvent<string> Questline_Complete = new();
    private void OnEnable()
    {
        Initialize();
    }
    public void Initialize()
    {
#if UNITY_EDITOR
        if (questsPath != "")
            LoadQuestsIntoQuestline();
#endif
        foreach (var item in questList)
        {
            item.Quest_Complete.RemoveAllListeners();
        }
        LoadQuestline(0);
    }
    public void LoadQuestline(int index)
    {
        currentQuestIndex = index;
        questList[currentQuestIndex].Quest_Complete.AddListener(QuestComplete);
    }
    private void QuestComplete()
    {
        Quest_Complete.Invoke(questList[currentQuestIndex].name);
        questList[currentQuestIndex].Quest_Complete.RemoveAllListeners();
        currentQuestIndex++;
        if (currentQuestIndex >= questList.Count)
        {
            Questline_Complete.Invoke(name);
            currentQuestIndex = 0;
        }
        else
        {
            questList[currentQuestIndex].Quest_Complete.RemoveListener(QuestComplete);
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
