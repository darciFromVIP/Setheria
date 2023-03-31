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
    [SerializeField] private TextMeshProUGUI seedText;
    private string filePath;
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(PlayThisWorld);
    }
    public void UpdateElement(string worldName, int worldSeed, string path)
    {
        nameText.text = worldName;
        seedText.text = "Seed: " + worldSeed.ToString();
        filePath = path;
    }
    public void PlayThisWorld()
    {
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
