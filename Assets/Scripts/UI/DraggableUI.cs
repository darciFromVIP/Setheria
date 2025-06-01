using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class DraggableUI : MonoBehaviour, IDragHandler
{
    private RectTransform rectTransform;
    public bool isLocked = true;
    public bool alwaysUnlocked = false;
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (!isLocked)
            rectTransform.anchoredPosition += eventData.delta;
    }
}
