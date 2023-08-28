using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LockUI : MonoBehaviour
{
    public Sprite lockedSprite, unlockedSprite;
    private Image img;
    private Button btn;
    private bool isLocked = true;
    private void Start()
    {
        img = GetComponent<Image>();
        btn = GetComponent<Button>();
        btn.onClick.AddListener(ButtonClicked);
        foreach (var item in FindObjectsOfType<DraggableUI>(true))
        {
            item.isLocked = isLocked;
        }
    }
    private void ButtonClicked()
    {
        isLocked = !isLocked;
        foreach (var item in FindObjectsOfType<DraggableUI>(true))
        {
            item.isLocked = isLocked;
        }
        if (isLocked)
            img.sprite = lockedSprite;
        else
            img.sprite = unlockedSprite;
    }
}
