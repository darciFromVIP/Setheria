using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class RecipeDetail : MonoBehaviour, NeedsLocalPlayerCharacter
{
    public InventoryItem inventoryItemPrefab;
    public TextMeshProUGUI resultItemNameText, resourceCostText, structureRequirement;
    public Transform resultItemParent, components;
    public Button craftBtn;
    public GameObject amountUI, blockingUI;
    public TMP_InputField amountInput;

    private int amount = 1;

    private PlayerController localPlayer;

    private List<Transform> componentParents = new();
    public List<InventoryItem> currentPlayerItems;
    private RecipeScriptable currentOpenedRecipe;
    private bool openedInStructure;
    private void Start()
    {
        foreach (var item in components.GetComponentsInChildren<Image>(true))
        {
            componentParents.Add(item.transform);
        }
    }
    public void UpdateDetails(RecipeScriptable recipeData, bool openedInStructure, int amount = 1)
    {
        ClearDetails();        
        this.amount = amount;
        amountInput.text = amount.ToString();
        if (recipeData.resultItem.itemData.stackable)
        {
            foreach (var item in amountUI.GetComponentsInChildren<Selectable>())
            {
                item.interactable = true;
            }
        }
        else
        {
            foreach (var item in amountUI.GetComponentsInChildren<Selectable>())
            {
                item.interactable = false;
            }
        }
        
        currentPlayerItems = FindObjectOfType<InventoryManager>(true).GetAllItems();

        this.openedInStructure = openedInStructure;
        craftBtn.interactable = recipeData.unlocked && (recipeData.requiredStructure == null ? !openedInStructure : openedInStructure);
        currentOpenedRecipe = recipeData;
        resultItemNameText.text = recipeData.resultItem.itemData.name;
        structureRequirement.text = "Structure: " + (recipeData.requiredStructure == null ? "None" : recipeData.requiredStructure.name);

        var resultItem = Instantiate(inventoryItemPrefab, resultItemParent);
        resultItem.stackText.text = (recipeData.resultItem.stacks * amount).ToString();
        resultItem.InitializeItem(recipeData.resultItem.itemData, recipeData.resultItem.stacks * amount, false, false);
        
        for (int i = 0; i < recipeData.componentItems.Count; i++)
        {
            foreach (var item in currentPlayerItems)
            {
                if (item.item == recipeData.componentItems[i].itemData)
                {
                    var newItem = Instantiate(inventoryItemPrefab, componentParents[i].transform);
                    newItem.InitializeItem(recipeData.componentItems[i].itemData, item.stacks , recipeData.componentItems[i].stacks * amount);
                    if (item.stacks < recipeData.componentItems[i].stacks * amount)
                        craftBtn.interactable = false;
                    break;
                }
            }
            if (componentParents[i].transform.childCount == 0)
            {
                var newItem = Instantiate(inventoryItemPrefab, componentParents[i].transform);
                newItem.InitializeItem(recipeData.componentItems[i].itemData, 0, recipeData.componentItems[i].stacks * amount);
                craftBtn.interactable = false;
            }
        }
        resourceCostText.text = "Resources: " + (recipeData.resourceCost * amount).ToString();
        if (!FindObjectOfType<GameManager>().TestSubtractResources(recipeData.resourceCost * amount))
            craftBtn.interactable = false;
    }    
    public void ClearDetails()
    {
        currentOpenedRecipe = null;
        resultItemNameText.text = "";
        if (resultItemParent.childCount > 0)
            Destroy(resultItemParent.GetChild(0).gameObject);
        foreach (var item in componentParents)
        {
            if (item.transform.childCount > 0)
            {
                var temp = item.transform.GetChild(0).gameObject;
                temp.transform.SetParent(null);
                Destroy(temp);                                      //Destroy() executes after the current Update loop, so the UpdateDetails() func still sees those objects in the same frame
            }
        }
        resourceCostText.text = "";
        craftBtn.interactable = false;
    }
    public void UpdateCurrentDetails()
    {
        if (currentOpenedRecipe)
            UpdateDetails(currentOpenedRecipe, openedInStructure);
    }

    public void CraftCurrentItem()
    {
        if (localPlayer.state == PlayerState.None)
        {
            blockingUI.SetActive(true);
            localPlayer.CmdStartWorking(currentOpenedRecipe.craftingDuration * amount);
            localPlayer.Work_Finished.AddListener(FinishCrafting);
        }
    }
    private void FinishCrafting()
    {
        var inventory = FindObjectOfType<InventoryManager>(true);
        foreach (var item in currentOpenedRecipe.componentItems)
        {
            var temp = new ItemRecipeInfo() { itemData = item.itemData, stacks = item.stacks * amount };
            inventory.RemoveItem(temp);
        }
        FindObjectOfType<GameManager>().ChangeResources(-currentOpenedRecipe.resourceCost * amount);
        var tempItem = new ItemRecipeInfo() { itemData = currentOpenedRecipe.resultItem.itemData, stacks = currentOpenedRecipe.resultItem.stacks * amount };
        inventory.AddItem(tempItem);
        if (!openedInStructure)
            GetComponentInParent<ManualScreen>().UpdateCurrentCategory();
        UpdateCurrentDetails();
        FindObjectOfType<AudioManager>().ItemCrafted(localPlayer.transform.position);
        localPlayer.GetComponent<PlayerCharacter>().AddXp(currentOpenedRecipe.xpGranted * amount);
        localPlayer.Work_Finished.RemoveListener(FinishCrafting);
        blockingUI.SetActive(false);
    }

    public void SetLocalPlayerCharacter(PlayerCharacter player)
    {
        localPlayer = player.GetComponent<PlayerController>();
    }
    public void ChangeAmount(bool increase)
    {
        if (increase)
            ChangeAmount(amount + 1);
        else
            ChangeAmount(amount - 1);
    }
    public void ChangeAmount(int value)
    {
        amount = value;
        if (amount < 1)
            amount = 1;
        if (amount > 99)
            amount = 99;
        Debug.Log(amount);
        UpdateDetails(currentOpenedRecipe, openedInStructure, amount);
    }
    public void ChangeAmount(string value)
    {
        ChangeAmount(int.Parse(value));
    }
}
