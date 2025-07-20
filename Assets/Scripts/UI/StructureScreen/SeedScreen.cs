using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SeedScreen : MonoBehaviour, WindowedUI
{
    public GameObject window;
    public GameObject plantsUI;
    private Planter currentPlanter;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
            ToggleWindow(false);
    }
    public void ToggleWindow(bool value)
    {
        if (value)
        {
            window.SetActive(true);
            currentPlanter = FindObjectOfType<StructureScreen>().currentStructure.GetComponent<Planter>();
            var plantButtons = plantsUI.GetComponentsInChildren<PlantButton>();
            var plantList = currentPlanter.harvestableCrops;
            
            for (int i = 0; i < plantButtons.Length; i++)
            {
                var tooltip = plantButtons[i].GetComponent<TooltipTrigger>();
                tooltip.content = "Growth time: " + plantList[i].timeToGrow;
                tooltip.content += "\nSeed type: " + plantList[i].seedItem.name;
                tooltip.content += "\n  <color=#FFD000>Seed cost: " + plantList[i].seedCost + "</color>";
                foreach (var item in plantList[i].harvestItems)
                {
                    tooltip.content += "\nHarvest item: " + item.harvestedItem.name;
                    tooltip.content += "\n  Harvest yield: " + item.minimumHarvestAmount + " - " + item.maximumHarvestAmount;
                }
                if (!plantButtons[i].plant.unlocked)
                    plantButtons[i].GetComponent<Button>().interactable = false;
                else
                    plantButtons[i].GetComponent<Button>().interactable = true;
            }
        }
        else
        {
            window.SetActive(false);
        }
    }
    public void ShowWindow()
    {
        ToggleWindow(true);
    }

    public void HideWindow()
    {
        ToggleWindow(false);
    }

    public bool IsActive()
    {
        return window.activeSelf;
    }
    public void PlantSeed(int plantIndex)
    {
        currentPlanter.PlantSeed(plantIndex);
    }
}
