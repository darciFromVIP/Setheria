using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[System.Serializable]
[CreateAssetMenu(menuName = "Databases/VFX Database")]
public class VFXDatabase : ScriptableObject
{
    [SerializeField] public List<GameObject> vfx;
    
    public GameObject GetVFXByName(string name)
    {
        return vfx.Find((x) => x.name == name);
    }
#if UNITY_EDITOR
    private void OnEnable()
    {
        hideFlags = HideFlags.HideAndDontSave;
        LoadVFXIntoDatabase();
    }
    [ContextMenu("Load VFX Into Database")]
    public void LoadVFXIntoDatabase()
    {
        List<GameObject> temp = new();
        temp.Clear();
        string[] assetNames = AssetDatabase.FindAssets("", new[] { "Assets/Graphics Prefabs/VFX/Database" });
        foreach (string SOName in assetNames)
        {
            var SOpath = AssetDatabase.GUIDToAssetPath(SOName);
            var character = AssetDatabase.LoadAssetAtPath<GameObject>(SOpath);
            if (character != null)
                temp.Add(character);
        }
        vfx.Clear();
        foreach (var item in temp)
        {
            vfx.Add(item);
        }
    }
#endif
}
