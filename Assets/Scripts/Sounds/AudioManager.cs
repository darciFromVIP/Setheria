using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.SceneManagement;
using FMOD;

public enum AmbienceParameter
{
    Day, Night
}
public enum MusicParameter
{
    ForestDay, ForestNight, Corruption
}
public class AudioManager : MonoBehaviour
{
    public Bus masterBus, musicBus, ambienceBus, sfxBus;
    public FMODEventsScriptable fmodEventsDatabase;
    public EventReference musicEvent, combatEvent;

    private EventInstance currentAmbienceInstance;
    private EventInstance currentMusicInstance;
    private EventInstance currentCombatInstance;

    private List<EventInstance> eventInstances = new();

    private float combatMusicTimer;
    private bool musicChanging = false, startingCombatMusic = false, stoppingCombatMusic = false;
    private List<HasHealth> targetsReceived = new();

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
        SceneManager.sceneLoaded += SceneChanged;
        masterBus = RuntimeManager.GetBus("bus:/");
        musicBus = RuntimeManager.GetBus("bus:/Music");
        ambienceBus = RuntimeManager.GetBus("bus:/Ambience");
        sfxBus = RuntimeManager.GetBus("bus:/SFX");
    }
    private void Update()
    {
        if (combatMusicTimer > 0) 
            combatMusicTimer -= Time.deltaTime;
        foreach (var item in targetsReceived)
        {
            if (item == null)
            {
                targetsReceived.Remove(item);
                if (targetsReceived.Count == 0)
                    StopCombatMusic();
                break;
            }
            if (item.GetComponent<CanAttack>().enemyTarget == null)
            {
                targetsReceived.Remove(item);
                if (targetsReceived.Count == 0)
                    StopCombatMusic();
                break;
            }
        }
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
    public void PlayMusic()
    {
        currentMusicInstance = RuntimeManager.CreateInstance(musicEvent);
        currentMusicInstance.start();
    }
    public void ChangeMusicParameter(MusicParameter parameter)
    {
        if (!currentMusicInstance.isValid())
            PlayMusic();
        currentMusicInstance.setParameterByName("Music", (float)parameter);
    }
    public void PlayCombatMusic()
    {
        if (startingCombatMusic)
            return;
        StartCoroutine(PlayCombatMusicCoro());
    }
    private IEnumerator PlayCombatMusicCoro()
    {
        yield return new WaitWhile(IsMusicChanging);
        if (startingCombatMusic)
            yield break;
        if (!currentCombatInstance.isValid())
        {
            currentCombatInstance = RuntimeManager.CreateInstance(combatEvent);
            currentCombatInstance.start();
        }
        else
        {
            bool paused;
            currentCombatInstance.getPaused(out paused);
            if (!paused)
                yield break;
        }
        if (combatMusicTimer <= 0)
        {
            currentCombatInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            currentCombatInstance.start();
        }
        StartCoroutine(UnpauseCombatFadeIn());
        StartCoroutine(PauseMusicFadeOut());
    }
    public void StopCombatMusic()
    {
        if (stoppingCombatMusic)
            return;
        StartCoroutine(StopCombatMusicCoro());
    }
    private IEnumerator StopCombatMusicCoro()
    {
        yield return new WaitWhile(IsMusicChanging);
        if (stoppingCombatMusic)
            yield break;
        combatMusicTimer = 15;
        StartCoroutine(PauseCombatFadeOut());
        StartCoroutine(UnpauseMusicFadeIn());
    }
    private bool IsMusicChanging()
    {
        return musicChanging;
    }
    private void ChangeToNightMusic()
    {
        float parameter;
        currentMusicInstance.getParameterByName("Music", out parameter);
        if (parameter == 0)
            ChangeMusicParameter(MusicParameter.ForestNight);
    }
    private void ChangeToDayMusic()
    {
        float parameter;
        currentMusicInstance.getParameterByName("Music", out parameter);
        if (parameter == 1)
            ChangeMusicParameter(MusicParameter.ForestDay);
    }
    private IEnumerator PauseMusicFadeOut()
    {
        musicChanging = true;
        float timer = 4;
        while (timer > 0)
        {
            currentMusicInstance.setVolume(Mathf.Lerp(0, 1, timer / 4));
            timer -= Time.deltaTime;
            yield return null;
        }
        currentMusicInstance.setPaused(true);
        musicChanging = false;
    }
    private IEnumerator PauseCombatFadeOut()
    {
        musicChanging = true;
        stoppingCombatMusic = true;
        float timer = 4;
        while (timer > 0)
        {
            currentCombatInstance.setVolume(Mathf.Lerp(0, 1, timer / 4));
            timer -= Time.deltaTime;
            yield return null;
        }
        currentCombatInstance.setPaused(true);
        musicChanging = false;
        yield return new WaitForSeconds(5);
        stoppingCombatMusic = false;
    }
    private IEnumerator UnpauseMusicFadeIn()
    {
        musicChanging = true;
        currentMusicInstance.setPaused(false);
        float timer = 4;
        while (timer > 0)
        {
            currentMusicInstance.setVolume(Mathf.Lerp(1, 0, timer / 4));
            timer -= Time.deltaTime;
            yield return null;
        }
        musicChanging = false;
    }
    private IEnumerator UnpauseCombatFadeIn()
    {
        musicChanging = true;
        startingCombatMusic = true;
        currentCombatInstance.setPaused(false);
        float timer = 4;
        while (timer > 0)
        {
            currentCombatInstance.setVolume(Mathf.Lerp(1, 0, timer / 4));
            timer -= Time.deltaTime;
            yield return null;
        }
        musicChanging = false;
        yield return new WaitForSeconds(5);
        startingCombatMusic = false;
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
    public void RecipeUnlocked()
    {
        PlayOneShot(fmodEventsDatabase.UnlockRecipe, default);
    }
    public void EatFood(Vector3 position)
    {
        PlayOneShot(fmodEventsDatabase.EatFood, position);
    }
    public void DrinkWater(Vector3 position)
    {
        PlayOneShot(fmodEventsDatabase.DrinkWater, position);
    }
    public void PlayerDeath()
    {
        PlayOneShot(fmodEventsDatabase.PlayerDeath, default);
    }
    public EventInstance CreateEventInstance(EventReference eventReference, Transform pos)
    {
        EventInstance instance = RuntimeManager.CreateInstance(eventReference);
        eventInstances.Add(instance);
        instance.set3DAttributes(new ATTRIBUTES_3D
        {
            position = new VECTOR { x = pos.position.x, y = pos.position.y, z = pos.position.z },
            forward = new VECTOR { x = pos.forward.x, y = pos.forward.y, z = pos.forward.z },
            up = new VECTOR {x = pos.up.x, y = pos.up.y, z = pos.up.z },
            velocity = new VECTOR { x = 0, y = 0, z = 0 }
        });
        instance.start();
        return instance;
    }
    private void SceneChanged(Scene arg0, LoadSceneMode arg1)
    {
        foreach (var item in eventInstances)
        {
            item.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            item.release();
        }
        currentAmbienceInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        currentAmbienceInstance.release();
        currentMusicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        currentMusicInstance.release();
        currentCombatInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        currentCombatInstance.release();
    }
    public void SetDayNightCycle(DayNightCycle reference)
    {
        reference.Night_Started.AddListener(ChangeToNightMusic);
        reference.Day_Started.AddListener(ChangeToDayMusic);
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
    public void TargetReceived(HasHealth target)
    {
        targetsReceived.Add(target);
        PlayCombatMusic();
    }
    public void ReceivedTargetLost(HasHealth target)
    {
        if (targetsReceived.Contains(target))
            targetsReceived.Remove(target);
        if (targetsReceived.Count == 0)
            StopCombatMusic();
    }
}
