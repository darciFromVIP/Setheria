using FoW;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldMap : MonoBehaviour, WindowedUI
{
    public GameObject mapWindow;
    public MapCamera mapCamera;
    public RectTransform fowMap;
    public GameObject mapIconPrefab, mapCameraButtons, mapButtonsWindow;

    private SettingsManager settingsManager;
    private GameObject ownedHeroIcon;
    private void Start()
    {
        settingsManager = FindObjectOfType<SettingsManager>();
    }
    void Update()
    {
        if (Input.GetKeyDown(settingsManager.settings.map))
        {
            ToggleWindow();
        }
    }
    public void ToggleWindow()
    {
        if (IsActive())
            HideWindow();
        else
            ShowWindow();
    }
    public GameObject SpawnIconOnMap(Sprite icon, string tooltipString, Vector2 positionInFog, IconSize iconSize, bool isOwnedHero = false)
    {
        var instance = Instantiate(mapIconPrefab, fowMap);
        UpdatePositionOfIcon(instance, positionInFog);
        instance.GetComponent<Image>().sprite = icon;
        instance.GetComponent<TooltipTrigger>().SetText(tooltipString, "");
        switch (iconSize)
        {
            case IconSize.Small:
                instance.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
                break;
            case IconSize.Medium:
                break;
            case IconSize.Large:
                instance.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
                break;
            default:
                break;
        }
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

    public void ShowWindow()
    {
        mapCamera.gameObject.SetActive(true);
        mapCameraButtons.gameObject.SetActive(true);
        mapButtonsWindow.gameObject.SetActive(true);
        mapWindow.SetActive(true);
        FindObjectOfType<Tooltip>(true).Hide();
        FindObjectOfType<TooltipWorld>(true).Hide();
    }

    public void HideWindow()
    {
        mapCamera.gameObject.SetActive(false);
        mapCameraButtons.gameObject.SetActive(false);
        mapButtonsWindow.gameObject.SetActive(false);
        mapWindow.SetActive(false);
        FindObjectOfType<Tooltip>(true).Hide();
        FindObjectOfType<TooltipWorld>(true).Hide();
    }

    public bool IsActive()
    {
        return mapWindow.activeSelf;
    }
}
