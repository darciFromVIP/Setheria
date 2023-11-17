using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IDropHandler, IPointerDownHandler
{
    public Image image, border;
    public TextMeshProUGUI stackText, cooldownText;
    public Slider cooldownSlider;
    public GameObject newItemNotification;
    public LayerMask itemDropMask;
    public ItemGradeDatabase gradeDatabase;
    public Sprite lockedSprite;
    private RectTransform rect;
    private bool draggable = true;
    public bool usable = true;

    [HideInInspector] public Transform parentAfterDrag;
    [HideInInspector] public ItemScriptable item;
    [HideInInspector] public int stacks = 0;
    private StashInventory stashInventoryParent;
    private InventoryManager inventoryManagerParent;
    private StashInventory stashInventory;
    private InventoryManager inventoryManager;

    public UnityEvent Item_Destroyed = new();
    public UnityEvent<int> Stacks_Changed = new();
    public UnityEvent<float> Cooldown_Changed = new();
    private void OnValidate()
    {
        stacks = 0;
    }
    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            parentAfterDrag = transform.parent;
            stashInventoryParent = null;
            inventoryManagerParent = null;
            stashInventoryParent = GetComponentInParent<StashInventory>(true);
            inventoryManagerParent = GetComponentInParent<InventoryManager>(true);
            stashInventory = FindObjectOfType<StashInventory>(true);
            inventoryManager = FindObjectOfType<InventoryManager>(true);
        }
    }
    public void InitializeItem(Item item)
    {
        InitializeItem(item.itemData, item.stacks);
    }
    public void InitializeItem(ItemScriptable item, int stacks, bool draggable = true, bool triggerNewItemNotif = true, bool ignoreLockedItem = true)
    {
        this.item = item;
        if (item.unlocked || ignoreLockedItem)
        {
            image.sprite = item.sprite;
            ChangeStacks(stacks);
            GetComponent<TooltipTrigger>().SetText(item.name, item.description, item.sprite);
        }
        else
        {
            image.sprite = lockedSprite;
            GetComponent<TooltipTrigger>().SetText("Hidden Item", "This item needs to be discovered by researching or finding it in the world.", lockedSprite);
        }
        border.sprite = gradeDatabase.GetBorderByName(item.grade.ToString());
        this.draggable = draggable;
        
        if (item.usage != null && draggable)
        {
            GetComponent<Button>().interactable = true;
            GetComponent<Button>().onClick.AddListener(UseItem);
        }
        if (item.cooldownGroup != CooldownGroup.None)
        {
            var gameManager = FindObjectOfType<GameManager>();
            switch (item.cooldownGroup)
            {
                case CooldownGroup.HealingPotions:
                    gameManager.Healing_Potions_Cooldown.AddListener(UpdateCooldown);
                    break;
                case CooldownGroup.ManaPotions:
                    gameManager.Mana_Potions_Cooldown.AddListener(UpdateCooldown);
                    break;
                default:
                    break;
            }
        }
        newItemNotification.SetActive(triggerNewItemNotif);
    }
    public void InitializeItem(ItemScriptable item, int currentStacks, int requiredStacks)          // For crafting manual
    {
        this.item = item;
        image.sprite = item.sprite;
        border.sprite = gradeDatabase.GetBorderByName(item.grade.ToString());
        if (requiredStacks > 0)
        {
            stackText.gameObject.SetActive(true);
            stackText.text = currentStacks.ToString() + "/" + requiredStacks.ToString();
        }
        else
            stackText.gameObject.SetActive(false);
        if (currentStacks < requiredStacks)
            image.color = new Color(0.5f, 0.5f, 0.5f);
        draggable = false;
        GetComponent<Button>().interactable = false;
        GetComponent<TooltipTrigger>().SetText(item.name, item.description, item.sprite);
        newItemNotification.SetActive(false);
    }
    public void ChangeStacks(int stacks, bool withNotification = true)
    {
        if (stacks > 0 && withNotification)
            newItemNotification.SetActive(true);
        this.stacks += stacks;
        Stacks_Changed.Invoke(this.stacks);
        if (item.stackable)
        {
            stackText.text = this.stacks.ToString();
            stackText.gameObject.SetActive(true);
        }
        if (stacks < 0)
            item.Item_Stacks_Lost.Invoke(item, -stacks);
        if (this.stacks <= 0)
        {
            DestroyItem();
        }
    }
    public void UseItem()
    {
        if (FindObjectOfType<GameManager>().localPlayerCharacter.GetComponent<PlayerController>().state != PlayerState.None)        //Already does something
            return;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftControl))
            return;
        if (!usable)
            return;
        if (transform.parent.TryGetComponent(out StashSlot stashSlot))
            return;
        if (item.itemType != ItemType.None)
        {
            var gearSlot = GetComponentInParent<CharacterGearSlot>(true);
            if (!gearSlot)
            {
                return;
            }
        }
        bool used = false;
        foreach (var item in item.usage)
        {
            if (item.TestExecute())
            {
                if (this.item.destroyItemOnUse)
                {
                    item.Action_Finished.RemoveAllListeners();
                    item.Action_Finished.AddListener(DestroyItem);
                }
                item.Execute();
                used = true;
            }
            else
                break;
        }
        if (used && item.stackable)
        {
            ChangeStacks(-1);
        }
        if (used)
        {
            if (item.cooldownGroup != CooldownGroup.None)
                FindObjectOfType<GameManager>().StartCooldown(item.cooldownGroup, item.usageCooldown);
            else
                FindObjectOfType<GameManager>().StartIndependentCooldown(UpdateCooldown, item.usageCooldown);
        }
    }
    public void DestroyItem()
    {
        transform.SetParent(null);                      // Destroy executes after current Update loop, too late for certain functions (with crafting)
        item.Item_Stacks_Lost.Invoke(item, stacks);
        Item_Destroyed.Invoke();
        Destroy(gameObject);
    }
    
    private void UpdateCooldown(float cd)
    {
        if (cd > 0)
        {
            usable = false;
            if (!cooldownSlider.gameObject.activeSelf)
                cooldownSlider.maxValue = cd;
            cooldownSlider.gameObject.SetActive(true);
            cooldownSlider.value = cd;
            if (cd >= 1)
                cooldownText.text = ((int)cd).ToString();
            else
                cooldownText.text = cd.ToString("F1");
        }
        else
        {
            usable = true;
            cooldownSlider.gameObject.SetActive(false);
        }
        Cooldown_Changed.Invoke(cd);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        newItemNotification.SetActive(false);
        QuickItemTransfer();
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (Input.GetKey(KeyCode.LeftControl) && item.stackable && !Input.GetKey(KeyCode.LeftShift))
        {
            if (stacks > 1)
            {
                int newStacks = stacks / 2;
                if (transform.parent.TryGetComponent(out StashSlot slot))
                    slot.CmdChangeStacks(-newStacks);
                else
                    ChangeStacks(-newStacks);
                FindObjectOfType<InventoryManager>(true).AddItem(item, newStacks, false);
            }
        }
        else QuickItemTransfer();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (draggable && !Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.LeftShift))
        {
            image.raycastTarget = false;
            parentAfterDrag = transform.parent;
            transform.SetParent(transform.root);
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(0, 0);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggable)
            rect.anchoredPosition = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggable)
        {
            image.raycastTarget = true;
            transform.SetParent(parentAfterDrag);
            transform.position = parentAfterDrag.position;

            if (parentAfterDrag.TryGetComponent(out StashSlot slot))
                return;

            List<RaycastResult> results = new();
            EventSystem.current.RaycastAll(eventData, results);
            if (results.Count > 0)
                return;
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, itemDropMask))
            {
                if (hit.collider is TerrainCollider)
                {
                    StartCoroutine(FindObjectOfType<GameManager>().localPlayerCharacter.GoToDropItem(this, hit.point));
                }
                if (hit.collider.TryGetComponent(out PlayerCharacter player))
                {
                    StartCoroutine(FindObjectOfType<GameManager>().localPlayerCharacter.GoToGiveItem(this, player));
                }
            }
        }
    }
    private void QuickItemTransfer()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Mouse0) && !Input.GetKey(KeyCode.LeftControl))
        {
            if (inventoryManagerParent && stashInventory)                       // To Stash
            {
                stashInventory.AddItem(item, stacks);
                DestroyItem();
            }
            else if (stashInventoryParent && inventoryManager)                  // To Inventory
            {
                if (!inventoryManager.GetFreeSlot())
                    return;
                inventoryManager.AddItem(item, stacks);
                GetComponentInParent<StashSlot>().CmdDeleteItemOnClients();
            }
        }
    }
    public void UnequipItem()
    {
        var slot = FindObjectOfType<InventoryManager>(true).GetFreeSlot();
        if (slot)
        {
            transform.SetParent(slot);
            var player = FindObjectOfType<GameManager>().localPlayerCharacter;
            foreach (var item in item.passiveBuffs)
            {
                if (item.buffType == BuffType.InventorySlots)
                {
                    if (!FindObjectOfType<InventoryManager>(true).TestReduceInventory((int)item.value))
                        return;
                }
            }
            foreach (var item in item.passiveBuffs)
            {
                player.CmdRemoveBuff(item.name);
            }
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        var inventoryItem = eventData.pointerDrag.GetComponent<InventoryItem>();
        if (inventoryItem)
        {
            if (inventoryItem.item == item && item.stackable)
            {
                if (transform.parent.TryGetComponent(out StashSlot parentSlot))
                {
                    parentSlot.CmdChangeStacks(inventoryItem.stacks);
                }
                else
                    ChangeStacks(inventoryItem.stacks, false);
                if (inventoryItem.parentAfterDrag.TryGetComponent(out StashSlot stashSlot))
                {
                    stashSlot.CmdDeleteItemOnClients();
                }
                inventoryItem.DestroyItem();
            }
            else if (!Input.GetKey(KeyCode.LeftShift))
            {
                var myParent = transform.parent;
                var otherParent = inventoryItem.parentAfterDrag;
                inventoryItem.parentAfterDrag = transform.parent;
                transform.SetParent(otherParent);
                transform.position = otherParent.transform.position;
                if (otherParent.TryGetComponent(out StashSlot slot))
                {
                    slot.CmdDeleteItemOnClients();
                    slot.CmdSpawnItemOnThisSlot(item.name, stacks);
                }
                if (myParent.TryGetComponent(out StashSlot stashSlot))
                {
                    stashSlot.CmdDeleteItemOnClients();
                    stashSlot.CmdSpawnItemOnThisSlot(inventoryItem.item.name, inventoryItem.stacks);
                }
            }
        }
    }
    //Returns 'true' if we touched or hovering on Unity UI element.
    public bool IsPointerOverUIElement()
    {
        return IsPointerOverUIElement(GetEventSystemRaycastResults());
    }
    //Returns 'true' if we touched or hovering on Unity UI element.
    private bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
    {
        for (int index = 0; index < eventSystemRaysastResults.Count; index++)
        {
            RaycastResult curRaysastResult = eventSystemRaysastResults[index];
            if (curRaysastResult.gameObject == gameObject)
                return true;
        }
        return false;
    }

    //Gets all event system raycast results of current mouse or touch position.
    static List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }
}
