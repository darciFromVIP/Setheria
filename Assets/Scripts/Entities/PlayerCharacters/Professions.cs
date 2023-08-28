using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Professions
{
    public int gathering, alchemy, cooking, fishing;
    public int maxGathering, maxAlchemy, maxCooking, maxFishing;
    private int gatheringMilestone, alchemyMilestone, cookingMilestone, fishingMilestone;
    private List<int> largeProfMilestones;

    [NonSerialized] public PlayerCharacter player;

    public Professions() {}

    public Professions(PlayerCharacter player)
    {
        gathering = 0;
        alchemy = 0;
        cooking = 0;
        fishing = 0;
        maxGathering = 50;
        maxAlchemy = 50;
        maxCooking = 50;
        maxFishing = 50;
        gatheringMilestone = 25;
        alchemyMilestone = 25;
        cookingMilestone = 25;
        fishingMilestone = 25;
        largeProfMilestones = new List<int> { 50, 75, 100 };
        this.player = player;
    }

    public void AddGathering(int amount)
    {
        if (gathering < maxGathering)
        {
            gathering += amount;
            player.SpawnProfessionFloatingText(TalentTreeType.Gathering, amount);
            if (gathering % gatheringMilestone > 0)
            {
                player.talentTrees.ChangeTalentPoints(gathering % gatheringMilestone);
                foreach (var item in largeProfMilestones)
                {
                    if (gatheringMilestone / item < 1)
                    {
                        gatheringMilestone = item;
                    }
                }
            }
        }
    }
    public void AddAlchemy(int amount)
    {
        if (alchemy < maxAlchemy)
        {
            alchemy += amount;
            player.SpawnProfessionFloatingText(TalentTreeType.Alchemy, amount);
            if (alchemy % alchemyMilestone > 0)
            {
                player.talentTrees.ChangeTalentPoints(alchemy % alchemyMilestone);
                foreach (var item in largeProfMilestones)
                {
                    if (alchemyMilestone / item < 1)
                    {
                        alchemyMilestone = item;
                    }
                }
            }
            if (alchemy == 1)
                player.UpdateManualCategories();
        }
    }
    public void AddCooking(int amount)
    {
        if (cooking < maxCooking)
        {
            cooking += amount;
            player.SpawnProfessionFloatingText(TalentTreeType.Cooking, amount);
            if (cooking % cookingMilestone > 0)
            {
                player.talentTrees.ChangeTalentPoints(cooking % cookingMilestone);
                foreach (var item in largeProfMilestones)
                {
                    if (cookingMilestone / item < 1)
                    {
                        cookingMilestone = item;
                    }
                }
            }
            if (cooking == 1)
                player.UpdateManualCategories();
        }
    }
    public void AddFishing(int amount)
    {
        if (fishing < maxFishing)
        {
            fishing += amount;
            player.SpawnProfessionFloatingText(TalentTreeType.Fishing, amount);
            if (fishing % fishingMilestone > 0)
            {
                player.talentTrees.ChangeTalentPoints(fishing % fishingMilestone);
                foreach (var item in largeProfMilestones)
                {
                    if (fishingMilestone / item < 1)
                    {
                        fishingMilestone = item;
                    }
                }
            }
            if (fishing == 1)
                player.UpdateManualCategories();
        }
    }
    public int GetProfessionExperience(TalentTreeType prof)
    {
        switch (prof)
        {
            case TalentTreeType.Gathering:
                return gathering;
            case TalentTreeType.Cooking:
                return cooking;
            case TalentTreeType.Alchemy:
                return alchemy;
            case TalentTreeType.Fishing:
                return fishing;
            default:
                break;
        }
        return 0;
    }
}
