using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StructureScreen : MonoBehaviour
{
    public GameObject window;
    public TextMeshProUGUI structureName;
    public Image structureImage;
    public Transform structureOptionsList;
    public StructureOptionUI structureOptionPrefab;
    public Structure currentStructure;
    private void Start()
    {
        Close();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.Escape))
            Close();
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
        window.SetActive(true);
    }
    public void Close()
    {
        window.SetActive(false);
        GetComponentInChildren<ShopScreen>(true).ToggleWindow(false);
        GetComponentInChildren<ResearchScreen>(true).ToggleWindow(false);
    }
}
