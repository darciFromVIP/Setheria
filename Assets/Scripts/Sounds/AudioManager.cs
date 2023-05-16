using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.SceneManagement;
public class AudioManager : MonoBehaviour
{
    public FMODEventsScriptable fmodEventsDatabase;

    private List<EventInstance> eventInstances = new();
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += CleanUp;
    }

    public void PlayOneShot(EventReference sound, Vector3 worldPos)
    {
        RuntimeManager.PlayOneShot(sound, worldPos);
    }
    public void UIHover()
    {
        PlayOneShot(fmodEventsDatabase.UIHover, transform.position);
    }
    public void UIClick()
    {
        PlayOneShot(fmodEventsDatabase.UIClick, transform.position);
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
}
