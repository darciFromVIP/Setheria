using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEditor;
public class LoadSpawnablePrefabs : MonoBehaviour
{
#if UNITY_EDITOR
    private void OnEnable()
    {
        LoadIntoSpawnablePrefabs();
    }
    [ContextMenu("Load Prefabs Into Network Manager")]
    public void LoadIntoSpawnablePrefabs()
    {
        List<GameObject> tempEvents = new();
        tempEvents.Clear();
        string[] assetNames = AssetDatabase.FindAssets("", new[] { "Assets/Prefabs/Entities/Heroes", "Assets/Prefabs/Entities/Enemies/Enemy Units", "Assets/Prefabs/Entities/Projectiles",
        "Assets/Prefabs/Items", "Assets/Prefabs/Structures/Built", "Assets/Prefabs/Entities/Ships", "Assets/Prefabs/UI", "Assets/Graphics Prefabs/VFX/Database", "Assets/Prefabs/Entities/Pets" });
        foreach (string SOName in assetNames)
        {
            var SOpath = AssetDatabase.GUIDToAssetPath(SOName);
            var character = AssetDatabase.LoadAssetAtPath<GameObject>(SOpath);
            if (character.TryGetComponent(out NetworkIdentity identity))
                tempEvents.Add(character);
        }
        var networkManager = GetComponent<NetworkManager>();
        networkManager.spawnPrefabs = tempEvents;
    }
#endif
}
