using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum StructureAction
{
    None, Craft, Shop, Upgrade, Demolish, TurnInResourcesAndKnowledge, Research, CookFish, OpenInventory, SetReturnPoint, DrawWater, Rest, StopRest
}
[CreateAssetMenu(menuName = "Structure Option")]
public class StructureOption : ScriptableObject
{
    public StructureAction structureAction;
    [TextArea(3, 3)]
    public string description;
    public Sprite icon;
    public List<RecipeScriptable> craftingRecipes;
    public List<ItemRecipeInfo> soldItems;
    public TalentTreeType professionRequired;
    public int professionLevelRequired;
}
