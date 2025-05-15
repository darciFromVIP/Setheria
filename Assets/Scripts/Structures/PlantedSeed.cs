using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlantedSeed : NetworkBehaviour, ISaveable
{
    public float timeToGrow = 3600;
    private float growTimer = 0;

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
        if (isServer)
        {
            int random = Random.Range(0, harvestableCrops.Count);
            RpcGrownUp(random);
        }
        sprout.SetActive(false);
    }
    [ClientRpc]
    private void RpcGrownUp(int random)
    {
        selectedCrop = harvestableCrops[random];
        cropsModels[random].SetActive(true);
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
            floatData3 = fertilizeTimer
        };
    }

    public void LoadState(SaveDataWorldObject state)
    {
        transform.position = new Vector3(state.positionX, state.positionY, state.positionZ );
        transform.rotation = new Quaternion(state.rotationX, state.rotationY, state.rotationZ, state.rotationW );
        growTimer = state.floatData1;
        pourWaterTimer = state.floatData2;
        fertilizeTimer = state.floatData3;
    }
}
