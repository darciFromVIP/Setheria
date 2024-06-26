using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StructureOptionUI : MonoBehaviour
{
    private StructureOption structureOption;
    private Structure currentStructure;
    public Slider cooldownSlider;

    private List<InventoryItem> selectedItems = new();
    public void Initialize(Sprite icon, StructureOption structureOption)
    {
        GetComponent<Button>().onClick.AddListener(OnClickEvent);
        GetComponent<Image>().sprite = icon;
        currentStructure = GetComponentInParent<StructureScreen>().currentStructure;
        if (currentStructure.TryGetComponent(out Tent tent))
            tent.Stopped_Resting.AddListener(TentStoppedResting);
        string description = structureOption.description;
        if (currentStructure.demolishCost > 0 && structureOption.structureAction == StructureAction.Demolish)
            description += "\n\nDemolish Cost: " + currentStructure.demolishCost + "<sprite=15>";
        GetComponent<TooltipTrigger>().SetText(structureOption.name, description, icon);
        if (structureOption.structureAction == StructureAction.None)
            GetComponent<TooltipTrigger>().enabled = false;
        this.structureOption = structureOption;

        var structures = FindObjectsOfType<Structure>(true);
        bool unlocked = true;
        for (int i = 0; i < structureOption.requiredStructures.Count; i++)
        {
            unlocked = false;
            foreach (var structure in structures)
            {
                if (structureOption.requiredStructures[i] == structure.structureData)
                {
                    unlocked = true;
                }
            }
            if (!unlocked)
                break;
        }
        GetComponent<Button>().interactable = unlocked;

        if (structureOption.requiredResources > FindObjectOfType<GameManager>().GetResources())
            GetComponent<Button>().interactable = false;
        if (FindObjectOfType<GameManager>().localPlayerCharacter.professions.GetProfessionExperience(structureOption.professionRequired) < structureOption.professionLevelRequired)
            GetComponent<Button>().interactable = false;
        if (!FindObjectOfType<InventoryManager>(true).GetItemOfName("Everstone") && structureOption.structureAction == StructureAction.SetReturnPoint)
            GetComponent<Button>().interactable = false;
        if (structureOption.structureAction == StructureAction.CallShips)
        {
            if (!FindObjectOfType<Ship>())
                GetComponent<Button>().interactable = false;
        }
        if (currentStructure.TryGetComponent(out PlantedSeed seed))
        {
            if (!FindObjectOfType<InventoryManager>(true).GetItemOfName("Water") && structureOption.structureAction == StructureAction.PourWater)
                GetComponent<Button>().interactable = false;
            if (!FindObjectOfType<InventoryManager>(true).GetItemOfName("Fertilizer") && structureOption.structureAction == StructureAction.Fertilize)
                GetComponent<Button>().interactable = false;

            if (structureOption.structureAction == StructureAction.PourWater || structureOption.structureAction == StructureAction.Fertilize)
                if (seed.GetGrowTimer() >= seed.timeToGrow)
                    GetComponent<Button>().interactable = false;
            if (structureOption.structureAction == StructureAction.Harvest)
                if (seed.GetGrowTimer() < seed.timeToGrow)
                    GetComponent<Button>().interactable = false;
        }
    }
    private void Update()
    {
        if (currentStructure)
        {
            if (currentStructure.TryGetComponent(out Well well) && structureOption.structureAction == StructureAction.DrawWater)
            {
                if (well.GetWaterTimer() > 0)
                {
                    GetComponent<Button>().interactable = false;
                    cooldownSlider.gameObject.SetActive(true);
                    cooldownSlider.maxValue = well.waterCooldown;
                    cooldownSlider.value = well.GetWaterTimer();
                }
                else
                    GetComponent<Button>().interactable = true;
            }
            else if (currentStructure.TryGetComponent(out Tent tent) && structureOption.structureAction == StructureAction.Rest)
            {
                if (tent.GetRestCooldown() > 0)
                {
                    GetComponent<Button>().interactable = false;
                    cooldownSlider.gameObject.SetActive(true);
                    cooldownSlider.maxValue = tent.restCooldown;
                    cooldownSlider.value = tent.GetRestCooldown();
                }
                else
                    GetComponent<Button>().interactable = true;
            }
            else if (currentStructure.TryGetComponent(out Shipyard shipyard) && structureOption.structureAction == StructureAction.CallShips)
            {
                if (shipyard.callShipsTimer > 0)
                {
                    GetComponent<Button>().interactable = false;
                    cooldownSlider.gameObject.SetActive(true);
                    cooldownSlider.maxValue = shipyard.callShipsCooldown;
                    cooldownSlider.value = shipyard.callShipsTimer;
                }
                else
                    GetComponent<Button>().interactable = true;
            }
            else if (currentStructure.TryGetComponent(out PlantedSeed seed))
            {
                if (structureOption.structureAction == StructureAction.PourWater)
                {
                    if (seed.GetWaterTimer() > 0)
                    {
                        GetComponent<Button>().interactable = false;
                        cooldownSlider.gameObject.SetActive(true);
                        cooldownSlider.maxValue = seed.pourWaterCooldown;
                        cooldownSlider.value = seed.GetWaterTimer();
                    }
                    else if (FindObjectOfType<InventoryManager>(true).GetItemOfName("Water"))
                        GetComponent<Button>().interactable = true;
                }
                if (structureOption.structureAction == StructureAction.Fertilize)
                {
                    if (seed.GetFertilizeTimer() > 0)
                    {
                        GetComponent<Button>().interactable = false;
                        cooldownSlider.gameObject.SetActive(true);
                        cooldownSlider.maxValue = seed.fertilizeCooldown;
                        cooldownSlider.value = seed.GetFertilizeTimer();
                    }
                    else if (FindObjectOfType<InventoryManager>(true).GetItemOfName("Fertilizer"))
                        GetComponent<Button>().interactable = true;
                }
            }
            else
                cooldownSlider.gameObject.SetActive(false);
        }
        else
            cooldownSlider.gameObject.SetActive(false);
    }
    public void OnClickEvent()
    {
        var player = FindObjectOfType<GameManager>().localPlayerCharacter.GetComponent<PlayerController>();
        var gameManager = FindObjectOfType<GameManager>();
        switch (structureOption.structureAction)
        {
            case StructureAction.None:
                break;
            case StructureAction.Craft:
                FindObjectOfType<ManualScreen>().ShowStructureRecipes(structureOption.craftingRecipes);
                break;
            case StructureAction.Shop:
                if (structureOption.soldItems.Count > 0)
                    FindObjectOfType<ShopScreen>().ShowScreen(structureOption.soldItems);
                if (structureOption.soldUpgrades.Count > 0)
                    FindObjectOfType<ShopScreen>().ShowScreen(structureOption.soldUpgrades);
                break;
            case StructureAction.Upgrade:
                if (currentStructure.TryGetComponent(out Tent tent2))
                    if (tent2.restingPlayers.Count > 0)
                    {
                        FindObjectOfType<SystemMessages>().AddMessage("You can't upgrade this structure while someone is resting here.");
                        return;
                    }
                FindObjectOfType<StructureScreen>().HideWindow();
                currentStructure.CmdUpgradeStructure();
                FindObjectOfType<GameManager>().ChangeResources(-structureOption.requiredResources);
                break;
            case StructureAction.Demolish:
                if (gameManager.TestSubtractResources(currentStructure.demolishCost))
                {
                    gameManager.ChangeResources(-currentStructure.demolishCost);
                    FindObjectOfType<InventoryManager>(true).AddItem(currentStructure.structureItem, 1);
                    currentStructure.CmdDemolishStructure();
                    GetComponentInParent<StructureScreen>().HideWindow();
                }
                else
                    FindObjectOfType<SystemMessages>().AddMessage("You don't have enough Resources.");
                break;
            case StructureAction.TurnInResourcesAndKnowledge:
                var items = FindObjectOfType<InventoryManager>(true).GetAllItems();
                selectedItems.Clear();
                foreach (var item in items)
                {
                    if (item.item.itemType == ItemType.Resources || item.item.itemType == ItemType.Knowledge)
                    {
                        selectedItems.Add(item);
                    }
                }
                player.CmdStartWorking(selectedItems.Count * 2);
                player.Work_Finished.RemoveListener(TurnInItems);
                player.Work_Finished.AddListener(TurnInItems);
                break;
            case StructureAction.Research:
                FindObjectOfType<ResearchScreen>().ToggleWindow(true);
                break;
            case StructureAction.CookFish:
                var items1 = FindObjectOfType<InventoryManager>(true).GetAllItems();
                selectedItems.Clear();
                foreach (var item in items1)
                {
                    if (item.item.itemType == ItemType.Fish)
                    {
                        selectedItems.Add(item);
                    }
                }
                int count = 0;
                foreach (var item in selectedItems)
                {
                    count += item.stacks;
                }
                player.CmdStartWorking(count);
                player.Work_Finished.RemoveListener(CookFish);
                player.Work_Finished.AddListener(CookFish);
                break;
            case StructureAction.OpenInventory:
                FindObjectOfType<StashInventory>(true).ShowWindow();
                break;
            case StructureAction.SetReturnPoint:
                player.CmdStartWorking(3);
                player.Work_Finished.RemoveListener(SetReturnPoint);
                player.Work_Finished.AddListener(SetReturnPoint);
                break;
            case StructureAction.DrawWater:
                player.CmdStartWorking(2);
                player.Work_Finished.RemoveListener(DrawWater);
                player.Work_Finished.AddListener(DrawWater);
                break;
            case StructureAction.Rest:
                if (player.state != PlayerState.None)
                    FindObjectOfType<SystemMessages>().AddMessage("You are busy right now.");
                else
                {
                    var tent = currentStructure.GetComponent<Tent>();
                    tent.Stopped_Resting.AddListener(TentStoppedResting);
                    tent.CmdRestPlayer(player.GetComponent<NetworkIdentity>());
                    if (player.isOwned)
                        FindObjectOfType<TentButton>().ShowBTN(currentStructure);
                }
                break;
            case StructureAction.StopRest:
                var tent1 = currentStructure.GetComponent<Tent>();
                if (player.state == PlayerState.OutOfGame)
                {
                    if (tent1.restingPlayers.Contains(player.GetComponent<PlayerCharacter>()))
                    {
                        tent1.CmdStopRestPlayer(player.GetComponent<NetworkIdentity>());
                        if (player.isOwned)
                            FindObjectOfType<TentButton>().HideBTN();
                    }
                    else
                        FindObjectOfType<SystemMessages>().AddMessage("You are not resting in this tent.");
                }
                else
                    FindObjectOfType<SystemMessages>().AddMessage("You are not resting right now.");
                break;
            case StructureAction.Repair:
                if (FindObjectOfType<GameManager>().GetResources() == 0)
                {
                    FindObjectOfType<SystemMessages>().AddMessage("You have not enough Resources.");
                    return;
                }
                var healthToRepair = currentStructure.GetComponent<HasHealth>().GetFinalMaxHealth() - currentStructure.GetComponent<HasHealth>().GetHealth();
                if (healthToRepair == 0)
                {
                    FindObjectOfType<SystemMessages>().AddMessage("This structure is fully repaired.");
                    return;
                }
                int toolLevel = 0;
                foreach (var item in FindObjectOfType<CharacterScreen>().GetEquippedGear())
                {
                    if (item.item.itemType == ItemType.HandicraftTool)
                        toolLevel = item.item.value;
                }
                if (toolLevel == 0)
                {
                    FindObjectOfType<SystemMessages>().AddMessage("You don't have a Handicraft Tool equipped.");
                    return;
                }
                currentStructure.PlayRepairSound();
                player.Work_Finished.AddListener(currentStructure.StopRepairSound);
                player.Work_Cancelled.AddListener(currentStructure.StopRepairSound);
                player.Repair_Tick.AddListener(currentStructure.CmdRepairStructure);
                float temp = Mathf.Ceil(healthToRepair / (toolLevel * 10));
                float temp2 = FindObjectOfType<GameManager>().GetResources();
                player.CmdStartWorking(temp2 < temp ? temp2 : temp);
                break;
            case StructureAction.CallShips:
                currentStructure.GetComponent<Shipyard>().CallShips();
                break;
            case StructureAction.PourWater:
                player.CmdStartWorking(1);
                player.Work_Finished.RemoveListener(PourWater);
                player.Work_Finished.AddListener(PourWater);
                break;
            case StructureAction.Fertilize:
                player.CmdStartWorking(1);
                player.Work_Finished.RemoveListener(Fertilize);
                player.Work_Finished.AddListener(Fertilize);
                break;
            case StructureAction.Harvest:
                player.CmdStartWorking(2);
                player.Work_Finished.RemoveListener(Harvest);
                player.Work_Finished.AddListener(Harvest);
                break;
            case StructureAction.Dismantle:
                FindObjectOfType<DismantleScreen>(true).ShowWindow();
                break;
            default:
                break;
        }
    }
    private void TurnInItems()
    {
        var inventory = FindObjectOfType<InventoryManager>(true);
        var gameManager = FindObjectOfType<GameManager>();
        int resources = 0;
        int knowledge = 0;
        foreach (var item in selectedItems)
        {
            if (item.item.itemType == ItemType.Resources)
            {
                gameManager.ChangeResources(item.item.value);
                resources += item.item.value;
            }
            if (item.item.itemType == ItemType.Knowledge)
            {
                gameManager.ChangeKnowledge(item.item.value);
                knowledge += item.item.value;
            }
            inventory.RemoveItem(new ItemRecipeInfo() { itemData = item.item, stacks = 1 });
        }
        gameManager.localPlayerCharacter.professions.AddExploration(selectedItems.Count);
        FindObjectOfType<AudioManager>().ResourcesCollected(GetComponentInParent<StructureScreen>().currentStructure.transform.position);
        FindObjectOfType<FloatingText>().CmdSpawnFloatingText("+ " + resources.ToString() + " <sprite=15>", GetComponentInParent<StructureScreen>().currentStructure.transform.position + Vector3.up, FloatingTextType.Resources);
        FindObjectOfType<FloatingText>().CmdSpawnFloatingText("+ " + knowledge.ToString() + " <sprite=11>", GetComponentInParent<StructureScreen>().currentStructure.transform.position + Vector3.up * 0.5f, FloatingTextType.Knowledge);

    }
    private void CookFish()
    {
        var inventory = FindObjectOfType<InventoryManager>(true);
        var player = FindObjectOfType<GameManager>().localPlayerCharacter;
        foreach (var item in selectedItems)
        {
            player.professions.AddCooking(1);
            inventory.AddItem(new SaveDataItem() { name = "Cooked Fish", stacks = item.stacks });
            inventory.RemoveItem(new ItemRecipeInfo() { itemData = item.item, stacks = item.stacks });
        }
    }
    private void TentStoppedResting()
    {
        if (currentStructure.TryGetComponent(out Tent tent))
        {
            tent.StartRestCooldown();
            tent.Stopped_Resting.RemoveAllListeners();
        }
    }
    private void SetReturnPoint()
    {
        FindObjectOfType<GameManager>().localPlayerCharacter.SetReturnPoint();
        FindObjectOfType<SystemMessages>().AddMessage("Return Point Successfully Set!", MsgType.Positive);
    }
    private void DrawWater()
    {
        currentStructure.GetComponent<Well>().CmdStartWaterCooldown();
        FindObjectOfType<InventoryManager>(true).AddItem(new ItemRecipeInfo { itemData = currentStructure.GetComponent<Well>().waterItem, stacks = currentStructure.GetComponent<Well>().waterStacks });
    }
    private void PourWater()
    {
        currentStructure.GetComponent<PlantedSeed>().CmdPourWater();
        var inventory = FindObjectOfType<InventoryManager>(true);
        inventory.RemoveItem(new ItemRecipeInfo { itemData = inventory.GetItemOfName("Water").item, stacks = 1 });
    }
    private void Fertilize()
    {
        currentStructure.GetComponent<PlantedSeed>().CmdFertilize();
        var inventory = FindObjectOfType<InventoryManager>(true);
        inventory.RemoveItem(new ItemRecipeInfo { itemData = inventory.GetItemOfName("Fertilizer").item, stacks = 1 });
    }
    private void Harvest()
    {
        currentStructure.GetComponent<PlantedSeed>().Harvest();
    }
}
