using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class ManualScreen : MonoBehaviour, WindowedUI
{
    public GameObject manualScreen;
    public GameObject recipeList;
    public GameObject recipeCategories;
    public GameObject allRecipes;
    public Recipe recipePrefab;
    public TooltipTrigger alchemyButton, fishingButton, cookingButton, explorationButton;

    private RecipeCategory currentOpenedCategory;
    private SettingsManager settingsManager;

    public RecipeDatabase recipeDatabase;
    public InputEnabledScriptable inputEnabled;
    private void Awake()
    {
        manualScreen.SetActive(false);
        settingsManager = FindObjectOfType<SettingsManager>();
    }
    private void Update()
    {
        if (!inputEnabled.inputEnabled)
            return;
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
        allRecipes.SetActive(true);
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
        foreach (var item in recipeList.transform.GetComponentsInChildren<Transform>(true))
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
            case RecipeCategory.Smithing:
                LoadArmoryRecipes();
                break;
            case RecipeCategory.Cooking:
                LoadCookingRecipes();
                break;
            case RecipeCategory.Exploration:
                LoadExplorationRecipes();
                break;
            case RecipeCategory.AllRecipes:
                LoadAllRecipes();
                break;
            default:
                break;
        }
    }
    private void SetRecipeData(List<RecipeScriptable> recipes, bool openedInStructure)
    {
        List<RecipeScriptable> nonVisible = new();
        List<Recipe> recipeCategories = new();
        List<Recipe> allRecipes = new();
        foreach (var recipe in recipes)
        {
            var tempRecipe = Instantiate(recipePrefab, recipeList.transform);
            allRecipes.Add(tempRecipe);
            tempRecipe.recipeData = recipe;
            if (recipe.visible)
                tempRecipe.UpdateText(recipe.name, openedInStructure);
            else
            {
                tempRecipe.UpdateText("???", openedInStructure);
                tempRecipe.GetComponent<Button>().interactable = false;
            }

            if (recipe.resultItem.itemData == null)
                recipeCategories.Add(tempRecipe);
            if (recipe.freshlyUnlocked && recipe.unlocked)
            {
                tempRecipe.ToggleNewItemNotif(true);
                recipe.freshlyUnlocked = false;
            }

            tempRecipe.ToggleCraftableNotif(false);
            if (recipe.componentItems.Count == 0)
                continue;

            if (recipe.visible)
            {
                int matchedCriteria = 0;
                var currentPlayerItems = GetAllAvailableItems();
                for (int i = 0; i < recipe.componentItems.Count; i++)
                {
                    var item = currentPlayerItems.Find(x => x.itemData == recipe.componentItems[i].itemData);
                    if (item != null)
                        if (item.stacks >= recipe.componentItems[i].stacks)
                            matchedCriteria++;
                }
                if (matchedCriteria >= recipe.componentItems.Count)
                    tempRecipe.ToggleCraftableNotif(true);
            }
        }
        foreach (var item in recipeCategories)
        {
            foreach (var item2 in item.recipeData.recipesInThisCategory)
            {
                foreach (var item3 in allRecipes)
                {
                    if (item2 == item3.recipeData)
                    {
                        item.categoryRecipeInstances.Add(item3);
                    }
                }
            }
        }
        foreach (var item in recipeCategories)
        {
            item.HierarchySignal(0);
        }
        foreach (var item in recipeCategories)
        {
            foreach (var item2 in item.categoryRecipeInstances)
            {
                item2.SetAsCategoryRecipe();
            }
        }
    }
    public void ShowStructureRecipes(List<RecipeScriptable> recipes)
    {
        manualScreen.SetActive(true);
        FindObjectOfType<RecipeDetail>(true).ClearDetails();
        recipeCategories.SetActive(false);
        allRecipes.SetActive(false);
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
        currentOpenedCategory = RecipeCategory.Smithing;
    }
    public void LoadCookingRecipes()
    {
        ClearRecipeList();
        SetRecipeData(recipeDatabase.cookingRecipes, false);
        currentOpenedCategory = RecipeCategory.Cooking;
    }
    public void LoadExplorationRecipes()
    {
        ClearRecipeList();
        SetRecipeData(recipeDatabase.explorationRecipes, false);
        currentOpenedCategory = RecipeCategory.Exploration;
    }
    public void LoadAllRecipes()
    {
        ClearRecipeList();
        var player = FindObjectOfType<GameManager>().localPlayerCharacter;
        var allRecipes = new List<RecipeScriptable>();
        allRecipes.AddRange(recipeDatabase.survivalRecipes);
        allRecipes.AddRange(recipeDatabase.structureRecipes);
        if (player.professions.alchemy > 0)
            allRecipes.AddRange(recipeDatabase.alchemyRecipes);
        if (player.professions.fishing > 0)
            allRecipes.AddRange(recipeDatabase.fishingRecipes);
        allRecipes.AddRange(recipeDatabase.armoryRecipes);
        if (player.professions.cooking > 0)
            allRecipes.AddRange(recipeDatabase.cookingRecipes);
        if (player.professions.exploration > 0)
            allRecipes.AddRange(recipeDatabase.explorationRecipes);
        allRecipes.Sort(CompareByIntValue);
        SetRecipeData(allRecipes, false);
        currentOpenedCategory = RecipeCategory.AllRecipes;
    }
    private int CompareByIntValue(RecipeScriptable a, RecipeScriptable b)
    {
        return a.priority.CompareTo(b.priority);
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
        if (prof.exploration <= 0)
        {
            explorationButton.GetComponent<Button>().interactable = false;
            explorationButton.SetText("Exploration (Locked)", "Unlock exploration by using a Beginner's Guide to Exploration first.");
        }
        else
        {
            explorationButton.GetComponent<Button>().interactable = true;
            explorationButton.SetText("Exploration", "");
        }
    }
    public void ShowRecipeByItem(ItemScriptable item)
    {
        if (!manualScreen.activeSelf)
        {
            ShowWindow();
        }
        var recipe = recipeDatabase.GetRecipeByName(item.name);
        if (recipe != null) 
        {
            if (recipe.unlocked && recipe.visible)
            {
                GetComponentInChildren<RecipeDetail>(true).UpdateDetails(recipe, false);
                currentOpenedCategory = recipe.recipeCategory;
                ClearRecipeList();
                UpdateCurrentCategory();
            }
        }
    }
    private List<ItemRecipeInfo> GetAllAvailableItems()
    {
        List<ItemRecipeInfo> currentPlayerItems = new();
        var player = FindObjectOfType<GameManager>().localPlayerCharacter;
        foreach (var item in FindObjectOfType<InventoryManager>(true).GetAllItems())
        {
            var temp = currentPlayerItems.Find(x => x.itemData == item.item);
            if (temp != null)
                temp.stacks += item.stacks;
            else
                currentPlayerItems.Add(new ItemRecipeInfo { itemData = item.item, stacks = item.stacks });
        }
        if (player.IsTentNearby())
        {
            List<ItemRecipeInfo> stashInventory = new();
            foreach (var item in FindObjectOfType<StashInventory>(true).GetAllItems())
            {
                var temp = stashInventory.Find(x => x.itemData == item.item);
                if (temp != null)
                    temp.stacks += item.stacks;
                else
                    stashInventory.Add(new ItemRecipeInfo { itemData = item.item, stacks = item.stacks });
            }
            foreach (var item in stashInventory)
            {
                var temp = currentPlayerItems.Find(x => x.itemData == item.itemData);
                if (temp != null)
                    temp.stacks += item.stacks;
                else
                    currentPlayerItems.Add(item);
            }
        }
        return currentPlayerItems;
    }
}
