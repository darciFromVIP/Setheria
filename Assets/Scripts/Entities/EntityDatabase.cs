using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[System.Serializable]
[CreateAssetMenu(menuName = "Databases/Entity Database")]
public class EntityDatabase : ScriptableObject
{
    [SerializeField] public List<Entity> entities;

    public Entity GetEntityByName(string name)
    {
        return entities.Find((x) => x.name == name);
    }
#if UNITY_EDITOR
    private void OnEnable()
    {
        LoadEntitiesIntoDatabase();
    }
    [ContextMenu("Load Entities Into Database")]
    public void LoadEntitiesIntoDatabase()
    {
        List<Entity> temp = new();
        temp.Clear();
        string[] assetNames = AssetDatabase.FindAssets("", new[] { "Assets/Prefabs/Entities/Ships", "Assets/Prefabs/Entities/Pets", "Assets/Prefabs/Entities/Enemies/Enemy Units" });
        foreach (string SOName in assetNames)
        {
            var SOpath = AssetDatabase.GUIDToAssetPath(SOName);
            var character = AssetDatabase.LoadAssetAtPath<Entity>(SOpath);
            if (character != null)
                temp.Add(character);
        }
        entities.Clear();
        foreach (var item in temp)
        {
            entities.Add(item);
        }
    }
#endif
}
