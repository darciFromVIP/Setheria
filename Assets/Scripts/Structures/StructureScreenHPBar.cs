using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class StructureScreenHPBar : MonoBehaviour
{
    public Slider hpBar;
    public TextMeshProUGUI hpText;
    private HasHealth currentStructure;
    public void AddListenerToHpEvents(HasHealth hp)
    {
        if (currentStructure)
            currentStructure.Health_Changed.RemoveListener(UpdateHP);
        currentStructure = hp;
        currentStructure.Health_Changed.AddListener(UpdateHP);
        UpdateHP(hp.GetHealth(), hp.GetFinalMaxHealth());
    }
    private void UpdateHP(float currentHP, float maxHP)
    {
        hpBar.maxValue = maxHP;
        hpBar.value = currentHP;
        hpText.text = (int)currentHP + "/" + (int)maxHP;
    }
}
