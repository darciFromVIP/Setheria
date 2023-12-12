using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class ItemButton : Button
{
    private short clickCount;
    private InventoryItem item;
    protected override void Awake()
    {
        base.Awake();
        item = GetComponent<InventoryItem>();
    }
    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        clickCount++;
        StartCoroutine(ClickTimer());
        if (clickCount >= 2 && currentSelectionState == SelectionState.Selected)
        {
            TryEquip();
            StopAllCoroutines();
            clickCount = 0;
        }
    }
    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);
        clickCount = 0;
    }
    private IEnumerator ClickTimer()
    {
        yield return new WaitForSeconds(1);
        clickCount = 0;
    }
    public void TryEquip()
    {
        if (item.item.itemType == ItemType.ArchaeologyTool || item.item.itemType == ItemType.Backpack || item.item.itemType == ItemType.Bracelet || item.item.itemType == ItemType.Chest
           || item.item.itemType == ItemType.Feet || item.item.itemType == ItemType.FishingTool || item.item.itemType == ItemType.HandicraftTool
           || item.item.itemType == ItemType.Hands || item.item.itemType == ItemType.Head || item.item.itemType == ItemType.Legs || item.item.itemType == ItemType.GatheringTool
           || item.item.itemType == ItemType.Necklace || item.item.itemType == ItemType.Ring || item.item.itemType == ItemType.Weapon)
        {
            if (item.transform.parent.TryGetComponent(out InventorySlot slot))
            {
                if (item.item.itemType == ItemType.Backpack)
                {
                    foreach (var item in FindObjectOfType<InventoryScreen>(true).GetComponentsInChildren<CharacterGearSlot>(true))
                    {
                        if (item.itemType == ItemType.Backpack && item.transform.childCount == 0)
                        {
                            this.item.parentAfterDrag = this.item.transform.parent;
                            item.EquipItem(this.item);
                            this.item.transform.SetParent(this.item.parentAfterDrag);
                            this.item.transform.position = this.item.parentAfterDrag.position;
                            FindObjectOfType<Tooltip>(true).Hide();
                            break;
                        }
                    }
                }
                foreach (var item in FindObjectOfType<CharacterScreen>(true).GetComponentsInChildren<CharacterGearSlot>(true))
                {
                    if (item.itemType == this.item.item.itemType)
                    {
                        this.item.parentAfterDrag = this.item.transform.parent;
                        item.EquipItem(this.item);
                        this.item.transform.SetParent(this.item.parentAfterDrag);
                        this.item.transform.position = this.item.parentAfterDrag.position;
                        FindObjectOfType<Tooltip>(true).Hide();
                        break;
                    }
                }
            }
            else if (item.transform.parent.TryGetComponent(out CharacterGearSlot gearSlot))
            {
                item.UnequipItem();
                FindObjectOfType<Tooltip>(true).Hide();
            }
        }
    }
}
