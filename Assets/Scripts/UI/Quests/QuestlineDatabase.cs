using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[CreateAssetMenu(menuName = "Databases/Questline Database")]
public class QuestlineDatabase : ScriptableObject
{
    public List<QuestlineScriptable> questlines;

    public QuestlineScriptable GetQuestlineByName(string name)
    {
        return questlines.Find((x) => x.name == name);
    }
#if UNITY_EDITOR
    private void OnEnable()
    {
        LoadQuestlinesIntoDatabase();
    }
    [ContextMenu("Load Questlines Into Database")]
    public void LoadQuestlinesIntoDatabase()
    {
        List<QuestlineScriptable> temp = new();
        temp.Clear();
        string[] assetNames = AssetDatabase.FindAssets("", new[] { "Assets/Game Data/Quests/Questlines" });
        foreach (string SOName in assetNames)
        {
            var SOpath = AssetDatabase.GUIDToAssetPath(SOName);
            var character = AssetDatabase.LoadAssetAtPath<QuestlineScriptable>(SOpath);
            if (character != null)
                temp.Add(character);
        }
        questlines.Clear();
        foreach (var item in temp)
        {
            questlines.Add(item);
        }
    }
#endif
}
