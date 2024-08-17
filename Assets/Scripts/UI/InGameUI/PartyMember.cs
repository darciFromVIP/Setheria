using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PartyMember : MonoBehaviour
{
    public EntityStatusBar statusBar;
    public TextMeshProUGUI nameTag, hungerText;

    private PlayerCharacter player;
    public void Initialize(PlayerCharacter player)
    {
        this.player = player;
        statusBar.Initialize(player);
        nameTag.text = player.nameTag.text;
        player.Hunger_Changed.AddListener(UpdateHungerUI);
        UpdateHungerUI();
    }
    private void UpdateHungerUI()
    {
        Color color;
        if (player.hunger > 50)
            color = Color.green;
        else if (player.hunger > 20)
            color = Color.yellow;
        else
            color = Color.red;
        hungerText.color = color;
        hungerText.text = player.hunger + "/" + player.maxHunger;
    }
}
