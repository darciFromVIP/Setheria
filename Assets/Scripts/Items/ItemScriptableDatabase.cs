using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[System.Serializable]
[CreateAssetMenu(menuName = "Databases/Item Scriptable Database")]
public class ItemScriptableDatabase : ScriptableObject
{
    [SerializeField] public List<ItemScriptable> items;

    public ItemScriptable GetItemByName(string name)
    {
        return items.Find((x) => x.name == name);
    }
#if UNITY_EDITOR
    private void OnEnable()
    {
        LoadItemsIntoDatabase();
    }
    [ContextMenu("Load Items Into Database")]
    public void LoadItemsIntoDatabase()
    {
        List<ItemScriptable> tempItems = new();
        tempItems.Clear();
        string[] assetNames = AssetDatabase.FindAssets("", new[] { "Assets/Game Data/Items" });
        foreach (string SOName in assetNames)
        {
            var SOpath = AssetDatabase.GUIDToAssetPath(SOName);
            var character = AssetDatabase.LoadAssetAtPath<ItemScriptable>(SOpath);
            tempItems.Add(character);
        }
        items.Clear();
        foreach (var item in tempItems)
        {
            item.unlocked = item.defaultUnlocked;
            items.Add(item);
        }
    }
#endif
}
