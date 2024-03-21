using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StructureScreen : MonoBehaviour, WindowedUI
{
    public GameObject window;
    public TextMeshProUGUI structureName;
    public Image structureImage;
    public Transform structureOptionsList;
    public StructureOptionUI structureOptionPrefab;
    public Structure currentStructure;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
            HideWindow();
    }
    public void Open(Structure structure)
    {
        currentStructure = structure;
        structureName.text = structure.structureItem.name;
        structureImage.sprite = structure.structureItem.sprite;
        foreach (var item in structureOptionsList.GetComponentsInChildren<Transform>())
        {
            if (item != structureOptionsList)
                Destroy(item.gameObject);
        }
        foreach (var item in structure.structureOptions)
        {
            var option = Instantiate(structureOptionPrefab, structureOptionsList);
            option.Initialize(item.icon, item);
        }
        for (int i = 0; i < 10 - structure.structureOptions.Count; i++)
        {
            var option = Instantiate(structureOptionPrefab, structureOptionsList);
            option.GetComponent<Button>().enabled = false;
            option.GetComponent<TooltipTrigger>().enabled = false;
        }
        window.SetActive(true);
    }
    public void HideWindow()
    {
        window.SetActive(false);
        GetComponentInChildren<ShopScreen>(true).ToggleWindow(false);
        GetComponentInChildren<ResearchScreen>(true).ToggleWindow(false);
        GetComponentInChildren<StashInventory>(true).HideWindow();
    }

    public void ShowWindow()
    {
        window.SetActive(true);
    }

    public bool IsActive()
    {
        return window.activeSelf;
    }
}
