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
        hungerText.text = player.hunger + "/" + player.maxHunger;
    }
}
