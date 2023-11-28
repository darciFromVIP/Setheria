using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.SceneManagement;
public enum AmbienceParameter
{
    Day, Night
}
public class AudioManager : MonoBehaviour
{
    public Bus masterBus, musicBus, ambienceBus, sfxBus;
    public FMODEventsScriptable fmodEventsDatabase;

    private EventInstance currentAmbienceInstance;

    private List<EventInstance> eventInstances = new();

    public static AudioManager instance;
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
            instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += CleanUp;
        masterBus = RuntimeManager.GetBus("bus:/");
        musicBus = RuntimeManager.GetBus("bus:/Music");
        ambienceBus = RuntimeManager.GetBus("bus:/Ambience");
        sfxBus = RuntimeManager.GetBus("bus:/SFX");
    }

    public void PlayOneShot(EventReference sound, Vector3 worldPos)
    {
        RuntimeManager.PlayOneShot(sound, worldPos);
    }
    public void PlayAmbience(EventReference ambience)
    {
        if (currentAmbienceInstance.isValid())
        {
            currentAmbienceInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
        currentAmbienceInstance = RuntimeManager.CreateInstance(ambience);
        currentAmbienceInstance.start();
    }
    public void ChangeAmbienceParameter(AmbienceParameter parameter)
    {
        switch (parameter)
        {
            case AmbienceParameter.Day:
                PlayOneShot(fmodEventsDatabase.DayStart, default);
                break;
            case AmbienceParameter.Night:
                PlayOneShot(fmodEventsDatabase.NightStart, default);
                break;
            default:
                break;
        }
        currentAmbienceInstance.setParameterByName("DayAndNight", (float)parameter);
    }
    public void UIHover()
    {
        PlayOneShot(fmodEventsDatabase.UIHover, transform.position);
    }
    public void UIClick()
    {
        PlayOneShot(fmodEventsDatabase.UIClick, transform.position);
    }
    public void ManualOpen()
    {
        PlayOneShot(fmodEventsDatabase.ManualOpen, default);
    }
    public void ManualClose()
    {
        PlayOneShot(fmodEventsDatabase.ManualClose, default);
    }
    public void UIError()
    {
        PlayOneShot(fmodEventsDatabase.UIInvalid, default);
    }
    public void InventoryOpen()
    {
        PlayOneShot(fmodEventsDatabase.InventoryOpen, default);
    }
    public void InventoryClose()
    {
        PlayOneShot(fmodEventsDatabase.InventoryClose, default);
    }
    public void QuestAccepted()
    {
        PlayOneShot(fmodEventsDatabase.QuestAccepted, default);
    }
    public void QuestCompleted()
    {
        PlayOneShot(fmodEventsDatabase.QuestComplete, default);
    }
    public void ToolBreak()
    {
        PlayOneShot(fmodEventsDatabase.ToolBreak, default);
    }
    public void BuildingFinished(Vector3 worldPos)
    {
        PlayOneShot(fmodEventsDatabase.BuildingFinished, worldPos);
    }
    public void BuildingDestroyed(Vector3 worldPos)
    {
        PlayOneShot(fmodEventsDatabase.BuildingDestroyed, worldPos);
    }
    public void ChestOpen(Vector3 worldPos)
    {
        PlayOneShot(fmodEventsDatabase.ChestOpen, worldPos);
    }
    public void ItemPickUp(Vector3 worldPos)
    {
        PlayOneShot(fmodEventsDatabase.ItemPickUp, worldPos);
    }
    public void ResourcesCollected(Vector3 worldPos)
    {
        PlayOneShot(fmodEventsDatabase.ResourcesCollected, worldPos);
    }
    public void ItemCrafted(Vector3 worldPos)
    {
        PlayOneShot(fmodEventsDatabase.ItemCrafted, worldPos);
    }
    public EventInstance CreateEventInstance(EventReference eventReference)
    {
        EventInstance instance = RuntimeManager.CreateInstance(eventReference);
        eventInstances.Add(instance);
        return instance;
    }
    private void CleanUp(Scene arg0, LoadSceneMode arg1)
    {
        foreach (var item in eventInstances)
        {
            item.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            item.release();
        }
    }
    public void ChangeMasterVolume(float value)
    {
        masterBus.setVolume(value);
    }
    public void ChangeMusicVolume(float value)
    {
        musicBus.setVolume(value);
    }
    public void ChangeAmbienceVolume(float value)
    {
        ambienceBus.setVolume(value);
    }
    public void ChangeSFXVolume(float value)
    {
        sfxBus.setVolume(value);
    }
}
