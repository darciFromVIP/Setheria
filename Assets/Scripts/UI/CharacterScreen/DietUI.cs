using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DietUI : MonoBehaviour, NeedsLocalPlayerCharacter
{
    private PlayerCharacter localPlayer;
    public LayoutElement entireRect;
    public RectTransform line;
    public TextMeshProUGUI carnivoreText, herbivoreText, dietText;
    public GameObject carnivoreTalents, herbivoreTalents, omnivoreTalents;
    public void SetLocalPlayerCharacter(PlayerCharacter player)
    {
        localPlayer = player;
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
        if (percentage >= 70 && !carnivoreTalents.activeSelf)
        {
            carnivoreTalents.SetActive(true);
            herbivoreTalents.SetActive(false);
            omnivoreTalents.SetActive(false);
            localPlayer.talentTrees.RefundTalentPoints(TalentTreeType.Diet, localPlayer);
            FindObjectOfType<SystemMessages>().AddMessage("Diet changed to Carnivore.", MsgType.Notice);
            dietText.text = "Carnivore";
        }
        else if (percentage <= 30 && !herbivoreTalents.activeSelf)
        {
            carnivoreTalents.SetActive(false);
            herbivoreTalents.SetActive(true);
            omnivoreTalents.SetActive(false);
            localPlayer.talentTrees.RefundTalentPoints(TalentTreeType.Diet, localPlayer);
            FindObjectOfType<SystemMessages>().AddMessage("Diet changed to Herbivore.", MsgType.Notice);
            dietText.text = "Herbivore";
        }
        else if (percentage > 30 && percentage < 70 && !omnivoreTalents.activeSelf)
        {
            carnivoreTalents.SetActive(false);
            herbivoreTalents.SetActive(false);
            omnivoreTalents.SetActive(true);
            localPlayer.talentTrees.RefundTalentPoints(TalentTreeType.Diet, localPlayer);
            FindObjectOfType<SystemMessages>().AddMessage("Diet changed to Omnivore.", MsgType.Notice);
            dietText.text = "Omnivore";
        }
    }
}
