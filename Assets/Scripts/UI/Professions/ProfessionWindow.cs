using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProfessionWindow : MonoBehaviour
{
    public TalentTreeType talentTreeType;
    public TextMeshProUGUI experienceText;
    public Button upgradeBtn;
    public GameObject lockedImage;
    private void OnEnable()
    {
        var talentScreen = GetComponentInParent<TalentScreen>();
        talentScreen.ChangeCurrentOpenedTalentTree(talentTreeType);
        upgradeBtn.interactable = talentScreen.CanUpgradeThisTree(talentTreeType);
        switch (talentTreeType)
        {
            case TalentTreeType.Special:
                break;
            case TalentTreeType.Gathering:
                if (talentScreen.localPlayer.professions.gathering == 0)
                    lockedImage.SetActive(true);
                else
                    lockedImage.SetActive(false);
                experienceText.text = talentScreen.localPlayer.professions.gathering.ToString() + "/" + talentScreen.localPlayer.professions.maxGathering;
                break;
            case TalentTreeType.Cooking:
                if (talentScreen.localPlayer.professions.cooking == 0)
                    lockedImage.SetActive(true);
                else
                    lockedImage.SetActive(false);
                experienceText.text = talentScreen.localPlayer.professions.cooking.ToString() + "/" + talentScreen.localPlayer.professions.maxCooking;
                break;
            case TalentTreeType.Alchemy:
                if (talentScreen.localPlayer.professions.alchemy == 0)
                    lockedImage.SetActive(true);
                else
                    lockedImage.SetActive(false);
                experienceText.text = talentScreen.localPlayer.professions.alchemy.ToString() + "/" + talentScreen.localPlayer.professions.maxAlchemy;
                break;
            case TalentTreeType.Fishing:
                if (talentScreen.localPlayer.professions.fishing == 0)
                    lockedImage.SetActive(true);
                else
                    lockedImage.SetActive(false);
                experienceText.text = talentScreen.localPlayer.professions.fishing.ToString() + "/" + talentScreen.localPlayer.professions.maxFishing;
                break;
            case TalentTreeType.Exploration:
                if (talentScreen.localPlayer.professions.exploration == 0)
                    lockedImage.SetActive(true);
                else
                    lockedImage.SetActive(false);
                experienceText.text = talentScreen.localPlayer.professions.exploration.ToString() + "/" + talentScreen.localPlayer.professions.maxExploration;
                break;
            default:
                break;
        }
    }
}
