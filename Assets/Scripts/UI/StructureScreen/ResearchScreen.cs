using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ResearchScreen : MonoBehaviour
{
    public GameObject window;
    public Button researchBTN;
    private List<ResearchRecipe> recipes = new();
    private int currentTier = 1;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
            ToggleWindow(false);
    }
    private void Start()
    {
        foreach (var item in GetComponentsInChildren<ResearchRecipe>(true))
        {
            recipes.Add(item);
        }
    }
    public void ToggleWindow(bool value)
    {
        window.SetActive(value);
        if (value)
        {
            UpdateRecipes();
            researchBTN.interactable = FindObjectOfType<GameManager>().TestSubtractKnowledge(currentTier * 20);
        }
    }
    public void UpdateRecipes()
    {
        foreach (var item in recipes)
        {
            item.UpdateRecipe();
        }
    }
    public void UnlockRandomRecipe()
    {
        ResearchRecipe result;
        do
        {
            result = recipes[Random.Range(0, recipes.Count)];
        } while (result.recipe.tier != currentTier || result.recipe.unlocked == true);

        FindObjectOfType<InventoryManager>(true).AddItem(result.recipe.recipeItem, 1);
        FindObjectOfType<GameManager>().ChangeKnowledge(-currentTier * 20);
        recipes.Remove(result);
        bool stayInCurrentTier = false;
        foreach (var item in recipes)
        {
            if (item.recipe.tier == currentTier)
                stayInCurrentTier = true;
        }
        if (!stayInCurrentTier)
        {
            currentTier++;
        }
        researchBTN.interactable = FindObjectOfType<GameManager>().TestSubtractKnowledge(currentTier * 20) && recipes.Count > 0;
    }
}
