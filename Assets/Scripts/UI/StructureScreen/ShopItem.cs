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
    public void Initialize(ItemRecipeInfo item)
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
            GetComponentInParent<ShopScreen>().CheckAvailability();
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
    public void CheckAvailability(GameManager gameManager)
    {
        if (gameManager.TestSubtractResources(itemIcon.item.resourceCost) && gameManager.TestSubtractKnowledge(itemIcon.item.knowledgeCost))
            purchaseBTN.interactable = true;
        else
            purchaseBTN.interactable = false;
    }
}
