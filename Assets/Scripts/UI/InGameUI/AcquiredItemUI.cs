using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AcquiredItemUI : MonoBehaviour
{
    public Image image;
    public TextMeshProUGUI text;

    public void UpdateUI(Sprite sprite, string text)
    {
        image.sprite = sprite;
        this.text.text = text;
    }
}
