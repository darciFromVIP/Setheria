using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class HydrationUI : MonoBehaviour, NeedsLocalPlayerCharacter
{
    private PlayerCharacter player;
    public TextMeshProUGUI hydrationText;
    public void SetLocalPlayerCharacter(PlayerCharacter player)
    {
        this.player = player;
        player.Water_Changed.AddListener(UpdateUI);
        UpdateUI();
    }

    private void UpdateUI()
    {
        Color color;
        if (player.water > 50)
            color = Color.green;
        else if (player.water > 20)
            color = Color.yellow;
        else 
            color = Color.red;
        hydrationText.color = color;
        hydrationText.text = "<sprite=20>" + player.water + "/" + player.maxWater;
        GetComponent<TooltipTrigger>().SetText("Hydration", "Displays your Hero satiety. If it falls to 0, your hero will start losing mana and Food levels will decrease faster. You lose 1 Water every " + player.GetWaterInterval() + " seconds.");
    }
}
