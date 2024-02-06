using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[System.Serializable]
[CreateAssetMenu(menuName = "Databases/Recipe Database")]
public class RecipeDatabase : ScriptableObject
{
    [SerializeField] public List<RecipeScriptable> allRecipes;
    [SerializeField] public List<RecipeScriptable> survivalRecipes;
    [SerializeField] public List<RecipeScriptable> structureRecipes;
    [SerializeField] public List<RecipeScriptable> alchemyRecipes;
    [SerializeField] public List<RecipeScriptable> fishingRecipes;
    [SerializeField] public List<RecipeScriptable> armoryRecipes;
    [SerializeField] public List<RecipeScriptable> cookingRecipes;
    [SerializeField] public List<RecipeScriptable> explorationRecipes;
    public RecipeScriptable GetRecipeByName(string name)
    {
        return allRecipes.Find((x) => x.name == name);
    }
#if UNITY_EDITOR
    private void OnEnable()
    {
        hideFlags = HideFlags.HideAndDontSave;
        LoadItemsIntoDatabase();
    }
    [ContextMenu("Load Recipes Into Database")]
    public void LoadItemsIntoDatabase()
    {
        allRecipes = new();
        survivalRecipes = new();
        structureRecipes = new();
        alchemyRecipes = new();
        fishingRecipes = new();
        armoryRecipes = new();
        cookingRecipes = new();
        explorationRecipes = new();
        List<RecipeScriptable> tempRecipes = new();
        tempRecipes.Clear();
        string[] assetNames = AssetDatabase.FindAssets("", new[] { "Assets/Game Data/Recipes" });
        foreach (string SOName in assetNames)
        {
            var SOpath = AssetDatabase.GUIDToAssetPath(SOName);
            var character = AssetDatabase.LoadAssetAtPath<RecipeScriptable>(SOpath);
            tempRecipes.Add(character);
        }
        foreach (var item in tempRecipes)
        {
            if (item.name != "Tent" && item.name != "Provisional Gathering Tool")
            {
                item.unlocked = false;
                item.visible = false;
            }
            allRecipes.Add(item);
            switch (item.recipeCategory)
            {
                case RecipeCategory.Survival:
                    survivalRecipes.Add(item);
                    break;
                case RecipeCategory.Structures:
                    structureRecipes.Add(item);
                    break;
                case RecipeCategory.Alchemy:
                    alchemyRecipes.Add(item);
                    break;
                case RecipeCategory.Fishing:
                    fishingRecipes.Add(item);
                    break;
                case RecipeCategory.Armorsmithing:
                    armoryRecipes.Add(item);
                    break;
                case RecipeCategory.Cooking:
                    cookingRecipes.Add(item);
                    break;
                case RecipeCategory.Exploration:
                    explorationRecipes.Add(item);
                    break;
                default:
                    break;
            }
        }
        survivalRecipes.Sort(CompareByIntValue);
        structureRecipes.Sort(CompareByIntValue);
        alchemyRecipes.Sort(CompareByIntValue);
        fishingRecipes.Sort(CompareByIntValue);
        armoryRecipes.Sort(CompareByIntValue);
        cookingRecipes.Sort(CompareByIntValue);
        explorationRecipes.Sort(CompareByIntValue);
    }
    private int CompareByIntValue(RecipeScriptable a, RecipeScriptable b)
    {
        return a.priority.CompareTo(b.priority);
    }
#endif
}
