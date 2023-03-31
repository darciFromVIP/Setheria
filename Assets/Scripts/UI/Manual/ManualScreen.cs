using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ManualScreen : MonoBehaviour
{
    public GameObject manualScreen;
    public GameObject recipeList;
    public GameObject recipeCategories;
    public Recipe recipePrefab;

    private RecipeCategory currentOpenedCategory;

    public RecipeDatabase recipeDatabase;
    private void Awake()
    {
        manualScreen.SetActive(false);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            ToggleWindow();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
            HideWindow();
    }
    private void HideWindow()
    {
        if (manualScreen.activeSelf)
        {
            FindObjectOfType<Tooltip>(true).Hide();
            FindObjectOfType<TooltipWorld>(true).Hide();
            FindObjectOfType<SoundManager>().ManualOpen();
        }
        manualScreen.SetActive(false);
    }
    public void ToggleWindow()
    {
        if (!manualScreen.activeSelf)
        {
            var recipeDetail = FindObjectOfType<RecipeDetail>(true);
            recipeDetail.UpdateCurrentDetails();
            UpdateCurrentCategory();
        }
        FindObjectOfType<SoundManager>().ManualOpen();
        manualScreen.SetActive(!manualScreen.activeSelf);
        if (!recipeCategories.activeSelf)
            ClearRecipeList();
        recipeCategories.SetActive(true);
        FindObjectOfType<Tooltip>(true).Hide();
        FindObjectOfType<TooltipWorld>(true).Hide();
    }
    private void ClearRecipeList()
    {
        foreach (var item in recipeList.transform.GetComponentsInChildren<Transform>())
        {
            if (item != recipeList.transform)
                Destroy(item.gameObject);
        }
    }
    public void UpdateCurrentCategory()
    {
        switch (currentOpenedCategory)
        {
            case RecipeCategory.Survival:
                LoadSurvivalRecipes();
                break;
            case RecipeCategory.Structures:
                LoadStructureRecipes();
                break;
            case RecipeCategory.Alchemy:
                LoadAlchemyRecipes();
                break;
            case RecipeCategory.Fishing:
                LoadFishingRecipes();
                break;
            case RecipeCategory.Armorsmithing:
                LoadArmoryRecipes();
                break;
            case RecipeCategory.Cooking:
                LoadCookingRecipes();
                break;
            default:
                break;
        }
    }
    private void SetRecipeData(List<RecipeScriptable> recipes, bool openedInStructure)
    {
        List<RecipeScriptable> nonVisible = new();
        foreach (var item in recipes)
        {
            if (!item.visible)
            {
                nonVisible.Add(item);
            }
            else
            {
                var recipe = Instantiate(recipePrefab, recipeList.transform);
                recipe.recipeData = item;
                recipe.UpdateText(item.name, openedInStructure);
                if (item.freshlyUnlocked)
                {
                    recipe.ToggleNewItemNotif(true);
                    item.freshlyUnlocked = false;
                }
            }
        }
        foreach (var item in nonVisible)
        {
            var recipe = Instantiate(recipePrefab, recipeList.transform);
            recipe.GetComponent<Button>().interactable = false;
            recipe.UpdateText("???", openedInStructure);
        }
    }
    public void ShowStructureRecipes(List<RecipeScriptable> recipes)
    {
        manualScreen.SetActive(true);
        FindObjectOfType<RecipeDetail>(true).ClearDetails();
        recipeCategories.SetActive(false);
        ClearRecipeList();
        SetRecipeData(recipes, true);
    }
    //The following Load methods are for Buttons
    public void LoadSurvivalRecipes()
    {
        ClearRecipeList();
        SetRecipeData(recipeDatabase.survivalRecipes, false);
        currentOpenedCategory = RecipeCategory.Survival;
    }
    public void LoadStructureRecipes()
    {
        ClearRecipeList();
        SetRecipeData(recipeDatabase.structureRecipes, false);
        currentOpenedCategory = RecipeCategory.Structures;
    }
    public void LoadAlchemyRecipes()
    {
        ClearRecipeList();
        SetRecipeData(recipeDatabase.alchemyRecipes, false);
        currentOpenedCategory = RecipeCategory.Alchemy;
    }
    public void LoadFishingRecipes()
    {
        ClearRecipeList();
        SetRecipeData(recipeDatabase.fishingRecipes, false);
        currentOpenedCategory = RecipeCategory.Fishing;
    }
    public void LoadArmoryRecipes()
    {
        ClearRecipeList();
        SetRecipeData(recipeDatabase.armoryRecipes, false);
        currentOpenedCategory = RecipeCategory.Armorsmithing;
    }
    public void LoadCookingRecipes()
    {
        ClearRecipeList();
        SetRecipeData(recipeDatabase.cookingRecipes, false);
        currentOpenedCategory = RecipeCategory.Cooking;
    }
}
