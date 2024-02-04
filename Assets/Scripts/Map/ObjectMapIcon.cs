using FoW;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum IconSize
{
    Medium, Small, Large
}

public class ObjectMapIcon : MonoBehaviour
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
            ToggleIconOnMap(false);
    }

    private void ToggleIconOnMap(bool value)
    {
        if (iconInstance != null)
            iconInstance.SetActive(value);
    }
    public void DestroyIcon()
    {
        Destroy(iconInstance);
    }
    private void OnDestroy()
    {
        DestroyIcon();
    }
}
