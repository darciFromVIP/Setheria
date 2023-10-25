using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Professions
{
    public int gathering, alchemy, cooking, fishing, exploration;
    public int maxGathering, maxAlchemy, maxCooking, maxFishing, maxExploration;
    private int gatheringMilestone, alchemyMilestone, cookingMilestone, fishingMilestone, explorationMilestone;
    private List<int> largeProfMilestones;

    [NonSerialized] public PlayerCharacter player;

    public Professions() {}

    public Professions(PlayerCharacter player)
    {
        gathering = 0;
        alchemy = 0;
        cooking = 0;
        fishing = 0;
        exploration = 0;
        maxGathering = 50;
        maxAlchemy = 50;
        maxCooking = 50;
        maxFishing = 50;
        maxExploration = 50;
        gatheringMilestone = 25;
        alchemyMilestone = 25;
        cookingMilestone = 25;
        fishingMilestone = 25;
        explorationMilestone = 25;
        largeProfMilestones = new List<int> { 50, 75, 100 };
        this.player = player;
    }

    public void AddGathering(int amount)
    {
        if (gathering < maxGathering)
        {
            gathering += amount;
            player.SpawnProfessionFloatingText(TalentTreeType.Gathering, amount, gathering, maxGathering);
            if (gathering / gatheringMilestone >= 1)
            {
                player.talentTrees.ChangeTalentPoints(gathering / gatheringMilestone);
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
            player.SpawnProfessionFloatingText(TalentTreeType.Alchemy, amount, alchemy, maxAlchemy);
            if (alchemy / alchemyMilestone >= 1)
            {
                player.talentTrees.ChangeTalentPoints(alchemy / alchemyMilestone);
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
            player.SpawnProfessionFloatingText(TalentTreeType.Cooking, amount, cooking, maxCooking);
            if (cooking/ cookingMilestone >= 1)
            {
                player.talentTrees.ChangeTalentPoints(cooking / cookingMilestone);
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
            player.SpawnProfessionFloatingText(TalentTreeType.Fishing, amount, fishing, maxFishing);
            if (fishing / fishingMilestone >= 1)
            {
                player.talentTrees.ChangeTalentPoints(fishing / fishingMilestone);
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
    public void AddExploration(int amount)
    {
        if (exploration < maxExploration)
        {
            exploration += amount;
            player.SpawnProfessionFloatingText(TalentTreeType.Exploration, amount, exploration, maxExploration);
            if (exploration / explorationMilestone >= 1)
            {
                player.talentTrees.ChangeTalentPoints(exploration / explorationMilestone);
                foreach (var item in largeProfMilestones)
                {
                    if (explorationMilestone / item < 1)
                    {
                        explorationMilestone = item;
                    }
                }
            }
            if (exploration == 1)
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
            case TalentTreeType.Exploration:
                return exploration;
            default:
                break;
        }
        return 0;
    }
    public void AddAnyProfession(TalentTreeType prof, int amount)
    {
        switch (prof)
        {
            case TalentTreeType.Special:
                break;
            case TalentTreeType.Gathering:
                AddGathering(amount);
                break;
            case TalentTreeType.Cooking:
                AddCooking(amount);
                break;
            case TalentTreeType.Alchemy:
                AddAlchemy(amount);
                break;
            case TalentTreeType.Fishing:
                AddFishing(amount);
                break;
            case TalentTreeType.Exploration:
                AddExploration(amount);
                break;
            default:
                break;
        }
    }
}
