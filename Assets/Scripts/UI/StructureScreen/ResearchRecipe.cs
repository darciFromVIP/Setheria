using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ResearchRecipe : MonoBehaviour
{
    public RecipeScriptable recipe;
    public Sprite hiddenRecipeSprite;
    public Image image, gradeBorder;
    public ItemGradeDatabase gradeDatabase;
    public TooltipTrigger tooltip;
    public void UpdateRecipe()
    {
        if (recipe.unlocked)
        {
            Debug.Log("Recipe Unlocked");
            image.sprite = recipe.resultItem.itemData.sprite;
            tooltip.SetText(recipe.name, recipe.resultItem.itemData.description, recipe.resultItem.itemData.sprite);
        }
        else
        {
            Debug.Log("Recipe Locked");
            image.sprite = hiddenRecipeSprite;
            tooltip.SetText("Hidden Recipe", "This recipe is still hidden. Press 'Research' to attempt to unlock it.", hiddenRecipeSprite);
        }
        gradeBorder.sprite = gradeDatabase.GetBorderByName(recipe.resultItem.itemData.grade.ToString());
    }
}
