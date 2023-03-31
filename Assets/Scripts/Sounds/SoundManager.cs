using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public List<AudioClip> sounds;
    private AudioSource audioSource;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        audioSource = GetComponent<AudioSource>();
    }
    public AudioClip FindSoundByName(string name)
    {
        return sounds.Find((x) => x.name == name);
    }
    public void Click()
    {
        audioSource.PlayOneShot(FindSoundByName("UI Click"));
    }
    public void UIHover()
    {
        audioSource.PlayOneShot(FindSoundByName("UI Hover"));
    }
    public void ManualOpen()
    {
        audioSource.PlayOneShot(FindSoundByName("Manual Open"));
    }
    public void PlaySound(AudioClip sound)
    {
        audioSource.PlayOneShot(sound);
    }
}
