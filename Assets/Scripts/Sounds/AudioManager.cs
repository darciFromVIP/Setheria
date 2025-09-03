using FMOD;
using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public bool inCombat = false;
    private float timer = 0;
    private float notificationTimer = 0;
    private List<HasHealth> targetsReceived = new();
    private HasHealth targetFound;
    private float combatTimer = 0;

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
        foreach (var item in targetsReceived)
        {
            if (item == null)
            {
                ReceivedTargetLost(item);
                break;
            }
            if (item.GetComponent<CanAttack>().enemyTarget == null || item.GetComponent<HasHealth>().GetHealth() <= 0)
            {
                ReceivedTargetLost(item);
                break;
            }
        }
        if (notificationTimer > 0)
            notificationTimer -= Time.deltaTime;
        if (!inCombat)
        {
            if (timer > 0)
            {
                currentMusicInstance.setVolume(Mathf.Lerp(1, 0, timer / 4));
                currentCombatInstance.setVolume(Mathf.Lerp(0, 1, timer / 4));
                timer -= Time.deltaTime;
            }
        }
        else
        {
            if (timer > 0)
            {
                currentMusicInstance.setVolume(Mathf.Lerp(0, 1, timer / 4));
                currentCombatInstance.setVolume(Mathf.Lerp(1, 0, timer / 4));
                timer -= Time.deltaTime;
            }
            if (combatTimer > 0)
            {
                combatTimer -= Time.deltaTime;
            }
            else
                CheckCombat();
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
    public void AnnounceDayNight(AmbienceParameter parameter)
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
    }
    public void ChangeAmbienceParameter(AmbienceParameter parameter)
    {
        if (!currentAmbienceInstance.isValid())
            StartCoroutine(DelayedChangeAmbienceParameter(parameter));
    }
    private IEnumerator DelayedChangeAmbienceParameter(AmbienceParameter parameter)
    {
        while (!currentAmbienceInstance.isValid())
            yield return null;
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
        if (inCombat)
            return;
        inCombat = true;
        FindObjectOfType<CharacterSkillsWindow>().EnableCombat();
        if (!currentCombatInstance.isValid())
        {
            currentCombatInstance = RuntimeManager.CreateInstance(combatEvent);
            currentCombatInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            currentCombatInstance.start();
            currentCombatInstance.setPaused(false);
        }
        timer = 4;
    }
    public void StopCombatMusic()
    {
        if (!inCombat)
            return;
        inCombat = false;
        FindObjectOfType<CharacterSkillsWindow>().DisableCombat();
        timer = 4;
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
    public void BaseUnderAttack(Vector3 pos)
    {
        if (notificationTimer > 0)
            return;
        notificationTimer = 10;
        FindObjectOfType<AudioManager>().PlayOneShot(fmodEventsDatabase.BaseUnderAttack, Vector3.zero);
        FindObjectOfType<SystemMessages>().AddMessageWithTeleportBTN("Your base is under attack!", pos, MsgType.Error);
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
    public void TargetFound(HasHealth target)
    {
        targetFound = target;
        targetFound.On_Death.AddListener(TargetDead);
        PlayCombatMusic();
    }
    public void TargetDead()
    {
        targetFound = null;
        CheckCombat();
    }
    public void TargetLost()
    {
        if (targetFound)
            targetFound.On_Death.RemoveListener(TargetDead);
        targetFound = null;
        UnityEngine.Debug.Log("Target lost: " + targetFound);
        combatTimer = 5;
    }
    public void TargetReceived(HasHealth target)
    {
        if (!targetsReceived.Contains(target))
            targetsReceived.Add(target);
        PlayCombatMusic();
    }
    public void ReceivedTargetLost(HasHealth target)
    {
        if (targetsReceived.Contains(target))
            targetsReceived.Remove(target);
        CheckCombat();
    }
    public void CheckCombat()
    {
        UnityEngine.Debug.Log("Targets received: " + targetsReceived.Count + "\nTarget Found: " + targetFound);
        if (targetsReceived.Count == 0 && targetFound == null)
            StopCombatMusic();
        else
            combatTimer = 5;
    }
}
