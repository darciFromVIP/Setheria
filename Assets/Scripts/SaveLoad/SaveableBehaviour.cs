using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

[Serializable]
public class SaveableBehaviour : MonoBehaviour
{
    [SerializeField] private string id;

    public string Id => id;
    private void Awake()
    {
        if (id == "")
            GenerateId();
    }

    [ContextMenu("Generate Id")]
    public void GenerateId()
    {
        id = Guid.NewGuid().ToString();
    }
#if UNITY_EDITOR
    [ContextMenu("GenerateIdsInScene")]
    public void GenerateIdsInScene()
    {
        foreach (SaveableBehaviour item in FindObjectsOfType<SaveableBehaviour>(true))
        {
            EditorUtility.SetDirty(item);
            item.GenerateId();
        }
    }
#endif
    public Dictionary<string, SaveDataWorldObject> SaveState()
    {
        var state = new Dictionary<string, SaveDataWorldObject>();

        foreach (var item in GetComponents<ISaveable>())
        {
            state.Add(item.GetType().ToString(), item.SaveState());
        }
        return state;
    }
    public void LoadState(Dictionary<string, SaveDataWorldObject> state)
    {
        foreach (var item in GetComponents<ISaveable>())
        {
            string typeName = item.GetType().ToString();
            if (state.TryGetValue(typeName, out SaveDataWorldObject value))
                item.LoadState(value);
        }
    }
}
