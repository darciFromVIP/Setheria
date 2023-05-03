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
    private Dictionary<ItemScriptable, bool> requiredItemsTemp = new();
    private Dictionary<StructureScriptable, bool> requiredStructuresTemp = new();

    private void OnEnable()
    {
        requiredItemsTemp.Clear();
        requiredStructuresTemp.Clear();
        foreach (var item in requiredItems)
        {
            item.Item_Acquired.AddListener(ReduceRequirement);
            requiredItemsTemp.Add(item, false);
        }
        foreach (var item in requiredStructures)
        {
            item.Structure_Built.AddListener(ReduceRequirement);
            requiredStructuresTemp.Add(item, false);
        }
    }
    private void ReduceRequirement(StructureScriptable structureBuilt)
    {
        requiredStructuresTemp[structureBuilt] = true;
        CheckRecipeAvailability();
    }
    private void ReduceRequirement(ItemScriptable itemAcquired)
    {
        requiredItemsTemp[itemAcquired] = true;
        CheckRecipeAvailability();   
    }
    private void CheckRecipeAvailability()
    {
        foreach (var item in requiredItemsTemp)
        {
            if (item.Value == false)
                return;
        }
        foreach (var item in requiredStructuresTemp)
        {
            if (item.Value == false)
                return;
        }
        UnlockRecipe();
    }
    public void UnlockRecipe()
    {
        unlocked = true;
        visible = true;
        freshlyUnlocked = true;
    }
}
