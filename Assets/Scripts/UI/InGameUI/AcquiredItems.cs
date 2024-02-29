using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AcquiredItems : MonoBehaviour
{
    public AcquiredItemUI prefab;

    public void ItemAcquired(ItemRecipeInfo itemData)
    {
        var instance = Instantiate(prefab, transform);
        instance.UpdateUI(itemData.itemData.sprite, itemData.stacks + "x " + itemData.itemData.name);
    }
    public void RecipeUnlocked(ItemRecipeInfo itemData)
    {
        var instance = Instantiate(prefab, transform);
        instance.UpdateUI(itemData.itemData.sprite, itemData.itemData.name + " unlocked!", true);
    }
}
