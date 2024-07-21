using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Video;
public class RecipeDetail : MonoBehaviour, NeedsLocalPlayerCharacter
{
    public InventoryItem inventoryItemPrefab;
    public TextMeshProUGUI resultItemNameText, resourceCostText, structureRequirement, professionRequirement;
    public Transform resultItemParent, components;
    public Button craftBtn, craftAllBtn;
    public GameObject blockingUI;
    public TMP_InputField amountInput;

    private int amount = 1;

    private PlayerController localPlayer;

    private List<Transform> componentParents = new();
    public List<ItemRecipeInfo> currentPlayerItems;
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
        var player = localPlayer.GetComponent<PlayerCharacter>();
        currentPlayerItems = GetAllAvailableItems();
        this.openedInStructure = openedInStructure;
        bool craftableInStructure = recipeData.unlocked && (recipeData.requiredStructure == null ? !openedInStructure : openedInStructure);
        currentOpenedRecipe = recipeData;
        resultItemNameText.text = recipeData.resultItem.itemData.name;
        structureRequirement.text = "Structure: " + (recipeData.requiredStructure == null ? "None" : recipeData.requiredStructure.name);
        if (recipeData.requiredProfession == TalentTreeType.Special)
            professionRequirement.text = "";
        else
        {
            if (player.professions.GetProfessionExperience(recipeData.requiredProfession) >= recipeData.requiredProfessionExperience)
                professionRequirement.text = recipeData.requiredProfession.ToString() + ": <color=green>" + player.professions.GetProfessionExperience(recipeData.requiredProfession) + "/" + recipeData.requiredProfessionExperience;
            else
                professionRequirement.text = recipeData.requiredProfession.ToString() + ": <color=red>" + player.professions.GetProfessionExperience(recipeData.requiredProfession) + "/" + recipeData.requiredProfessionExperience;
        }
        bool hasRequiredProfession = player.professions.GetProfessionExperience(recipeData.requiredProfession) >= recipeData.requiredProfessionExperience;
        craftBtn.interactable = craftableInStructure && hasRequiredProfession;

        var resultItem = Instantiate(inventoryItemPrefab, resultItemParent);
        resultItem.stackText.text = (recipeData.resultItem.stacks * this.amount).ToString();
        resultItem.InitializeItem(recipeData.resultItem.itemData, recipeData.resultItem.stacks * this.amount, false, false);
        
        for (int i = 0; i < recipeData.componentItems.Count; i++)
        {
            foreach (var item in currentPlayerItems)
            {
                if (item.itemData == recipeData.componentItems[i].itemData)
                {
                    var newItem = Instantiate(inventoryItemPrefab, componentParents[i].transform);
                    newItem.InitializeItem(recipeData.componentItems[i].itemData, item.stacks , recipeData.componentItems[i].stacks * this.amount);
                    if (item.stacks < recipeData.componentItems[i].stacks * this.amount)
                        craftBtn.interactable = false;
                    break;
                }
            }
            if (componentParents[i].transform.childCount == 0)
            {
                var newItem = Instantiate(inventoryItemPrefab, componentParents[i].transform);
                newItem.InitializeItem(recipeData.componentItems[i].itemData, 0, recipeData.componentItems[i].stacks * this.amount);
                craftBtn.interactable = false;
            }
        }
        resourceCostText.text = "Resources: " + (recipeData.resourceCost * this.amount).ToString();
        if (!FindObjectOfType<GameManager>().TestSubtractResources(recipeData.resourceCost * this.amount))
        {
            resourceCostText.text = "Resources: <color=red>" + (recipeData.resourceCost * this.amount).ToString();
            craftBtn.interactable = false;
        }

