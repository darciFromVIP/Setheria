using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class SaveDataPlayer
{
    public float positionX;
    public float positionY;
    public float positionZ;
    public float rotationW;
    public float rotationX;
    public float rotationY;
    public float rotationZ;
    public float everstonePointX;
    public float everstonePointY;
    public float everstonePointZ;
    public string name;
    public Hero hero;
    public int level;
    public int xp;
    public int maxXp;
    public int attributePoints;
    public List<SaveDataItem> inventory = new();
    public List<SaveDataItem> equippedGear = new();
    public int hunger;
    public int maxHunger;
    public float hungerInterval;
    public float health;
    public float baseMaxHealth;
    public float baseHealthRegen;
    public float corruptedHealth;
    public float corruption;
    public float mana;
    public float baseMaxMana;
    public float baseManaRegen;
    public float corruptedMana;
    public float power;
    public float criticalChance;
    public float criticalDamage;
    public float attackSpeed;
    public float attackRange;
    public float armor;
    public float cooldownReduction;
    public float cooldown1;
    public float cooldown2;
    public float cooldown3; 
    public float cooldown4; 
    public float cooldown5;
    public TalentTrees talentTrees;
    public Professions professions;
    public List<QuestlineSaveable> unsyncedQuestlines = new();
    public List<bool> unlockedItems = new();
    public List<bool> unlockedRecipes = new();

    public SaveDataPlayer() { }
    public SaveDataPlayer(Hero hero)
    {
        switch (hero)
        {
            case Hero.Lycandruid:
                name = "Wolferius the Lycandruid";
                health = 250;
                baseMaxHealth = 250;
                baseHealthRegen = 0.6f;
                mana = 150;
                baseMaxMana = 150;
                baseManaRegen = 0.25f;
                power = 15;
                criticalChance = 0;
                criticalDamage = 50;
                attackSpeed = 4f;
                attackRange = 1.5f;
                armor = 0;
                cooldownReduction = 0;
                break;
            case Hero.ForestProtector:
                name = "Nirri the Forest Protector";
                health = 150;
                baseMaxHealth = 150;
                baseHealthRegen = 0.25f;
                mana = 250;
                baseMaxMana = 250;
                baseManaRegen = 0.6f;
                power = 10;
                criticalChance = 0;
                criticalDamage = 50;
                attackSpeed = 4f;
                attackRange = 7;
                armor = 0;
                cooldownReduction = 0;
                break;
            default:
                break;
        }
        positionX = 0;
        positionY = 0;
        positionZ = 0;
        level = 1;
        xp = 0;
        maxXp = 100;
        attributePoints = 0;
        this.hero = hero;
        hunger = 100;
        maxHunger = 100;
        hungerInterval = 20;
    }
}
