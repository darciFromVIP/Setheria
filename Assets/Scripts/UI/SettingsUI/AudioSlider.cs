using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public enum AudioSliderType
{
    None, MasterVolume, MusicVolume, AmbienceVolume, SFXVolume
}
public class AudioSlider : MonoBehaviour
{
    public AudioSliderType audioSliderType;
    [SerializeField] private Slider slider;
    private SettingsManager settingsManager;

    private void Start()
    {
        slider.onValueChanged.AddListener(ChangeVolume);
        settingsManager = FindObjectOfType<SettingsManager>(true);
    }
    public void UpdateSlider(float value)
    {
        slider.value = value;
    }
    private void ChangeVolume(float value)
    {
        switch (audioSliderType)
        {
            case AudioSliderType.None:
                break;
            case AudioSliderType.MasterVolume:
                settingsManager.ChangeMasterVolume(value);
                break;
            case AudioSliderType.MusicVolume:
                settingsManager.ChangeMusicVolume(value);
                break;
            case AudioSliderType.AmbienceVolume:
                settingsManager.ChangeAmbienceVolume(value);
                break;
            case AudioSliderType.SFXVolume:
                settingsManager.ChangeSFXVolume(value);
                break;
            default:
                break;
        }
    }
}
