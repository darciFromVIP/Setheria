using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum RecipeCategory
{
    Survival, Structures, Alchemy, Fishing, Smithing, Cooking, Exploration, AllRecipes
}
[CreateAssetMenu(menuName = "Recipe")]
public class RecipeScriptable : ScriptableObject
{
    public RecipeCategory recipeCategory;
    [Tooltip("1 means top of the list, the higher the priority, the lower on the list this recipe appears.")]
    public int priority;
    public ItemRecipeInfo resultItem;
    public List<ItemRecipeInfo> componentItems;
    public int resourceCost;
    public bool unlocked = false;
    [Tooltip("Unlocked from previous playthrough but still not craftable unless unlocked again")]
    public bool visible = false;
    [HideInInspector] public bool freshlyUnlocked = false;
    [Tooltip("The structure at which you have to craft this item.")]
    public ItemScriptable requiredStructure;
    public float craftingDuration;
    public int xpGranted;
    public int tier;
    [Tooltip("An item which unlocks this recipe.")]
    public ItemScriptable recipeItem;
    [Tooltip("The items which unlock this recipe upon acquisition.")]
    public List<ItemScriptable> requiredItems = new();
    [Tooltip("The structures which unlock this recipe upon building.")]
    public List<StructureScriptable> requiredStructures = new();
    private Dictionary<ItemScriptable, bool> requiredItemsTemp = new();
    private Dictionary<StructureScriptable, bool> requiredStructuresTemp = new();
    public TalentTreeType requiredProfession;
    public int requiredProfessionExperience;
    public List<RecipeScriptable> recipesInThisCategory = new();

    [HideInInspector] public UnityEvent Recipe_Unlocked;

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
        FindObjectOfType<GameManager>().CmdUnlockRecipe(name);
    }
    public void UnlockRecipe()
    {
        Debug.Log(name);
        if (!unlocked && resultItem.itemData != null)
            FindObjectOfType<AcquiredItems>().RecipeUnlocked(resultItem);
        unlocked = true;
        visible = true;
        freshlyUnlocked = true;
        Recipe_Unlocked.Invoke();
    }
}
