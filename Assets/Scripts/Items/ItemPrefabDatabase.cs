using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[System.Serializable]
[CreateAssetMenu(menuName = "Databases/Item Prefab Database")]
public class ItemPrefabDatabase : ScriptableObject
{
    [SerializeField] public List<Item> items;

    public Item GetItemByName(string name)
    {
        return items.Find((x) => x.name == name);
    }
#if UNITY_EDITOR
    private void OnEnable()
    {
        hideFlags = HideFlags.HideAndDontSave;
        LoadItemsIntoDatabase();
    }
    [ContextMenu("Load Items Into Database")]
    public void LoadItemsIntoDatabase()
    {
        List<Item> tempEvents = new();
        tempEvents.Clear();
        string[] assetNames = AssetDatabase.FindAssets("", new[] { "Assets/Prefabs/Items" });
        foreach (string SOName in assetNames)
        {
            var SOpath = AssetDatabase.GUIDToAssetPath(SOName);
            var character = AssetDatabase.LoadAssetAtPath<Item>(SOpath);
            tempEvents.Add(character);
        }
        items.Clear();
        foreach (var item in tempEvents)
        {
            items.Add(item);
        }
    }
#endif
}
