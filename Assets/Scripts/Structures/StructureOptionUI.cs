using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class StructureOptionUI : MonoBehaviour
{
    private StructureOption structureOption;

    private List<InventoryItem> selectedItems = new();
    public void Initialize(Sprite icon, StructureOption structureOption)
    {
        GetComponent<Button>().onClick.AddListener(OnClickEvent);
        GetComponent<Image>().sprite = icon;
        var structure = GetComponentInParent<StructureScreen>().currentStructure;
        string description = structureOption.description;
        if (structure.demolishCost > 0 && structureOption.structureAction == StructureAction.Demolish)
            description += "\n\nDemolish Cost: " + structure.demolishCost + "<sprite=15>";
        GetComponent<TooltipTrigger>().SetText(structureOption.name, description, icon);
        if (structureOption.structureAction == StructureAction.None)
            GetComponent<TooltipTrigger>().enabled = false;
        this.structureOption = structureOption;
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
                    FindObjectOfType<InventoryManager>().AddItem(structure.structureItem, 1);
                    structure.CmdDemolishStructure();
                    GetComponentInParent<StructureScreen>().HideWindow();
                }
                else
                    FindObjectOfType<SystemMessages>().AddMessage("You don't have enough Resources!");
                break;
            case StructureAction.TurnInResourcesAndKnowledge:
                var items = FindObjectOfType<InventoryManager>().GetAllItems();
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
                player.Work_Finished.AddListener(TurnInItems);
                break;
            case StructureAction.Research:
                FindObjectOfType<ResearchScreen>().ToggleWindow(true);
                break;
            case StructureAction.CookFish:
                var items1 = FindObjectOfType<InventoryManager>().GetAllItems();
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
                player1.Work_Finished.AddListener(CookFish);
                break;
            case StructureAction.OpenInventory:
                FindObjectOfType<StashInventory>(true).ShowWindow();
                break;
            case StructureAction.SetReturnPoint:
                var player2 = FindObjectOfType<GameManager>().localPlayerCharacter.GetComponent<PlayerController>();
                player2.CmdStartWorking(3);
                player2.Work_Finished.AddListener(SetReturnPoint);
                break;
            default:
                break;
        }
    }
    private void TurnInItems()
    {
        var inventory = FindObjectOfType<InventoryManager>();
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
        FindObjectOfType<FloatingText>().SpawnFloatingText("+ " + resources.ToString() + " Resources", GetComponentInParent<StructureScreen>().currentStructure.transform.position + Vector3.up, FloatingTextType.Resources);
        FindObjectOfType<FloatingText>().SpawnFloatingText("+ " + knowledge.ToString() + " Knowledge", GetComponentInParent<StructureScreen>().currentStructure.transform.position + Vector3.up * 1.5f, FloatingTextType.Knowledge);

    }
    private void CookFish()
    {
        var inventory = FindObjectOfType<InventoryManager>();
        foreach (var item in selectedItems)
        {
            inventory.AddItem(new SaveDataItem() { name = "Cooked Fish", stacks = item.stacks });
            inventory.RemoveItem(new ItemRecipeInfo() { itemData = item.item, stacks = item.stacks });
        }
    }
    private void SetReturnPoint()
    {
        FindObjectOfType<GameManager>().localPlayerCharacter.SetReturnPoint();
        FindObjectOfType<SystemMessages>().AddMessage("Return Point Successfully Set!");
    }
}
