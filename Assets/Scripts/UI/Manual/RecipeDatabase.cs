using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CreateAssetMenu(menuName = "Databases/Recipe Database")]
public class RecipeDatabase : ScriptableObject
{
    public List<RecipeScriptable> allRecipes;
    public List<RecipeScriptable> survivalRecipes;
    public List<RecipeScriptable> structureRecipes;
    public List<RecipeScriptable> alchemyRecipes;
    public List<RecipeScriptable> fishingRecipes;
    public List<RecipeScriptable> armoryRecipes;
    public List<RecipeScriptable> cookingRecipes;
    public List<RecipeScriptable> explorationRecipes;
    public RecipeScriptable GetRecipeByName(string name)
    {
        return allRecipes.Find((x) => x.name == name);
    }
#if UNITY_EDITOR
    private void OnEnable()
    {
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
    }
#endif
}
