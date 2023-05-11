using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using Mirror;
public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IDropHandler
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
        if (this.stacks <= 0)
        {
            if (item.itemType == ItemType.HandicraftTool || item.itemType == ItemType.FishingTool || item.itemType == ItemType.HandicraftTool || item.itemType == ItemType.MiningTool)
            {
                FindObjectOfType<SystemMessages>().AddMessage("Your " + item.itemType + " just broke!");
            }
            DestroyItem();
        }
    }
    public void UseItem()
    {
        if (FindObjectOfType<GameManager>().localPlayerCharacter.GetComponent<PlayerController>().state != PlayerState.None)        //Already does something
            return;
        if (!usable)
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
        FindObjectOfType<Tooltip>(true).Hide();
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
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (draggable)
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        newItemNotification.SetActive(false);
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
                player.CmdRemoveBuff(item);
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
        }
    }
}
