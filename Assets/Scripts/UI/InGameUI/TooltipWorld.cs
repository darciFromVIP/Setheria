using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
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
}
