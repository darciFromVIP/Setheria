using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class Tooltip : MonoBehaviour
{
    public Image spriteImage, keybindImage;
    public TextMeshProUGUI headerText, contentText, keybindLabel, keybindText;
    public LayoutElement layoutElement;
    public int characterWrapLimit;
    public GameObject keybindObject;

    private RectTransform rectTransform;
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        Hide();
    }
    private void Update()
    {
        Vector2 position = Input.mousePosition;
        float pivotX = position.x < Screen.width / 2 ? 0 : 1;
        float pivotY = position.y < Screen.width / 2 ? 0 : 1.2f;

        rectTransform.pivot = new Vector2(pivotX, pivotY);
        transform.position = position;
    }
    public void Show(string header, string content, Sprite sprite, KeybindType keybindType, string keybindLabel)
    {
        headerText.text = header;
        contentText.text = content;
        spriteImage.sprite = sprite;
        if (keybindType != KeybindType.None)
        {
            keybindObject.SetActive(true);
            var keybindData = FindObjectOfType<SettingsManager>().GetDataByKeybindType(keybindType);
            keybindImage.sprite = keybindData.sprite;
            this.keybindLabel.text = keybindLabel;
            keybindText.text = keybindData.text;
        }
        else
            keybindObject.SetActive(false);
        if (sprite == null)
            spriteImage.gameObject.SetActive(false);
        else
            spriteImage.gameObject.SetActive(true);

        int headerLength = headerText.text.Length;
        int contentLength = contentText.text.Length;
        layoutElement.enabled = (headerLength > characterWrapLimit || contentLength > characterWrapLimit) ? true : false;

        gameObject.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
