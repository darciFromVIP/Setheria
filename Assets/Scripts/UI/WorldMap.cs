using FoW;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldMap : MonoBehaviour
{
    public GameObject mapWindow;
    public MapCamera mapCamera;
    public RectTransform fowMap;
    public GameObject mapIconPrefab;

    private GameObject ownedHeroIcon;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            mapCamera.gameObject.SetActive(!mapCamera.gameObject.activeSelf);
            mapWindow.SetActive(!mapWindow.activeSelf);
            FindObjectOfType<Tooltip>(true).Hide();
            FindObjectOfType<TooltipWorld>(true).Hide();
        }

    }
    public GameObject SpawnIconOnMap(Sprite icon, string tooltipString, Vector2 positionInFog, bool isOwnedHero = false)
    {
        var instance = Instantiate(mapIconPrefab, fowMap);
        UpdatePositionOfIcon(instance, positionInFog);
        instance.GetComponent<Image>().sprite = icon;
        instance.GetComponent<TooltipTrigger>().SetText(tooltipString, "");
        if (isOwnedHero)
            ownedHeroIcon = instance;
        return instance;
    }
    public void UpdatePositionOfIcon(GameObject icon, Vector2 fogPosition)
    {
        FogOfWarTeam fow = FogOfWarTeam.GetTeam(0);
        var xPercent = fogPosition.x / fow.mapResolution.x;
        var yPercent = fogPosition.y / fow.mapResolution.y;
        var xPos = fowMap.rect.width * xPercent;
        var yPos = fowMap.rect.height * yPercent;
        icon.transform.SetLocalPositionAndRotation(new Vector2(xPos, yPos), Quaternion.identity);
    }
}
