using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
[System.Serializable]
public class WorldList : MonoBehaviour
{
    [SerializeField] private TMP_InputField worldNameInput;

    [SerializeField] private GameObject worldElementPrefab;

    private void OnEnable()
    {
        LoadWorlds();
    }
    [ContextMenu("Load Worlds")]
    public void LoadWorlds()
    {
        foreach (var item in transform.GetComponentsInChildren<WorldElement>())
        {
            Destroy(item.gameObject);
        }
        var saveload = FindObjectOfType<SaveLoadSystem>(true);
        Directory.CreateDirectory(saveload.dataDirPath);
        var info = new DirectoryInfo(saveload.dataDirPath);
        var fileInfo = info.GetDirectories();
        foreach (var item in fileInfo)
        {
            var obj = Instantiate(worldElementPrefab, transform);
            var files = item.GetFiles("*.WorldData", SearchOption.TopDirectoryOnly);
            foreach (var item2 in files)
            {
                Debug.Log(item2);
            }
            if (files.Length > 0)
            {
                var worldData = saveload.LoadFileWorld(files[0].ToString());
                obj.GetComponent<WorldElement>().UpdateElement(worldData.worldSaveData.worldName, files[0].ToString());
            }
        }
    }
    
    public void CreateWorld()
    {
        if (worldNameInput.text == "")
            worldNameInput.text = "New World";
        var saveload = FindObjectOfType<SaveLoadSystem>(true);
        saveload.SaveFileWorld(new SaveDataWorldServer(worldNameInput.text), true);
        List<SaveDataPlayer> newPlayerData = new();
        for (int j = 0; j < System.Enum.GetValues(typeof(Hero)).Length; j++)
        {
            newPlayerData.Add(new SaveDataPlayer((Hero)j));
        }
        saveload.SaveNewPlayerFile(newPlayerData, worldNameInput.text);
        worldNameInput.text = "";
    }
}
