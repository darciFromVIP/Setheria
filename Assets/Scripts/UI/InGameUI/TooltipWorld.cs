using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TooltipWorld : MonoBehaviour
{
    public TextMeshProUGUI headerText, keybindLabel, keybindText;
    private RectTransform rectTransform;
    public LayoutElement layoutElement;
    public Image keybindImage;
    public int characterWrapLimit;
    public GameObject keybindObject;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        Hide();
    }
    private void Update()
    {
        if (IsPointerOverUIElement())
            Hide();
        Vector2 position = Input.mousePosition;
        float pivotX = position.x < Screen.width / 2 ? 0 : 1;
        float pivotY = 0;

        rectTransform.pivot = new Vector2(pivotX, pivotY);
        transform.position = position;
    }
    public void Show(string header, KeybindType type, string keybindLabel)
    {
        if (type != KeybindType.None)
        {
            keybindObject.SetActive(true);
            var keybindData = FindObjectOfType<SettingsManager>().GetDataByKeybindType(type);
            keybindImage.sprite = keybindData.sprite;
            this.keybindLabel.text = keybindLabel;
            keybindText.text = keybindData.text;
        }
        else
            keybindObject.SetActive(false);
        headerText.text = header;
        int headerLength = headerText.text.Length;
        layoutElement.enabled = (headerLength > characterWrapLimit) ? true : false;
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
    //Returns 'true' if we touched or hovering on Unity UI element.
    public bool IsPointerOverUIElement()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults.Count > 0;
    }
}
