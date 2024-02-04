using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
public class WorldElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    private string filePath;
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(PlayThisWorld);
    }
    public void UpdateElement(string worldName, string path)
    {
        nameText.text = worldName;
        filePath = path;
    }
    public void PlayThisWorld()
    {
        FindObjectOfType<LoadingScreen>().LoadOperation("Starting server...");
        FindObjectOfType<SaveLoadSystem>().SetCurrentWorld(filePath);
        Debug.Log("Current World Loaded");
        SteamLobby.instance.HostLobby();
    }
    public void DeleteThisWorld()
    {
        Directory.Delete(Directory.GetParent(filePath).ToString(), true);
        Destroy(gameObject);
    }
}
