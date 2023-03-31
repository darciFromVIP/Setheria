using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class RandomImages : MonoBehaviour
{
    public List<Sprite> randomImages;
    private Image img;

    private void Awake()
    {
        img = GetComponent<Image>();
        int random = Random.Range(0, randomImages.Count);
        img.sprite = randomImages[random];
    }
}
