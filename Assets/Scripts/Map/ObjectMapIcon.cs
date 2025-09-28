using FoW;
using JetBrains.Annotations;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public enum IconSize
{
    Medium, Small, Large
}
[RequireComponent(typeof(NetworkIdentity))]
public class ObjectMapIcon : NetworkBehaviour
{
    public Sprite mapIcon;
    [Tooltip("Fill this only for player characters!")]
    public Sprite ownedMapIcon;
    public string mapTooltipText;
    public IconSize iconSize;
    public bool staticObject = true;
    public TalentTreeType professionType;

    public bool fishingUnlocked, gatheringUnlocked, explorationUnlocked;

    private FogOfWarTeam fow;
    private GameObject iconInstance;
    private WorldMap map;

    public UnityEvent Icon_Loaded = new();
    private void Start()
    {
        StartCoroutine(WaitForFow());
        if (TryGetComponent(out HideInFog fog))
            fog.Visibility_Changed.AddListener(ToggleIconOnMap);
    }
    private void Update()
    {
        if (fow == null || iconInstance == null)
            return;
        if (!staticObject)
            map.UpdatePositionOfIcon(iconInstance, fow.WorldPositionToFogPosition(transform.position));
    }
    private IEnumerator WaitForFow()
    {
        while (fow == null)
        {
            fow = FogOfWarTeam.GetTeam(0);
            yield return null;
        }
        var pos = fow.WorldPositionToFogPosition(transform.position);
        map = FindObjectOfType<WorldMap>(true);
        bool isOwnedHero = false;
        if (TryGetComponent(out PlayerCharacter character))
            if (character.isOwned)
                isOwnedHero = true;
        
        iconInstance = map.SpawnIconOnMap(isOwnedHero ? ownedMapIcon : mapIcon, mapTooltipText, pos, iconSize, isOwnedHero);

        if (TryGetComponent(out HideInFog fog))
        {
            if (fow.GetFogValue(transform.position) < fog.minFogStrength * 255 && CanShowIcon())
                ToggleIconOnMap(true);
            else
                ToggleIconOnMap(false);
        }
        Icon_Loaded.Invoke();
    }
    [Command(requiresAuthority = false)]
    public void CmdToggleIconOnMap(bool value)
    {
        RpcToggleIconOnMap(value);
    }
    [ClientRpc]
    public void RpcToggleIconOnMap(bool value)
    {
        ToggleIconOnMap(value);
    }
    public void ToggleIconOnMap(bool value)
    {
        if (iconInstance != null)
        {
            if (value && !CanShowIcon())
                return;
            iconInstance.SetActive(value);
            if (iconInstance.activeSelf)
                if (TryGetComponent(out Heartstone heartstone))
                {
                    heartstone.ActivateRespawn();
                }
        }
    }
    [Command(requiresAuthority = false)]
    public void CmdDestroyIcon()
    {
        RpcDestroyIcon();
    }
    [ClientRpc]
    public void RpcDestroyIcon()
    {
        Destroy(iconInstance);
    }
    [Command(requiresAuthority = false)]
    public void CmdToggleCheckmark()
    {
        RpcToggleCheckmark();
    }
    [ClientRpc]
    public void RpcToggleCheckmark()
    {
        StartCoroutine(WaitForInstance());
    }
    private IEnumerator WaitForInstance()
    {
        while (iconInstance == null)
            yield return null;
        iconInstance.GetComponent<MapIconPrefab>().ToggleCheckmark();
    }
    private bool CanShowIcon()
    {
        switch (professionType)
        {
            case TalentTreeType.Gathering:
                return gatheringUnlocked;
            case TalentTreeType.Fishing:
                return fishingUnlocked;
            case TalentTreeType.Exploration:
                return explorationUnlocked;
            default:
                break;
        }
        return true;
    }
    public void UnlockProfession(TalentTreeType professionType)
    {
        switch (professionType)
        {
            case TalentTreeType.Gathering:
                gatheringUnlocked = true;
                break;
            case TalentTreeType.Fishing:
                fishingUnlocked = true;
                break;
            case TalentTreeType.Exploration:
                explorationUnlocked = true;
                break;
            default:
                break;
        }
        if (TryGetComponent(out HideInFog fog))
            if (fog.IsVisible())
                ToggleIconOnMap(true);
    }
    public void LockProfession(TalentTreeType professionType)
    {
        switch (professionType)
        {
            case TalentTreeType.Gathering:
                gatheringUnlocked = false;
                break;
            case TalentTreeType.Fishing:
                fishingUnlocked = false;
                break;
            case TalentTreeType.Exploration:
                explorationUnlocked = false;
                break;
            default:
                break;
        }
        ToggleIconOnMap(false);
    }
}
