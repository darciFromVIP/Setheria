using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HeroIcon : MonoBehaviour
{
    [Tooltip("Lycandruid -> Forest Protector")]
    public List<Sprite> heroIcons;

    private Image image;
    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void SetHeroIcon(Hero heroType)
    {
        image.sprite = heroIcons[(int)heroType];
    }
}
