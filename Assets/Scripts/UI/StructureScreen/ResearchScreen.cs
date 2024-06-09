using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ResearchScreen : MonoBehaviour, WindowedUI
{
    public GameObject window;
    public Button researchBTN;
    private List<ResearchRecipe> recipes = new();
    private int currentTier = 1;
    private bool allResearchesDone;
    public List<Button> tierButtons = new();
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
            item.recipe.Recipe_Unlocked.AddListener(UpdateRecipes);
        }
    }
    public void ToggleWindow(bool value)
    {
        window.SetActive(value);
        if (value)
        {
            UpdateRecipes();
        }
    }
    public void ShowWindow()
    {
        ToggleWindow(true);
    }

    public void HideWindow()
    {
        ToggleWindow(false);
    }

    public bool IsActive()
    {
        return window.activeSelf;
    }
    public void UpdateRecipes()
    {
        CheckTier();
        foreach (var item in recipes)
        {
            item.UpdateRecipe();
        }
        researchBTN.interactable = FindObjectOfType<GameManager>().TestSubtractKnowledge(currentTier * 20) && !allResearchesDone;
        researchBTN.GetComponentInChildren<TextMeshProUGUI>().text = "Research<sprite=1>" + currentTier * 20;
    }
    public void UnlockRandomRecipe()
    {  
        ResearchRecipe result;
        do
        {
            result = recipes[Random.Range(0, recipes.Count)];
        } while (result.recipe.tier != currentTier || result.recipe.unlocked);
        FindObjectOfType<GameManager>().ChangeKnowledge(-currentTier * 20);
        researchBTN.interactable = FindObjectOfType<GameManager>().TestSubtractKnowledge(currentTier * 20) && recipes.Count > 0 && !allResearchesDone;
        FindObjectOfType<GameManager>().CmdUnlockRecipe(result.recipe.name);
    }
    public void CheckTier()
    {
        allResearchesDone = true;
        foreach (var item in recipes)
        {
            if (!item.recipe.unlocked && item.recipe.tier == currentTier)
                allResearchesDone = false;
        }
        if (allResearchesDone)
            researchBTN.interactable = false;
    }
    public void ChangeCurrentTier(int tier)
    {
        currentTier = tier;
        UpdateRecipes();
    }
    public void UnlockTier(int tier)
    {
        tierButtons[tier - 1].interactable = true;
    }
}
