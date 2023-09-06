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
        FindObjectOfType<GameManager>().CmdUnlockRecipe(unlockedRecipe.name);
        FindObjectOfType<ManualScreen>().UpdateCurrentCategory();
        Action_Finished.Invoke();
    }

    public override bool TestExecute()
    {
        return !unlockedRecipe.unlocked;
    }
}
