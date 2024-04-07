using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
public class ActiveItemSlot : MonoBehaviour, IDropHandler
{
    public SettingsManager settingsManager;
    public KeybindType keybind;
    private KeyCode keyCode;
    public Sprite lockSprite;
    public Sprite emptySprite;
    public bool isFree = false;
    public Image image, keybindImage;
    public TextMeshProUGUI stackText, cooldownText, keybindText;
    public Slider cooldownSlider;

    public InventoryItem reference;
    private TooltipTrigger tooltip;
    public InputEnabledScriptable inputEnabled;

    private void Start()
    {
        tooltip = GetComponent<TooltipTrigger>();
        settingsManager = FindObjectOfType<SettingsManager>();
        settingsManager.Key_Changed.AddListener(UpdateKeybind);
        UpdateKeybind();
        if (!isFree)
        {
            image.sprite = lockSprite;
            SetLockedSlotTooltip();
        }
        else
            SetEmptySlotTooltip();
    }
    private void Update()
    {
        if (!inputEnabled.inputEnabled)
            return;
        if (Input.GetKeyDown(keyCode))
        {
            UseItem();
        }
    }
    private void UpdateKeybind()
    {
        var keybindData = settingsManager.GetDataByKeybindType(keybind);
        keybindImage.sprite = keybindData.sprite;
        keybindText.text = keybindData.text;
        switch (keybind)
        {
            case KeybindType.ActiveItem1:
                keyCode = settingsManager.settings.activeItem1;
                break;
            case KeybindType.ActiveItem2:
                keyCode = settingsManager.settings.activeItem2;
                break;
            case KeybindType.ActiveItem3:
                keyCode = settingsManager.settings.activeItem3;
                break;
            case KeybindType.ActiveItem4:
                keyCode = settingsManager.settings.activeItem4;
                break;
            case KeybindType.ActiveItem5:
                keyCode = settingsManager.settings.activeItem5;
                break;
            case KeybindType.ActiveItem6:
                keyCode = settingsManager.settings.activeItem6;
                break;
            case KeybindType.ActiveItem7:
                keyCode = settingsManager.settings.activeItem7;
                break;
            case KeybindType.ActiveItem8:
                keyCode = settingsManager.settings.activeItem8;
                break;
            default:
                break;
        }
    }
    public void ToggleSlotAvailability(bool value)
    {
        isFree = value;
        if (isFree)
        {
            image.sprite = emptySprite;
            SetEmptySlotTooltip();
        }
        else
        {
            image.sprite = lockSprite;
            SetLockedSlotTooltip();
        }
    }
    public void OnDrop(PointerEventData eventData)
    {
        if (isFree)
        {
            var inventoryItem = eventData.pointerDrag.GetComponent<InventoryItem>();
            if (inventoryItem.parentAfterDrag.TryGetComponent(out CharacterGearSlot slot)
                || (inventoryItem.parentAfterDrag.TryGetComponent(out InventorySlot inventorySlot) && inventoryItem.item.itemType == ItemType.None))
            {
                if (inventoryItem.item.usage.Count > 0 || inventoryItem.item.activeSkills.Count > 0)
                    Initialize(inventoryItem);
            }
        }
    }
    public void Initialize(InventoryItem item)
    {
        reference = item;
        image.sprite = item.item.sprite;
        if (item.item.stackable)
            stackText.text = item.stacks.ToString();
        else
            stackText.text = "";
        item.Item_Destroyed.AddListener(Destroy);
        item.Stacks_Changed.AddListener(ChangeStacks);
        item.Cooldown_Changed.AddListener(UpdateCooldown);
        tooltip.SetText(item.item.name, item.item.description, item.item.sprite);
    }
    private void Destroy()
    {
        reference = null;
        image.sprite = emptySprite;
        stackText.text = "";
        SetEmptySlotTooltip();
        UpdateCooldown(0);
    }
    private void SetEmptySlotTooltip()
    {
        tooltip.SetText("Empty Slot (" + keyCode.ToString()[keyCode.ToString().Length - 1] + ")",
            "This is a slot for an active item. Drag any item that can be activated from your inventory or equipped gear to this slot and use " + keyCode + " to activate it.",
            image.sprite);
    }
    private void SetLockedSlotTooltip()
    {
        tooltip.SetText("Locked Slot (" + keyCode.ToString()[keyCode.ToString().Length - 1] + ")",
            "This is a locked slot for an active item. You have to find a special item which can unlock it.",
            image.sprite);
    }
    private void ChangeStacks(int stacks)
    {
        stackText.text = stacks.ToString();
    }
    public void UseItem()
    {
        if (reference)
        {
            reference.UseItem();
        }
    }
    private void UpdateCooldown(float cd)
    {
        if (!reference)
            cd = 0;
        if (cd > 0)
        {
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
            cooldownSlider.gameObject.SetActive(false);
        }
    }
}
