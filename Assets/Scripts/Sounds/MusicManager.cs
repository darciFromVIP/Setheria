using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public List<AudioClip> music;
    private AudioSource audioSource;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        audioSource = GetComponent<AudioSource>();
        SceneManager.sceneLoaded += SceneLoaded;
    }
    private void SceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.name == "Forest Biome")
            Ambience();
    }
    public AudioClip FindMusicByName(string name)
    {
        return music.Find((x) => x.name == name);
    }
    public void MainMenu()
    {
        audioSource.Stop();
        audioSource.PlayOneShot(FindMusicByName("Raw Demo 3"));
    }
    public void Ambience()
    {
        audioSource.Stop();
        audioSource.PlayOneShot(FindMusicByName("Raw Demo 1"));
    }
    public void Combat()
    {
        audioSource.Stop();
        audioSource.PlayOneShot(FindMusicByName("Raw Demo 2"));
    }
    public void PlaySound(AudioClip sound)
    {
        audioSource.Stop();
        audioSource.PlayOneShot(sound);
    }
}
