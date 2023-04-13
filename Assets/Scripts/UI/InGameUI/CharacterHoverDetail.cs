using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CharacterHoverDetail : MonoBehaviour
{
    private Character lastUpdatedCharacter;
    private bool targetedCharacter = false;

    public GameObject window;
    public TextMeshProUGUI characterName, hpPercentage, characterLevel, hpText, mpText;
    public Slider hpSlider, mpSlider;
    public BuffListTarget buffList;

    public void Show(Character character, bool targetedCharacter)
    {
        if (this.targetedCharacter == false)
            this.targetedCharacter = targetedCharacter;
        if (lastUpdatedCharacter && !this.targetedCharacter)
        {
            UnsubscribeFromCharacterEvents();
        }
        window.SetActive(true);
        if (this.targetedCharacter == targetedCharacter)
        {
            lastUpdatedCharacter = character;
            UpdateDetail();
        }
    }
    public void Hide(bool disableTargeting)
    {
        if (disableTargeting)
            targetedCharacter = false;
        if (targetedCharacter)
            return;
        if (lastUpdatedCharacter)
        {
            UnsubscribeFromCharacterEvents();
        }
        window.SetActive(false);
    }
    private void UnsubscribeFromCharacterEvents()
    {
        if (lastUpdatedCharacter.TryGetComponent(out HasHealth hp))
            hp.Health_Changed.RemoveListener(UpdateHealth);
        if (lastUpdatedCharacter.TryGetComponent(out HasMana mp))
            mp.Mana_Changed.RemoveListener(UpdateMana);
        lastUpdatedCharacter.Buff_Added.RemoveListener(AddBuffToList);
    }
    public void UpdateDetail()
    {
        int nameIndex = lastUpdatedCharacter.name.IndexOf("(");
        if (nameIndex != -1)
            characterName.text = lastUpdatedCharacter.name.Substring(0, nameIndex);
        else
            characterName.text = lastUpdatedCharacter.name;
        var characterHpComp = lastUpdatedCharacter.GetComponent<HasHealth>();
        var characterMpComp = lastUpdatedCharacter.GetComponent<HasMana>();
        if (characterHpComp)
        {
            hpPercentage.text = (characterHpComp.GetHealth() / characterHpComp.GetFinalMaxHealth() * 100).ToString("F0") + "%";
            hpText.text = characterHpComp.GetHealth().ToString("F0") + "/" + characterHpComp.GetFinalMaxHealth().ToString("F0");
            hpSlider.maxValue = characterHpComp.GetFinalMaxHealth();
            hpSlider.value = characterHpComp.GetHealth();
            characterHpComp.Health_Changed.AddListener(UpdateHealth);
        }
        if (characterMpComp)
        {
            mpText.text = characterMpComp.GetMana().ToString("F0") + "/" + characterMpComp.GetFinalMaxMana().ToString("F0");
            mpSlider.maxValue = characterMpComp.GetFinalMaxMana();
            mpSlider.value = characterMpComp.GetMana();
            characterMpComp.Mana_Changed.AddListener(UpdateMana);
        }
        characterLevel.text = "Lv" + lastUpdatedCharacter.level.ToString();
        lastUpdatedCharacter.Buff_Added.RemoveListener(AddBuffToList);
        lastUpdatedCharacter.Buff_Added.AddListener(AddBuffToList);
        foreach (var item in buffList.GetComponentsInChildren<BuffUI>())
        {
            Destroy(item.gameObject);
        }
        foreach (var item in lastUpdatedCharacter.buffs)
        {
            buffList.AddBuff(item.name, item);
        }
    }
    private void AddBuffToList(string buffName, Buff buffInstance)
    {
        buffList.AddBuff(buffName, buffInstance);
    }
    private void UpdateHealth(float hp, float maxHp)
    {
        if (hp <= 0)
        {
            UnsubscribeFromCharacterEvents();
            Hide(true);
            return;
        }
        hpPercentage.text = (hp / maxHp * 100).ToString("F0") + "%";
        hpText.text = hp.ToString("F0") + "/" + maxHp.ToString("F0");
        hpSlider.maxValue = maxHp;
        hpSlider.value = hp;
    }
    private void UpdateMana(float mp, float maxMp)
    {
        mpText.text = mp.ToString("F0") + "/" + maxMp.ToString("F0");
        mpSlider.maxValue = maxMp;
        mpSlider.value = mp;
    }
}
