using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopScreen : MonoBehaviour, WindowedUI
{
    public GameObject window;
    public Transform shopItemList;
    public ShopItem shopItemPrefab;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
            ToggleWindow(false);
    }
    public void ShowScreen(List<ItemRecipeInfo> items)
    {
        ToggleWindow(true);
        foreach (var item in shopItemList.GetComponentsInChildren<ShopItem>())
        {
            Destroy(item.gameObject);
        }
        foreach (var item in items)
        {
            var shopItem = Instantiate(shopItemPrefab, shopItemList);
            shopItem.Initialize(item);
        }
    }
    public void ToggleWindow(bool value)
    {
        if (value)
        {
            window.SetActive(true);
        }
        else
        {
            window.SetActive(false);
        }
    }
    public void ShowWindow()
    {
        ToggleWindow(true);
    }

    public void HideWindow()
    {
        ToggleWindow(false);
    }

    public bool IsActive()
    {
        return window.activeSelf;
    }
    public void CheckAvailability()
    {
        foreach (var item in shopItemList.GetComponentsInChildren<ShopItem>())
        {
            if (item.itemIcon.item.unlocked)
                item.CheckAvailability(FindObjectOfType<GameManager>());
            else
                item.purchaseBTN.interactable = false;
        }
    }
}
