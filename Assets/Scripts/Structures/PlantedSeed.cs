using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlantedSeed : NetworkBehaviour
{
    public float timeToGrow = 3600;
    private float growTimer;

    public float pourWaterCooldown;
    public float fertilizeCooldown;
    private float pourWaterTimer;
    private float fertilizeTimer;
    private int fertilized = 0;
    private bool grown = false;

    [Tooltip("How much does watering boost the growth? Enter a number between 0 and 100 to indicate a percentage of the current timer, which will be subtracted from the timer.")]
    public float waterBoostPercentage;

    public List<ItemScriptable> harvestableCrops = new List<ItemScriptable>();
    public List<GameObject> cropsModels = new List<GameObject>();
    public int minimumCount = 2;
    public int maximumCount = 3;
    private ItemScriptable selectedCrop;

    public GameObject sprout, statusBar;
    public TextMeshProUGUI fertilizedText;
    public Slider slider;

    private void Start()
    {
        growTimer = 0;
        slider.maxValue = timeToGrow;
    }
    private void Update()
    {
        if (grown)
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
        int random = Random.Range(0, harvestableCrops.Count);
        selectedCrop = harvestableCrops[random];
        cropsModels[random].SetActive(true);
        sprout.SetActive(false);
    }
    public void Harvest()
    {
        int random = Random.Range(minimumCount, maximumCount + 1) + fertilized;
        FindObjectOfType<InventoryManager>().AddItem(new ItemRecipeInfo { itemData = selectedCrop, stacks = random });
        FindObjectOfType<StructureScreen>().HideWindow();
        FindObjectOfType<GameManager>().localPlayerCharacter.professions.AddGathering(1);
        DestroySeed();
    }
    [Command(requiresAuthority = false)]
    private void DestroySeed()
    {
        NetworkServer.Destroy(gameObject);
    }
}
