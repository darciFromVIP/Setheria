using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class HeroDescription : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI xpText;

    public void UpdateDescription(SaveDataPlayer playerData)
    {
        gameObject.SetActive(true);
        nameText.text = playerData.name;
        levelText.text = playerData.level.ToString();
        xpText.text = playerData.xp.ToString() + "/" + playerData.maxXp;
    }
}
