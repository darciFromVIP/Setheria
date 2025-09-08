using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProfessionWindow : MonoBehaviour
{
    public TalentTreeType talentTreeType;
    public TextMeshProUGUI experienceText;
    public GameObject lockedImage;
    private void OnEnable()
    {
        var talentScreen = GetComponentInParent<TalentScreen>();
        talentScreen.ChangeCurrentOpenedTalentTree(talentTreeType);
        switch (talentTreeType)
        {
            case TalentTreeType.Combat:
                break;
            case TalentTreeType.Gathering:
                if (talentScreen.localPlayer.professions.gathering == 0)
                    lockedImage.SetActive(true);
                else
                    lockedImage.SetActive(false);
                experienceText.text = talentScreen.localPlayer.professions.gathering.ToString() + "/" + talentScreen.localPlayer.professions.gatheringMilestone;
                break;
            case TalentTreeType.Cooking:
                if (talentScreen.localPlayer.professions.cooking == 0)
                    lockedImage.SetActive(true);
                else
                    lockedImage.SetActive(false);
                experienceText.text = talentScreen.localPlayer.professions.cooking.ToString() + "/" + talentScreen.localPlayer.professions.cookingMilestone;
                break;
            case TalentTreeType.Alchemy:
                if (talentScreen.localPlayer.professions.alchemy == 0)
                    lockedImage.SetActive(true);
                else
                    lockedImage.SetActive(false);
                experienceText.text = talentScreen.localPlayer.professions.alchemy.ToString() + "/" + talentScreen.localPlayer.professions.alchemyMilestone;
                break;
            case TalentTreeType.Fishing:
                if (talentScreen.localPlayer.professions.fishing == 0)
                    lockedImage.SetActive(true);
                else
                    lockedImage.SetActive(false);
                experienceText.text = talentScreen.localPlayer.professions.fishing.ToString() + "/" + talentScreen.localPlayer.professions.fishingMilestone;
                break;
            case TalentTreeType.Exploration:
                if (talentScreen.localPlayer.professions.exploration == 0)
                    lockedImage.SetActive(true);
                else
                    lockedImage.SetActive(false);
                experienceText.text = talentScreen.localPlayer.professions.exploration.ToString() + "/" + talentScreen.localPlayer.professions.explorationMilestone;
                break;
            default:
                break;
        }
    }
}
