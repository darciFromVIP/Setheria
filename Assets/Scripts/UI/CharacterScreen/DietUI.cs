using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DietUI : MonoBehaviour, NeedsLocalPlayerCharacter
{
    public LayoutElement entireRect;
    public RectTransform line;
    public TextMeshProUGUI carnivoreText, herbivoreText, dietText;
    private float dietPercentage = 50;
    public void SetLocalPlayerCharacter(PlayerCharacter player)
    {
        player.DietPercentageChanged.AddListener(UpdateUI);
    }

    private void UpdateUI(float percentage)
    {
        var maxWidth = entireRect.flexibleWidth;
        line.localPosition = new Vector3((maxWidth * ((float)percentage / 100)) - 2.2f, line.localPosition.y, line.localPosition.z);
        if (line.localPosition.x < 2.2f)
            line.localPosition = new Vector3(2.2f, line.localPosition.y, line.localPosition.z);
        else if (line.localPosition.x > 553)
            line.localPosition = new Vector3(553, line.localPosition.y, line.localPosition.z);
        carnivoreText.text = percentage.ToString() + "%";
        herbivoreText.text = (100 - percentage).ToString() + "%";
        if (percentage >= 70 && dietPercentage < 70)
        {
            FindObjectOfType<SystemMessages>().AddMessage("Diet changed to Carnivore.", MsgType.Notice);
            dietText.text = "Carnivore";
        }
        else if (percentage <= 30 && dietPercentage > 30)
        {
            FindObjectOfType<SystemMessages>().AddMessage("Diet changed to Herbivore.", MsgType.Notice);
            dietText.text = "Herbivore";
        }
        else if (percentage > 30 && percentage < 70 && (dietPercentage <= 30 || dietPercentage >= 70))
        {
            FindObjectOfType<SystemMessages>().AddMessage("Diet changed to Omnivore.", MsgType.Notice);
            dietText.text = "Omnivore";
        }
        dietPercentage = percentage;
    }
}
