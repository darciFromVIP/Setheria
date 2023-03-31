using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string header;
    [TextArea(3, 3)]
    public string content;
    public Sprite sprite;
    public void SetText(string header, string content, Sprite sprite)
    {
        this.header = header;
        this.content = content;
        this.sprite = sprite;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        FindObjectOfType<Tooltip>(true).Show(header, content, sprite);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        FindObjectOfType<Tooltip>(true).Hide();
    }
}
