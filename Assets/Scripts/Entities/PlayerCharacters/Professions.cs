using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Professions
{
    public int gathering, alchemy, cooking, fishing;
    private int gatheringMilestone, alchemyMilestone, cookingMilestone, fishingMilestone;
    private List<int> largeProfMilestones;

    private PlayerCharacter player;

    public Professions(PlayerCharacter player)
    {
        gathering = 0;
        alchemy = 0;
        cooking = 0;
        fishing = 0;
        gatheringMilestone = 25;
        alchemyMilestone = 25;
        cookingMilestone = 25;
        fishingMilestone = 25;
        largeProfMilestones = new List<int> { 50, 75, 100 };
        this.player = player;
    }

    public void AddGathering(int amount)
    {
        gathering += amount;
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
    public void AddAlchemy(int amount)
    {
        alchemy += amount;
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
    }
    public void AddCooking(int amount)
    {
        cooking += amount;
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
    }
    public void AddFishing(int amount)
    {
        fishing += amount;
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
    }
}
