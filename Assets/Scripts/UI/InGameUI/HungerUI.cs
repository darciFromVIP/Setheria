using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class HungerUI : MonoBehaviour, NeedsLocalPlayerCharacter
{
    private PlayerCharacter player;
    public TextMeshProUGUI hungerText;
    public void SetLocalPlayerCharacter(PlayerCharacter player)
    {
        this.player = player;
        player.Hunger_Changed.AddListener(UpdateUI);
        UpdateUI();
    }

    private void UpdateUI()
    {
        Color color;
        if (player.hunger > 50)
            color = Color.green;
        else if (player.hunger > 20)
            color = Color.yellow;
        else 
            color = Color.red;
        hungerText.color = color;
        hungerText.text = "<sprite=12>" + player.hunger + "/" + player.maxHunger;
        GetComponent<TooltipTrigger>().SetText("Food", "Displays your Hero satiety. If it falls to 0, your hero will start starving to death. You lose 1 Food every " + player.GetHungerInterval() + " seconds.");
    }
}
