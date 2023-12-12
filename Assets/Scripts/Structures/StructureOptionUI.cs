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
        string description = structureOption.description;
        if (currentStructure.demolishCost > 0 && structureOption.structureAction == StructureAction.Demolish)
            description += "\n\nDemolish Cost: " + currentStructure.demolishCost + "<sprite=15>";
        GetComponent<TooltipTrigger>().SetText(structureOption.name, description, icon);
        if (structureOption.structureAction == StructureAction.None)
            GetComponent<TooltipTrigger>().enabled = false;
        this.structureOption = structureOption;

        if (FindObjectOfType<GameManager>().localPlayerCharacter.professions.GetProfessionExperience(structureOption.professionRequired) < structureOption.professionLevelRequired)
            GetComponent<Button>().interactable = false;
        if (!FindObjectOfType<InventoryManager>(true).GetItemOfName("Everstone") && structureOption.structureAction == StructureAction.SetReturnPoint)
            GetComponent<Button>().interactable = false;
    }
    private void Update()
    {
        if (currentStructure)
        {
            if (currentStructure.TryGetComponent(out Well well))
            {
                cooldownSlider.gameObject.SetActive(true);
                cooldownSlider.maxValue = well.waterCooldown;
                cooldownSlider.value = well.GetWaterTimer();
            }
            else
                cooldownSlider.gameObject.SetActive(false);
        }
        else
            cooldownSlider.gameObject.SetActive(false);
    }
    public void OnClickEvent()
    {
        switch (structureOption.structureAction)
        {
            case StructureAction.None:
                break;
            case StructureAction.Craft:
                FindObjectOfType<ManualScreen>().ShowStructureRecipes(structureOption.craftingRecipes);
                break;
            case StructureAction.Shop:
                FindObjectOfType<ShopScreen>().ShowScreen(structureOption.soldItems);
                break;
            case StructureAction.Upgrade:
                break;
            case StructureAction.Demolish:
                var gameManager = FindObjectOfType<GameManager>();
                var structure = GetComponentInParent<StructureScreen>().currentStructure;
                if (gameManager.TestSubtractResources(structure.demolishCost))
                {
                    gameManager.ChangeResources(-structure.demolishCost);
                    FindObjectOfType<InventoryManager>(true).AddItem(structure.structureItem, 1);
                    structure.CmdDemolishStructure();
                    GetComponentInParent<StructureScreen>().HideWindow();
                }
                else
                    FindObjectOfType<SystemMessages>().AddMessage("You don't have enough Resources!");
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
                var player = FindObjectOfType<GameManager>().localPlayerCharacter.GetComponent<PlayerController>();
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
                var player1 = FindObjectOfType<GameManager>().localPlayerCharacter.GetComponent<PlayerController>();
                int count = 0;
                foreach (var item in selectedItems)
                {
                    count += item.stacks;
                }
                player1.CmdStartWorking(count);
                player1.Work_Finished.RemoveListener(CookFish);
                player1.Work_Finished.AddListener(CookFish);
                break;
            case StructureAction.OpenInventory:
                FindObjectOfType<StashInventory>(true).ShowWindow();
                break;
            case StructureAction.SetReturnPoint:
                var player2 = FindObjectOfType<GameManager>().localPlayerCharacter.GetComponent<PlayerController>();
                player2.CmdStartWorking(3);
                player2.Work_Finished.RemoveListener(SetReturnPoint);
                player2.Work_Finished.AddListener(SetReturnPoint);
                break;
            case StructureAction.DrawWater:
                if (currentStructure.GetComponent<Well>().GetWaterTimer() > 0)
                    FindObjectOfType<SystemMessages>().AddMessage("There is no water in the Well!");
                else
                {
                    var player3 = FindObjectOfType<GameManager>().localPlayerCharacter.GetComponent<PlayerController>();
                    player3.CmdStartWorking(2);
                    player3.Work_Finished.RemoveListener(DrawWater);
                    player3.Work_Finished.AddListener(DrawWater);
                }
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
    private void SetReturnPoint()
    {
        FindObjectOfType<GameManager>().localPlayerCharacter.SetReturnPoint();
        FindObjectOfType<SystemMessages>().AddMessage("Return Point Successfully Set!", MsgType.Positive);
    }
    private void DrawWater()
    {
        currentStructure.GetComponent<Well>().CmdStartWaterCooldown();
        FindObjectOfType<InventoryManager>(true).AddItem(new ItemRecipeInfo { itemData = currentStructure.GetComponent<Well>().waterItem, stacks = 1 });

    }
}
