using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[System.Serializable]
[CreateAssetMenu(menuName = "Databases/Structure Upgrade Database")]
public class StructureUpgradeDatabase : ScriptableObject
{
    [SerializeField] public List<StructureUpgradeScriptable> upgrades;

    public StructureUpgradeScriptable GetUpgradeByName(string name)
    {
        return upgrades.Find((x) => x.name == name);
    }
#if UNITY_EDITOR
    private void OnEnable()
    {
        LoadUpgradesIntoDatabase();
    }
    [ContextMenu("Load Entities Into Database")]
    public void LoadUpgradesIntoDatabase()
    {
        List<StructureUpgradeScriptable> temp = new();
        temp.Clear();
        string[] assetNames = AssetDatabase.FindAssets("", new[] { "Assets/Game Data/StructureUpgrades" });
        foreach (string SOName in assetNames)
        {
            var SOpath = AssetDatabase.GUIDToAssetPath(SOName);
            var character = AssetDatabase.LoadAssetAtPath<StructureUpgradeScriptable>(SOpath);
            if (character != null)
                temp.Add(character);
        }
        upgrades.Clear();
        foreach (var item in temp)
        {
            upgrades.Add(item);
        }
    }
#endif
}
