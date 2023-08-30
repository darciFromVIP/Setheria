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
    public string name;
    public Hero hero;
    public int level;
    public int xp;
    public int maxXp;
    public int attributePoints;
    public List<SaveDataItem> inventory = new();
    public int hunger;
    public int maxHunger;
    public float hungerInterval;
    public float health;
    public float baseMaxHealth;
    public float bonusMaxHealth;
    public float baseHealthRegen;
    public float bonusHealthRegen;
    public float mana;
    public float baseMaxMana;
    public float bonusMaxMana;
    public float baseManaRegen;
    public float bonusManaRegen;
    public float power;
    public float criticalChance;
    public float criticalDamage;
    public float attackSpeed;
    public float attackRange;
    public float armor;
    public float cooldownReduction;
    public TalentTrees talentTrees;
    public Professions professions;

    public SaveDataPlayer() { }
    public SaveDataPlayer(Hero hero)
    {
        switch (hero)
        {
            case Hero.Lycandruid:
                name = "Wolferius the Lycandruid";
                health = 400;
                baseMaxHealth = 400;
                baseHealthRegen = 0.6f;
                mana = 200;
                baseMaxMana = 200;
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
                health = 300;
                baseMaxHealth = 300;
                baseHealthRegen = 0.25f;
                mana = 300;
                baseMaxMana = 300;
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
