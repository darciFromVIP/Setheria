using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
public class TurnInItemsInteractable : MonoBehaviour, IInteractable
{
    public List<ItemRecipeInfo> requiredItems;
    public float workDuration;

    public GameObject tooltipUI;
    public Transform requiredItemsParent;
    public Button turnInBTN;
    public InventoryItem inventoryItemPrefab;

    public UnityEvent Items_Turned_In = new();

    public PlayerController player;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
            tooltipUI.SetActive(false);
    }
    public void Interact(PlayerCharacter player)
    {
        this.player = player.GetComponent<PlayerController>();
        foreach (var item in requiredItemsParent.GetComponentsInChildren<InventoryItem>())
        {
            Destroy(item.gameObject);
        }
        var items = FindObjectOfType<InventoryManager>().GetAllItems();
        turnInBTN.interactable = true;
        for (int i = 0; i < requiredItems.Count; i++)
        {
            var newItem = Instantiate(inventoryItemPrefab, requiredItemsParent);
            bool initialized = false;
            foreach (var item in items)
            {
                if (item.item == requiredItems[i].itemData)
                {
                    newItem.InitializeItem(requiredItems[i].itemData, item.stacks, requiredItems[i].stacks);
                    initialized = true;
                    if (item.stacks < requiredItems[i].stacks)
                        turnInBTN.interactable = false;
                    break;
                }
            }
            if (!initialized)
            {
                newItem.InitializeItem(requiredItems[i].itemData, 0, requiredItems[i].stacks);
                turnInBTN.interactable = false;
            }
        }
        tooltipUI.SetActive(true);
    }
    public void TurnInItems()
    {
        player.CmdStartWorking(workDuration);
        player.Work_Finished.AddListener(ItemsTurnedIn);
        tooltipUI.SetActive(false);
    }
    protected virtual void ItemsTurnedIn()
    {
        var inventory = FindObjectOfType<InventoryManager>();
        foreach (var item in requiredItems)
        {
            inventory.RemoveItem(item);
        }
        Items_Turned_In.Invoke();
    }
}
