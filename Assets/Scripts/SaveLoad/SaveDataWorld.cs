using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[System.Serializable]
public class SaveDataWorld
{
    public string worldName;
    public int worldSeed;
    public byte[] fogOfWar;
    public List<QuestlineSaveable> syncedQuestlines;
    public int resources, knowledge;
    public List<int> structureUpgrades = new();
    public List<SaveDataItem> stash = new();
    public List<bool> unlockedItems = new();
    public List<bool> unlockedRecipes = new();
}
