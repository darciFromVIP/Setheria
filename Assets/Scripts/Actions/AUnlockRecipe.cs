using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Actions/Unlock Recipe")]
public class AUnlockRecipe : ActionTemplate
{
    public RecipeScriptable unlockedRecipe;
    public List<ItemScriptable> unlockedItems = new();
    public override void ActionFinished()
    {
        
    }

    public override void Execute()
    {
        unlockedRecipe.UnlockRecipe();
        foreach (var item in unlockedItems)
        {
            item.unlocked = true;
        }
        FindObjectOfType<ManualScreen>().UpdateCurrentCategory();
        Action_Finished.Invoke();
    }

    public override bool TestExecute()
    {
        return !unlockedRecipe.unlocked;
    }
}
