using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Actions/Use with Tool")]
public class AUseWithTool : ActionTemplate
{
    public ItemType toolType;
    public int requiredLevel;
    public List<ItemDropChances> itemDropsTable;
    public override void ActionFinished()
    {
        
    }

    public override void Execute()
    {
        int random;
        int temp;
        foreach (var item in itemDropsTable)
        {
            random = Random.Range(0, 100);
            temp = 0;
            foreach (var item2 in item.itemDropChances)
            {
                temp += item2.chance;
                if (random >= temp - item2.chance && random < temp)
                {
                    var player = FindObjectOfType<GameManager>().localPlayerCharacter;
                    FindObjectOfType<InventoryManager>().AddItem(item2.item);
                    Action_Finished.Invoke();
                }
            }
        }
    }

    public override bool TestExecute()
    {
        return FindObjectOfType<CharacterScreen>(true).CheckToolLevel(toolType, requiredLevel);
    }
}
