using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct HarvestItem
{
    public ItemScriptable harvestedItem;
    public int minimumHarvestAmount, maximumHarvestAmount;
}
[System.Serializable]
public struct PlantedPlant
{
    public int timeToGrow;
    public int seedCost;
    public List<HarvestItem> harvestItems;
    public ItemScriptable seedItem;
}
public class Planter : NetworkBehaviour, ISaveable
{
    private float timeToGrow;
    private float growTimer = 0;

    public float pourWaterCooldown;
    public float fertilizeCooldown;
    private float pourWaterTimer;
    private float fertilizeTimer;
    private int fertilized = 0;
    public bool grown = false;
    public bool planted = false;

    [Tooltip("How much does watering boost the growth? Enter a number between 0 and 100 to indicate a percentage of the current timer, which will be subtracted from the timer.")]
    public float waterBoostPercentage;

    public List<PlantedPlant> harvestableCrops;
    public List<GameObject> cropsModels = new List<GameObject>();
    private int selectedCropIndex;

    public GameObject sprout;
    public TextMeshProUGUI fertilizedText;
    public Slider slider;

    private void Update()
    {
        if (grown || !planted)
            return;

        if (growTimer < timeToGrow)
            growTimer += Time.deltaTime;
        else
            GrownUp();

        slider.value = growTimer;
        if (pourWaterTimer > 0)
            pourWaterTimer -= Time.deltaTime;
        if (fertilizeTimer > 0) 
            fertilizeTimer -= Time.deltaTime;
    }
    public void PlantSeed(int plantIndex)
    {
        var seeds = FindObjectOfType<InventoryManager>().GetItemOfName(harvestableCrops[plantIndex].seedItem.name);
        if (seeds != null)
        {
            if (seeds.stacks < harvestableCrops[plantIndex].seedCost)
            {
                FindObjectOfType<SystemMessages>().AddMessage("You don't have enough seeds for this plant.");
                return;
            }
        }
        else
        {
            FindObjectOfType<SystemMessages>().AddMessage("You don't have enough seeds for this plant.");
            return;
        }
        seeds.ChangeStacks(-harvestableCrops[plantIndex].seedCost);
        CmdPlantSeed(plantIndex);
    }
    [Command(requiresAuthority = false)]
    public void CmdPlantSeed(int plantIndex)
    {
        RpcPlantSeed(plantIndex);    
    }
    [ClientRpc]
    public void RpcPlantSeed(int plantIndex)
    {
        selectedCropIndex = plantIndex;
        timeToGrow = harvestableCrops[plantIndex].timeToGrow;
        slider.gameObject.SetActive(true);
        slider.maxValue = timeToGrow;
        planted = true;
    }

    [Command(requiresAuthority = false)]
    public void CmdPourWater()
    {
        RpcPourWater();
    }
    [ClientRpc]
    private void RpcPourWater()
    {
        pourWaterTimer = pourWaterCooldown;
        growTimer += (timeToGrow - growTimer) * (waterBoostPercentage / 100);
    }
    [Command(requiresAuthority = false)]
    public void CmdFertilize()
    {
        RpcFertilize();
    }
    [ClientRpc]
    private void RpcFertilize()
    {
        fertilizeTimer = fertilizeCooldown;
        fertilized++;
        fertilizedText.transform.parent.gameObject.SetActive(true);
        fertilizedText.text = fertilized.ToString();
    }
    public float GetGrowTimer()
    {
        return growTimer;
    }
    public float GetWaterTimer()
    {
        return pourWaterTimer;
    }
    public float GetFertilizeTimer()
    {
        return fertilizeTimer;
    }
    private void GrownUp()
    {
        slider.gameObject.SetActive(false);
        grown = true;
        if (isServer)
        {
            RpcGrownUp(selectedCropIndex);
        }
        sprout.SetActive(false);
    }
    [ClientRpc]
    private void RpcGrownUp(int cropIndex)
    {
        cropsModels[cropIndex].SetActive(true);
    }
    public void Harvest()
    {
        var player = FindObjectOfType<GameManager>().localPlayerCharacter;
        var talentLevel = player.talentTrees.IsTalentUnlocked("Improved Planter", 1);
        if (talentLevel >= 1)
        {
            int random = Random.Range(0, 100);
            if (random <= talentLevel * 50)
            {
                var inventory = FindObjectOfType<InventoryManager>(true);
                inventory.AddItem(inventory.itemDatabase.GetItemByName("Seed"), 1);
            }
        }
        foreach (var item in harvestableCrops[selectedCropIndex].harvestItems)
        {
            int random = Random.Range(item.minimumHarvestAmount, item.maximumHarvestAmount + 1) + fertilized;
            FindObjectOfType<InventoryManager>().AddItem(new ItemRecipeInfo { itemData = item.harvestedItem, stacks = random });
        }
        player.professions.AddGathering(1);
        CmdHarvest();
    }

    [Command(requiresAuthority = false)]
    public void CmdHarvest()
    {
        RpcHarvest();
    }
    [ClientRpc]
    public void RpcHarvest()
    {
        cropsModels[selectedCropIndex].SetActive(false);
        sprout.SetActive(true);
        grown = false;
        planted = false;
        growTimer = 0;
        fertilizeTimer = 0;
        pourWaterTimer = 0;
        fertilizedText.transform.parent.gameObject.SetActive(false);
    }

    public SaveDataWorldObject SaveState()
    {
        return new SaveDataWorldObject { 
            positionX = transform.position.x, 
            positionY = transform.position.y, 
            positionZ = transform.position.z, 
            rotationW = transform.rotation.w, 
            rotationX = transform.rotation.x, 
            rotationY = transform.rotation.y, 
            rotationZ = transform.rotation.z,
            floatData1 = growTimer,
            floatData2 = pourWaterTimer,
            floatData3 = fertilizeTimer,
            intData1 = selectedCropIndex
        };
    }

    public void LoadState(SaveDataWorldObject state)
    {
        transform.position = new Vector3(state.positionX, state.positionY, state.positionZ );
        transform.rotation = new Quaternion(state.rotationX, state.rotationY, state.rotationZ, state.rotationW );
        PlantSeed(state.intData1);
        growTimer = state.floatData1;
        pourWaterTimer = state.floatData2;
        fertilizeTimer = state.floatData3;
    }
}
