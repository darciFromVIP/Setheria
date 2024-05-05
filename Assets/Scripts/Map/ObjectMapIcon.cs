using FoW;
using JetBrains.Annotations;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    private FogOfWarTeam fow;
    private GameObject iconInstance;
    private WorldMap map;
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
            if (fow.GetFogValue(transform.position) < fog.minFogStrength * 255)
                ToggleIconOnMap(true);
            else
                ToggleIconOnMap(false);
        }
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
    private void ToggleIconOnMap(bool value)
    {
        if (iconInstance != null)
        {
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
        if (iconInstance)
            iconInstance.GetComponent<MapIconPrefab>().ToggleCheckmark();
    }

}
