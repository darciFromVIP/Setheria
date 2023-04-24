using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class Tooltip : MonoBehaviour
{
    public Image spriteImage;
    public TextMeshProUGUI headerText, contentText;
    public LayoutElement layoutElement;
    public int characterWrapLimit;

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

        rectTransform.pivot = new Vector2(pivotX, 0);
        transform.position = position;
    }
    public void Show(string header, string content, Sprite sprite)
    {
        headerText.text = header;
        contentText.text = content;
        spriteImage.sprite = sprite;
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
