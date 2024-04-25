using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Mirror;
using HighlightPlus;
public class TurnInItemsInteractable : NetworkBehaviour, IInteractable, ISaveable
{
    public List<ItemRecipeInfo> requiredItems;
    public float workDuration;

    public GameObject tooltipUI;
    public Transform requiredItemsParent;
    public Button turnInBTN;
    public InventoryItem inventoryItemPrefab;

    public UnityEvent Items_Turned_In = new();
    public EventScriptable Quest_Event;

    public Animator animator;
    public PlayerController player;

    public bool interactable = true;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
            tooltipUI.SetActive(false);
    }
    public virtual void Interact(PlayerCharacter player)
    {
        if (!interactable)
            return;
        this.player = player.GetComponent<PlayerController>();
        foreach (var item in requiredItemsParent.GetComponentsInChildren<InventoryItem>(true))
        {
            Destroy(item.gameObject);
        }
        var items = FindObjectOfType<InventoryManager>(true).GetAllItems();
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
        if (interactable)
        {
            var inventory = FindObjectOfType<InventoryManager>(true);
            foreach (var item in requiredItems)
            {
                inventory.RemoveItem(item);
            }
        }
        if (animator)
            CmdSetAnimation();
        Items_Turned_In.Invoke();
        if (Quest_Event != null)
            Quest_Event.voidEvent.Invoke();
        interactable = false;
        CmdTurnOffCollider();
        var outline = GetComponentInChildren<HighlightTrigger>();
        if (outline)
            outline.enabled = interactable;
        if (TryGetComponent(out ObjectMapIcon mapIcon))
        {
            mapIcon.CmdToggleCheckmark();
        }
    }
    [Command(requiresAuthority = false)]
    private void CmdTurnOffCollider()
    {
        RpcTurnOffCollider();
    }
    [ClientRpc]
    private void RpcTurnOffCollider()
    {
        GetComponent<Collider>().enabled = false;
    }
    [Command(requiresAuthority = false)]
    private void CmdSetAnimation()
    {
        RpcSetAnimation();
    }
    [ClientRpc]
    private void RpcSetAnimation()
    {
        animator.SetTrigger("ItemTurnedIn");
    }

    public SaveDataWorldObject SaveState()
    {
        return new SaveDataWorldObject
        {
            boolData1 = interactable
        };
    }

    public void LoadState(SaveDataWorldObject state)
    {
        if (!interactable)
            ItemsTurnedIn();
    }
    [Command(requiresAuthority = false)]
    public void CmdDestroy()
    {
        if (TryGetComponent(out ObjectMapIcon mapIcon))
        {
            mapIcon.RpcDestroyIcon();
        }
        NetworkServer.Destroy(gameObject);
    }
}
