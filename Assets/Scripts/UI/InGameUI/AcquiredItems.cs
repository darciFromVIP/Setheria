using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AcquiredItems : MonoBehaviour
{
    public AcquiredItemUI prefab;
    private float timer;

    private void Update()
    {
        if (timer > 0)
            timer -= Time.deltaTime;
    }

    public void ItemAcquired(ItemRecipeInfo itemData)
    {
        var instance = Instantiate(prefab, transform);
        instance.UpdateUI(itemData.itemData.sprite, itemData.stacks + "x " + itemData.itemData.name);
        FindObjectOfType<RecipeDetail>(true).UpdateCurrentDetails();
    }
    public void RecipeUnlocked(ItemRecipeInfo itemData)
    {
        var instance = Instantiate(prefab, transform);
        instance.UpdateUI(itemData.itemData.sprite, itemData.itemData.name + " unlocked!", true);
        if (timer <= 0)
        {
            FindObjectOfType<AudioManager>().RecipeUnlocked();
            timer = 6;
        }
    }
}
