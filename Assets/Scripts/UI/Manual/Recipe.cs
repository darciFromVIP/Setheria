using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class Recipe : MonoBehaviour, IPointerEnterHandler
{
    public RecipeScriptable recipeData;
    public TextMeshProUGUI text;
    public bool openedInStructure = false;
    public GameObject newItemNotif;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(RecipeClicked);
    }
    public void UpdateText(string text, bool openedInStructure = false)
    {
        this.text.text = text;
        this.openedInStructure = openedInStructure;
    }
    private void RecipeClicked()
    {
        FindObjectOfType<RecipeDetail>().UpdateDetails(recipeData, openedInStructure);
    }
    public void ToggleNewItemNotif(bool value)
    {
        newItemNotif.SetActive(value);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        ToggleNewItemNotif(false);
    }
}
