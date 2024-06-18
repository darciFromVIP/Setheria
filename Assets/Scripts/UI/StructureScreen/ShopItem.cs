using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ShopItem : MonoBehaviour
{
    public InventoryItem itemIcon;
    public TextMeshProUGUI labelText, costText;
    public Button purchaseBTN;
    public StructureUpgradeScriptable upgrade;
    public void InitializeItem(ItemRecipeInfo item)
    {
        itemIcon.InitializeItem(item.itemData, item.stacks, false, false, false);
        if (item.itemData.unlocked)
        {
            labelText.text = item.itemData.name;
            costText.text = "";
            if (item.itemData.resourceCost > 0)
                costText.text += "<sprite=0>" + item.itemData.resourceCost;
            if (item.itemData.knowledgeCost > 0)
                costText.text += "<sprite=1>" + item.itemData.knowledgeCost;
            if (costText.text == "")
                costText.text = "Free";
            GetComponentInParent<ShopScreen>().CheckAvailability();
            purchaseBTN.onClick.AddListener(PurchaseItem);
        }
        else
        {
            labelText.text = "???";
            costText.text = "???";
            purchaseBTN.interactable = false;
        }
    }
    public void InitializeUpgrade(StructureUpgradeScriptable upgrade)
    {
        this.upgrade = upgrade;
        itemIcon.image.sprite = upgrade.icon;
        itemIcon.border.gameObject.SetActive(false);
        itemIcon.stackText.text = upgrade.currentLevel.ToString();
        itemIcon.stackText.gameObject.SetActive(true);
        itemIcon.GetComponent<TooltipTrigger>().SetText(upgrade.name, upgrade.description, upgrade.icon);
        if (upgrade.unlocked)
        {
            labelText.text = upgrade.name;
            if (upgrade.currentLevel >= upgrade.maxLevel)
            {
                purchaseBTN.interactable = false;
                costText.text = "Max Level";
                return;
            }
            costText.text = "";
            if (upgrade.baseResourceCost + upgrade.resourceCostPerLevel > 0)
                costText.text += "<sprite=0>" + upgrade.CalculateResourceCost();
            if (upgrade.baseKnowledgeCost + upgrade.knowledgeCostPerLevel > 0)
                costText.text += "<sprite=1>" + upgrade.CalculateKnowledgeCost();
            GetComponentInParent<ShopScreen>().CheckAvailability();
            purchaseBTN.onClick.AddListener(PurchaseUpgrade);
        }
        else
        {
            labelText.text = "???";
            costText.text = "???";
            purchaseBTN.interactable = false;
        }
    }
    public void PurchaseItem()
    {
        var gameManager = FindObjectOfType<GameManager>();
        gameManager.ChangeResources(-itemIcon.item.resourceCost);
        gameManager.ChangeKnowledge(-itemIcon.item.knowledgeCost);
        if (itemIcon.item.itemType == ItemType.Unit)
            GetComponentInParent<StructureScreen>().currentStructure.SpawnUnit(itemIcon.item.name);
        else
            FindObjectOfType<InventoryManager>(true).AddItem(itemIcon.item, itemIcon.stacks);
        GetComponentInParent<ShopScreen>().CheckAvailability();
    }
    public void PurchaseUpgrade()
    {
        var gameManager = FindObjectOfType<GameManager>();
        gameManager.CmdStructureUpgrade(upgrade.name);
    }
    public void CheckAvailability(GameManager gameManager)
    {
        if (itemIcon.item != null)
        {
            if (gameManager.TestSubtractResources(itemIcon.item.resourceCost) && gameManager.TestSubtractKnowledge(itemIcon.item.knowledgeCost))
                purchaseBTN.interactable = true;
            else
                purchaseBTN.interactable = false;
        }
        else if (upgrade != null)
        {
            if (gameManager.TestSubtractResources(upgrade.CalculateResourceCost()) && gameManager.TestSubtractKnowledge(upgrade.CalculateKnowledgeCost()))
                purchaseBTN.interactable = true;
            else
                purchaseBTN.interactable = false;

            itemIcon.stackText.text = upgrade.currentLevel.ToString();
            if (upgrade.currentLevel >= upgrade.maxLevel)
            {
                purchaseBTN.interactable = false;
                costText.text = "Max Level";
                return;
            }
            costText.text = "";
            if (upgrade.baseResourceCost + upgrade.resourceCostPerLevel > 0)
                costText.text += "<sprite=0>" + upgrade.CalculateResourceCost();
            if (upgrade.baseKnowledgeCost + upgrade.knowledgeCostPerLevel > 0)
                costText.text += "<sprite=1>" + upgrade.CalculateKnowledgeCost();
        }
    }
}
