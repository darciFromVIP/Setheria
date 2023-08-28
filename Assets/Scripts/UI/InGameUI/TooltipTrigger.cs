using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string header;
    [TextArea(3, 3)]
    public string content;
    public Sprite sprite;
    public string keybindLabel;
    public KeybindType keybindType;
    public Image keyImage;
    public TextMeshProUGUI keyText;
    private void Start()
    {
        var data = FindObjectOfType<SettingsManager>().GetDataByKeybindType(keybindType);
        if (keyImage)
            keyImage.sprite = data.sprite;
        if (keyText)
            keyText.text = data.text;
    }
    private void OnDestroy()
    {
        var tooltip = FindObjectOfType<Tooltip>(true);
        if (tooltip)
            tooltip.Hide();
    }
    public void SetText(string header, string content, Sprite sprite)
    {
        this.header = header;
        this.content = content;
        this.sprite = sprite;
    }
    public void SetText(string header, string content)
    {
        this.header = header;
        this.content = content;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        FindObjectOfType<Tooltip>(true).Show(header, content, sprite, keybindType, keybindLabel);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        FindObjectOfType<Tooltip>(true).Hide();
    }
}
