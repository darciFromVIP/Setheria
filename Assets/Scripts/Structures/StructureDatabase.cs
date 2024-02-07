using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[System.Serializable]
[CreateAssetMenu(menuName = "Databases/Structure Database")]
public class StructureDatabase : ScriptableObject
{
    [SerializeField] public List<Structure> structures;

    public Structure GetStructureByName(string name)
    {
        return structures.Find((x) => x.name == name);
    }
#if UNITY_EDITOR
    private void OnEnable()
    {
        LoadStructuresIntoDatabase();
    }
    [ContextMenu("Load Structures Into Database")]
    public void LoadStructuresIntoDatabase()
    {
        List<Structure> temp = new();
        temp.Clear();
        string[] assetNames = AssetDatabase.FindAssets("", new[] { "Assets/Prefabs/Structures/Built" });
        foreach (string SOName in assetNames)
        {
            var SOpath = AssetDatabase.GUIDToAssetPath(SOName);
            var character = AssetDatabase.LoadAssetAtPath<Structure>(SOpath);
            if (character != null)
                temp.Add(character);
        }
        structures.Clear();
        foreach (var item in temp)
        {
            structures.Add(item);
        }
    }
#endif
}
