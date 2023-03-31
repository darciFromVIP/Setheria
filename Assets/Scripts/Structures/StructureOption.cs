using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum StructureAction
{
    None, Craft, Shop, Upgrade, Destroy, TurnInResourcesAndKnowledge, Research, CookFish, OpenInventory
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
}
