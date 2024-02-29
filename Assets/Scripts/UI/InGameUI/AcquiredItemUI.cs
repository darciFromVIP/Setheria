using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AcquiredItemUI : MonoBehaviour
{
    public Image image;
    public TextMeshProUGUI text;
    public Material recipeUnlockMaterial;

    public void UpdateUI(Sprite sprite, string text, bool isRecipe = false)
    {
        if (isRecipe)
        {
            GetComponent<Animator>().speed = 0.5f;
            GetComponentInParent<Image>().material = recipeUnlockMaterial;
        }
        image.sprite = sprite;
        this.text.text = text;
    }
}
