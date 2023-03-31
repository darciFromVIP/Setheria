using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class StructureScreenHPBar : MonoBehaviour
{
    public Slider hpBar;
    public TextMeshProUGUI hpText;
    public void AddListenerToHpEvents(HasHealth hp)
    {
        hp.Health_Changed.AddListener(UpdateHP);
        UpdateHP(hp.GetHealth(), hp.GetBaseMaxHealth());
    }
    private void UpdateHP(float currentHP, float maxHP)
    {
        hpBar.maxValue = maxHP;
        hpBar.value = currentHP;
        hpText.text = (int)currentHP + "/" + (int)maxHP;
    }
}
