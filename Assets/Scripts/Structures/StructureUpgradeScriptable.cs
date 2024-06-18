using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
[CreateAssetMenu(menuName = "Structure Upgrade")]
public class StructureUpgradeScriptable : ScriptableObject
{
    public int currentLevel = 0, maxLevel, baseResourceCost, resourceCostPerLevel, baseKnowledgeCost, knowledgeCostPerLevel;
    public Sprite icon;
    public bool unlocked;
    [TextArea(5, 5)]
    public string description;

    public UnityEvent upgradeEvent;

    private void OnEnable()
    {
        currentLevel = 0;
    }

    public int CalculateResourceCost()
    {
        return baseResourceCost + (resourceCostPerLevel * (currentLevel + 1));
    }
    public int CalculateKnowledgeCost()
    {
        return baseKnowledgeCost + (knowledgeCostPerLevel * (currentLevel + 1));
    }
    public void Upgrade()
    {
        currentLevel++;
        upgradeEvent.Invoke();
    }
}
