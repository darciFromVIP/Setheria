using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioWindow : MonoBehaviour
{
    private List<AudioSlider> audioSliders = new();
    private SettingsManager settingsManager;

    private void Awake()
    {
        foreach (var item in GetComponentsInChildren<AudioSlider>(true))
        {
            audioSliders.Add(item);
        }
        settingsManager = FindObjectOfType<SettingsManager>(true);
    }

    private void OnEnable()
    {
        UpdateSliders();
    }
    public void UpdateSliders()
    {
        foreach (var item in audioSliders)
        {
            item.UpdateSlider(settingsManager.GetDataByAudioSlider(item.audioSliderType));
        }
    }
}
