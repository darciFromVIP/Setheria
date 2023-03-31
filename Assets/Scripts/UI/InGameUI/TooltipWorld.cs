using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class TooltipWorld : MonoBehaviour
{
    public TextMeshProUGUI headerText;
    private RectTransform rectTransform;
    public LayoutElement layoutElement;
    public int characterWrapLimit;

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
    public void Show(string header)
    {
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