        craftAllBtn.interactable = craftBtn.interactable;
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
        craftAllBtn.interactable = false;
        currentPlayerItems.Clear();
    }
    public void UpdateCurrentDetails()
    {
        if (currentOpenedRecipe)
            UpdateDetails(currentOpenedRecipe, openedInStructure, amount);
    }

    public void CraftCurrentItem()
    {
        if (localPlayer.state == PlayerState.None)
        {
            blockingUI.SetActive(true);
            localPlayer.CmdStartWorking(currentOpenedRecipe.craftingDuration * amount);
            localPlayer.Work_Finished.AddListener(FinishCrafting);
            localPlayer.Work_Cancelled.AddListener(CraftingCancelled);
        }
    }
    public void CraftAllItems()
    {
        if (localPlayer.state == PlayerState.None)
        {
            int tempAmount = 99;
            for (int i = 0; i < currentOpenedRecipe.componentItems.Count; i++)
            {
                foreach (var item in currentPlayerItems)
                {
                    if (item.itemData == currentOpenedRecipe.componentItems[i].itemData)
                    {
                        int temp = item.stacks / currentOpenedRecipe.componentItems[i].stacks;
                        if (temp < tempAmount)
                            tempAmount = temp;
                    }
                }
            }
            amount = tempAmount;
            if (amount > 0)
            {
                blockingUI.SetActive(true);
                localPlayer.CmdStartWorking(currentOpenedRecipe.craftingDuration * amount);
                localPlayer.Work_Finished.AddListener(FinishCrafting);
                localPlayer.Work_Cancelled.AddListener(CraftingCancelled);
            }
        }
    }
    private void CraftingCancelled()
    {
        blockingUI.SetActive(false);
    }
    private void FinishCrafting()
    {
        var inventory = FindObjectOfType<InventoryManager>(true);
        var stash = FindObjectOfType<StashInventory>(true);
        var canUseStash = localPlayer.GetComponent<PlayerCharacter>().IsTentNearby();
        foreach (var item in currentOpenedRecipe.componentItems)
        {
            var temp = new ItemRecipeInfo() { itemData = item.itemData, stacks = item.stacks * amount };
            int totalStacks = item.stacks * amount;
            var inventoryItem = inventory.GetItemOfName(temp.itemData.name);
            if (inventoryItem != null)
                if (inventoryItem.stacks >= totalStacks)
                    inventory.RemoveItem(temp);
                else if (canUseStash)
                {
                    var stashItem = stash.GetItemOfName(temp.itemData.name);
                    if (stashItem != null)
                        if (stashItem.stacks >= totalStacks)
                            stash.RemoveItem(temp);
                        else if (inventoryItem.stacks + stashItem.stacks >= totalStacks)
                        {
                            temp.stacks = inventoryItem.stacks;
                            var afterStacks = totalStacks - inventoryItem.stacks;
                            inventory.RemoveItem(temp);
                            temp.stacks = afterStacks;
                            stash.RemoveItem(temp);
                        }
                        else
                        {
                            SendComponentMissingMessage();
                            return;
                        }
                    else
                    {
                        SendComponentMissingMessage();
                        return;
                    }
                }
                else
                {
                    SendComponentMissingMessage();
                    return;
                }
            else if (canUseStash)
            {
                var stashItem = stash.GetItemOfName(temp.itemData.name);
                if (stashItem != null)
                    if (stashItem.stacks >= totalStacks)
                        stash.RemoveItem(temp);
                    else
                    {
                        SendComponentMissingMessage();
                        return;
                    }
                else
                {
                    SendComponentMissingMessage();
                    return;
                }
            }
            else
            {
                SendComponentMissingMessage();
                return;
            }
        }
        FindObjectOfType<GameManager>().ChangeResources(-currentOpenedRecipe.resourceCost * amount);
        var tempItem = new ItemRecipeInfo() { itemData = currentOpenedRecipe.resultItem.itemData, stacks = currentOpenedRecipe.resultItem.stacks * amount };
        if (tempItem.itemData.stackable)
            inventory.AddItem(tempItem);
        else
        {
            tempItem.stacks = 1;
            for (int i = 0; i < currentOpenedRecipe.resultItem.stacks * amount; i++)
            {
                inventory.AddItem(tempItem);
            }
        }
        if (!openedInStructure)
            GetComponentInParent<ManualScreen>().UpdateCurrentCategory();
        FindObjectOfType<AudioManager>().ItemCrafted(localPlayer.transform.position);
        localPlayer.GetComponent<PlayerCharacter>().CmdAddXp(currentOpenedRecipe.xpGranted * amount);
        localPlayer.GetComponent<PlayerCharacter>().professions.AddAnyProfession(currentOpenedRecipe.requiredProfession, amount);
        localPlayer.Work_Finished.RemoveListener(FinishCrafting);
        UpdateCurrentDetails();
        blockingUI.SetActive(false);
    }
    private void SendComponentMissingMessage()
    {
        FindObjectOfType<SystemMessages>().AddMessage("A component is missing.");
        localPlayer.Work_Finished.RemoveListener(FinishCrafting);
        UpdateCurrentDetails();
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
        if (currentOpenedRecipe == null)
            return;
        amount = value;
        if (amount < 1)
            amount = 1;
        if (amount > 99)
            amount = 99;
        UpdateDetails(currentOpenedRecipe, openedInStructure, amount);
    }
    private List<ItemRecipeInfo> GetAllAvailableItems()
    {
        List<ItemRecipeInfo> currentPlayerItems = new();
        var player = localPlayer.GetComponent<PlayerCharacter>();
        foreach (var item in FindObjectOfType<InventoryManager>(true).GetAllItems())
        {
            var temp = currentPlayerItems.Find(x => x.itemData == item.item);
            if (temp != null)
                temp.stacks += item.stacks;
            else
                currentPlayerItems.Add(new ItemRecipeInfo { itemData = item.item, stacks = item.stacks });
        }
        if (player.IsTentNearby())
        {
            List<ItemRecipeInfo> stashInventory = new();
            foreach (var item in FindObjectOfType<StashInventory>(true).GetAllItems())
            {
                var temp = stashInventory.Find(x => x.itemData == item.item);
                if (temp != null)
                    temp.stacks += item.stacks;
                else
                    stashInventory.Add(new ItemRecipeInfo { itemData = item.item, stacks = item.stacks });
            }
            foreach (var item in stashInventory)
            {
                var temp = currentPlayerItems.Find(x => x.itemData == item.itemData);
                if (temp != null)
                    temp.stacks += item.stacks;
                else
                    currentPlayerItems.Add(item);
            }
        }
        return currentPlayerItems;
    }
    public void ChangeAmount(string value)
    {
        ChangeAmount(int.Parse(value));
    }
}
