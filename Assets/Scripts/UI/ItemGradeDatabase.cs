using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CreateAssetMenu(menuName = "Databases/Item Grade Database")]
public class ItemGradeDatabase : ScriptableObject
{
    public List<Sprite> grades;

    public Sprite GetBorderByName(string name)
    {
        return grades.Find((x) => x.name == name);
    }
#if UNITY_EDITOR
    private void OnEnable()
    {
        LoadItemsIntoDatabase();
    }
    [ContextMenu("Load Items Into Database")]
    public void LoadItemsIntoDatabase()
    {
        List<Sprite> tempEvents = new();
        tempEvents.Clear();
        string[] assetNames = AssetDatabase.FindAssets("", new[] { "Assets/Graphics/UI/Icons/Borders" });
        foreach (string SOName in assetNames)
        {
            var SOpath = AssetDatabase.GUIDToAssetPath(SOName);
            var character = AssetDatabase.LoadAssetAtPath<Sprite>(SOpath);
            tempEvents.Add(character);
        }
        grades.Clear();
        foreach (var item in tempEvents)
        {
            grades.Add(item);
        }
    }
#endif
}
