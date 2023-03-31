using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum RecipeCategory
{
    Survival, Structures, Alchemy, Fishing, Armorsmithing, Cooking
}
[CreateAssetMenu(menuName = "Recipe")]
public class RecipeScriptable : ScriptableObject
{
    public RecipeCategory recipeCategory;
    public ItemRecipeInfo resultItem;
    public List<ItemRecipeInfo> componentItems;
    public int resourceCost;
    public bool unlocked = false;
    [Tooltip("Unlocked from previous playthrough but still not craftable unless unlocked again")]
    public bool visible = false;
    [HideInInspector] public bool freshlyUnlocked = false;
    public ItemScriptable requiredStructure;
    public float craftingDuration;
    public int xpGranted;
    public int tier;
    public ItemScriptable recipeItem;
    public List<ItemScriptable> requiredItems = new();
    public List<StructureScriptable> requiredStructures = new();

    private int incompleteRequirements;

    private void OnEnable()
    {
        incompleteRequirements = 0;
        foreach (var item in requiredItems)
        {
            incompleteRequirements++;
            item.Item_Acquired.AddListener(ReduceRequirement);
        }
        foreach (var item in requiredStructures)
        {
            incompleteRequirements++;
            item.Structure_Built.AddListener(ReduceRequirement);
        }
    }
    private void ReduceRequirement()
    {
        incompleteRequirements--;
        if (incompleteRequirements <= 0)
        {
            UnlockRecipe();
        }
    }
    public void UnlockRecipe()
    {
        unlocked = true;
        visible = true;
        freshlyUnlocked = true;
    }
}
