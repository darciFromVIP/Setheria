using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ManualScreen : MonoBehaviour, WindowedUI
{
    public GameObject manualScreen;
    public GameObject recipeList;
    public GameObject recipeCategories;
    public Recipe recipePrefab;
    public TooltipTrigger alchemyButton, fishingButton, cookingButton;

    private RecipeCategory currentOpenedCategory;
    private SettingsManager settingsManager;

    public RecipeDatabase recipeDatabase;
    private void Awake()
    {
        manualScreen.SetActive(false);
        settingsManager = FindObjectOfType<SettingsManager>();
    }
    private void Update()
    {
        if (Input.GetKeyDown(settingsManager.settings.manual))
        {
            ToggleWindow();
        }
    }
    public void HideWindow()
    {
        if (manualScreen.activeSelf)
        {
            FindObjectOfType<Tooltip>(true).Hide();
            FindObjectOfType<TooltipWorld>(true).Hide();
            FindObjectOfType<AudioManager>().ManualClose();
        }
        manualScreen.SetActive(false);
    }
    public void ToggleWindow()
    {
        if (!manualScreen.activeSelf)
        {
            FindObjectOfType<AudioManager>().ManualOpen();
            var recipeDetail = FindObjectOfType<RecipeDetail>(true);
            recipeDetail.UpdateCurrentDetails();
            UpdateCurrentCategory();
        }
        else
            FindObjectOfType<AudioManager>().ManualClose();
        manualScreen.SetActive(!manualScreen.activeSelf);
        if (!recipeCategories.activeSelf)
            ClearRecipeList();
        recipeCategories.SetActive(true);
        FindObjectOfType<Tooltip>(true).Hide();
        FindObjectOfType<TooltipWorld>(true).Hide();
    }
    public void ShowWindow()
    {
        ToggleWindow();
    }

    public bool IsActive()
    {
        return manualScreen.activeSelf;
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
        foreach (var recipe in recipes)
        {
            if (!recipe.visible)
            {
                nonVisible.Add(recipe);
            }
            else
            {
                var tempRecipe = Instantiate(recipePrefab, recipeList.transform);
                tempRecipe.recipeData = recipe;
                tempRecipe.UpdateText(recipe.name, openedInStructure);
                if (recipe.freshlyUnlocked)
                {
                    tempRecipe.ToggleNewItemNotif(true);
                    recipe.freshlyUnlocked = false;
                }

                tempRecipe.ToggleCraftableNotif(false);
                var currentPlayerItems = FindObjectOfType<InventoryManager>().GetAllItems();
                int matchedCriteria = 0;
                for (int i = 0; i < recipe.componentItems.Count; i++)
                {
                    foreach (var playerItem in currentPlayerItems)
                    {
                        if (playerItem.item == recipe.componentItems[i].itemData)
                        {
                            if (playerItem.stacks >= recipe.componentItems[i].stacks)
                            {
                                matchedCriteria++;
                            }
                            break;
                        }
                    }
                }
                if (matchedCriteria >= recipe.componentItems.Count)
                    tempRecipe.ToggleCraftableNotif(true);
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
    public void UpdateCategoryButtons(Professions prof)
    {
        if (prof.alchemy <= 0)
        {
            alchemyButton.GetComponent<Button>().interactable = false;
            alchemyButton.SetText("Alchemy (Locked)", "Unlock alchemy by using a Beginner's Guide to Alchemy first.");
        }
        else
        {
            alchemyButton.GetComponent<Button>().interactable = true;
            alchemyButton.SetText("Alchemy", "");
        }
        if (prof.cooking <= 0)
        {
            cookingButton.GetComponent<Button>().interactable = false;
            cookingButton.SetText("Cooking (Locked)", "Unlock cooking by using a Beginner's Guide to Cooking first.");
        }
        else
        {
            cookingButton.GetComponent<Button>().interactable = true;
            cookingButton.SetText("Cooking", "");
        }
        if (prof.fishing <= 0)
        {
            fishingButton.GetComponent<Button>().interactable = false;
            fishingButton.SetText("Fishing (Locked)", "Unlock fishing by using a Beginner's Guide to Fishing first.");
        }
        else
        {
            fishingButton.GetComponent<Button>().interactable = true;
            fishingButton.SetText("Fishing", "");
        }
    }
}
