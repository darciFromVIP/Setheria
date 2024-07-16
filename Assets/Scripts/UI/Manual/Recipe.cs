using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class Recipe : MonoBehaviour, IPointerEnterHandler
{
    public RecipeScriptable recipeData;
    public TextMeshProUGUI text;
    public bool openedInStructure = false;
    public GameObject newItemNotif, craftableNotif;
    private int hierarchyIndex = 0;

    public List<Recipe> categoryRecipeInstances = new();

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(RecipeClicked);
    }
    private void OnDisable()
    {
        if (recipeData)
            if (recipeData.resultItem.itemData == null && categoryRecipeInstances.Count > 0)
            {
                text.text = text.text.Replace("▼", "▲");
                foreach (var item in categoryRecipeInstances)
                {
                    item.gameObject.SetActive(false);
                }
            }
    }
    public void UpdateText(string text, bool openedInStructure = false)
    {
        this.text.text = text;
        if (text.Contains("▼"))
            ToggleCraftableNotif(false);
        this.openedInStructure = openedInStructure;
    }
    private void RecipeClicked()
    {
        if (recipeData.resultItem.itemData != null)
            FindObjectOfType<RecipeDetail>().UpdateDetails(recipeData, openedInStructure);
        else
        {
            bool active = false;
            foreach (var item in categoryRecipeInstances)
            {
                active = item.gameObject.activeSelf;
                item.gameObject.SetActive(!item.gameObject.activeSelf);
            }
            if (active)
                text.text = text.text.Replace("▲", "▼");
            else
                text.text = text.text.Replace("▼", "▲");
            FindObjectOfType<RecipeDetail>().ClearDetails();
        }
    }
    public void HierarchySignal(int index)
    {
        if (index != 0)
            hierarchyIndex = index;
        foreach (var item in categoryRecipeInstances)
        {
            for (int i = 0; i <= hierarchyIndex; i++)
            {
                item.HierarchySignal(hierarchyIndex + 1);
            }
        }
    }
    public void SetAsCategoryRecipe()
    {
        for (int i = 0; i < hierarchyIndex; i++)
        {
            text.text = text.text.Insert(0, " ");
        }
        gameObject.SetActive(false);
    }
    public void ToggleNewItemNotif(bool value)
    {
        newItemNotif.SetActive(value);
    }
    public void ToggleCraftableNotif(bool value)
    {
        craftableNotif.SetActive(value);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        ToggleNewItemNotif(false);
    }
}
