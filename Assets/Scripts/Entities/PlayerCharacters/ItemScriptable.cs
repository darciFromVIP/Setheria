using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public enum ItemGrade
{
    Common, Uncommon, Rare, Unique, Legendary
}
public enum ItemType
{
    None, Head, Chest, Hands, Legs, Feet, Weapon, Backpack, Necklace, Ring, Bracelet, HandicraftTool, GatheringTool, ArchaeologyTool, FishingTool, Resources, Knowledge, Unit, Fish
}
public enum CooldownGroup
{
    None, HealingPotions, ManaPotions
}
[CreateAssetMenu(menuName = "Item")]
public class ItemScriptable : ScriptableObject
{
    [TextArea(3, 3)]
    public string description;
    public ItemGrade grade;
    public ItemType itemType;
    public Sprite sprite;
    public bool stackable = true;
    public List<ActionTemplate> usage;
    public float usageCooldown;
    public CooldownGroup cooldownGroup;
    public List<BuffScriptable> passiveBuffs;
    [Tooltip("Miscellaneous Value - For Tools: Level of Tool, For Items worth Resources/Knowledge: How much of them is it worth")]
    public int value;
    public bool destroyItemOnUse = false;
    public int resourceCost;
    public int knowledgeCost;
    public bool unlocked = true;

    public UnityEvent<ItemScriptable> Item_Acquired = new();
    public UnityEvent<ItemScriptable, int> Item_Stacks_Acquired = new();
}
