using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[System.Serializable]
[CreateAssetMenu(menuName = "Databases/Buff Scriptable Database")]
public class BuffDatabase : ScriptableObject
{
    [SerializeField] public List<BuffScriptable> buffs;


    public BuffScriptable GetBuffByName(string name)
    {
        return buffs.Find((x) => x.name == name);
    }
#if UNITY_EDITOR
    private void OnEnable()
    {
        LoadItemsIntoDatabase();
    }
    [ContextMenu("Load Items Into Database")]
    public void LoadItemsIntoDatabase()
    {
        List<BuffScriptable> tempEvents = new();
        tempEvents.Clear();
        string[] assetNames = AssetDatabase.FindAssets("", new[] { "Assets/Game Data/Buffs/Ability Buffs", "Assets/Game Data/Buffs/Item Buffs" });
        foreach (string SOName in assetNames)
        {
            var SOpath = AssetDatabase.GUIDToAssetPath(SOName);
            var character = AssetDatabase.LoadAssetAtPath<BuffScriptable>(SOpath);
            tempEvents.Add(character);
        }
        buffs.Clear();
        foreach (var item in tempEvents)
        {
            buffs.Add(item);
        }
    }
#endif
}
