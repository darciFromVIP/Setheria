using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class ButtonSound : MonoBehaviour, IPointerEnterHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        FindObjectOfType<AudioManager>().UIHover();
    }
    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(FindObjectOfType<AudioManager>().UIClick);
    }
}
