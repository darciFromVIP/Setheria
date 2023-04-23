using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class CharacterScreen : MonoBehaviour, NeedsLocalPlayerCharacter
{
    public GameObject window;

    private PlayerCharacter playerCharacter;

    public TextMeshProUGUI powerText, attackSpeedText, criticalChanceText, criticalDamageText, cooldownReductionText, healthText, healthRegenText, manaText, manaRegenText, armorText;
    public TextMeshProUGUI levelText, xpText, attributePointsText, nameText;
    public Slider xpSlider;
    public Transform modelPoint;
    public HeroModelDatabase modelDatabase;
    public List<Button> attributeButtons;

    public GameObject currentOpenedWindow;
    public void SetLocalPlayerCharacter(PlayerCharacter player)
    {
        playerCharacter = player;
        Instantiate(modelDatabase.GetModelByHeroType(player.hero), modelPoint);
        nameText.text = player.heroName;
        player.Xp_Changed.AddListener(UpdateXp);
        player.Level_Up.AddListener(UpdateLevel);
        player.Attributes_Changed.AddListener(UpdateAttributes);
        var attackComp = player.GetComponent<CanAttack>();
        attackComp.Power_Changed.AddListener(UpdatePower);
        attackComp.Attack_Speed_Changed.AddListener(UpdateAttackSpeed);
        attackComp.Critical_Chance_Changed.AddListener(UpdateCriticalChance);
        attackComp.Critical_Damage_Changed.AddListener(UpdateCriticalDamage);
        attackComp.Cooldown_Reduction_Changed.AddListener(UpdateCooldownReduction);
        var healthComp = player.GetComponent<HasHealth>();
        healthComp.Health_Changed.AddListener(UpdateHealth);
        healthComp.Health_Regen_Changed.AddListener(UpdateHealthRegen);
        healthComp.Armor_Changed.AddListener(UpdateArmor);
        var manaComp = player.GetComponent<HasMana>();
        manaComp.Mana_Changed.AddListener(UpdateMana);
        manaComp.Mana_Regen_Changed.AddListener(UpdateManaRegen);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            ToggleWindow();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
            HideWindow();
    }
    public void ToggleWindow()
    {
        FindObjectOfType<Tooltip>(true).Hide();
        window.SetActive(!window.activeSelf);
    }
    public void HideWindow()
    {
        FindObjectOfType<Tooltip>(true).Hide();
        window.SetActive(false);
    }
    private void UpdateXp(int currentXp, int maxXp)
    {
        xpText.text = currentXp.ToString() + "/" + maxXp.ToString();
        xpSlider.maxValue = maxXp;
        xpSlider.value = currentXp;
    }
    private void UpdateLevel(int level)
    {
        levelText.text = level.ToString();
    }
    private void UpdatePower(float value)
    {
        powerText.text = ((int)value).ToString();
    }
    private void UpdateAttackSpeed(float value)
    {
        attackSpeedText.text = value.ToString("F2");
    }
    private void UpdateCriticalChance(float value)
    {
        criticalChanceText.text = value.ToString("F2") + "%";
    }
    private void UpdateCriticalDamage(float value)
    {
        criticalDamageText.text = value.ToString("F2") + "%";
    }
    private void UpdateCooldownReduction(float value)
    {
        cooldownReductionText.text = value.ToString("F2") + "%";
    }
    private void UpdateHealth(float currentHealth, float maxHealth)
    {
        healthText.text = ((int)maxHealth).ToString();
    }
    private void UpdateHealthRegen(float value)
    {
        healthRegenText.text = value.ToString("F2");
    }
    private void UpdateMana(float currentMana, float maxMana)
    {
        manaText.text = ((int)maxMana).ToString();
    }
    private void UpdateManaRegen(float value)
    {
        manaRegenText.text = value.ToString("F2");
    }
    private void UpdateArmor(float value)
    {
        armorText.text = value.ToString("F2");
    }
    private void UpdateAttributes(int value)
    {
        if (value <= 0)
            foreach (var item in attributeButtons)
                item.interactable = false;
        else
            foreach (var item in attributeButtons)
                item.interactable = true;

        attributePointsText.text = value.ToString();
    }
    public void MaxHealthAtt()
    {
        playerCharacter.AddMaxHealthAttribute(1);
    }
    public void HealthRegenAtt()
    {
        playerCharacter.AddHealthRegenAttribute(1);
    }
    public void ArmorAtt()
    {
        playerCharacter.AddArmorAttribute(1);
    }
    public void MaxManaAtt()
    {
        playerCharacter.AddMaxManaAttribute(1);
    }
    public void ManaRegenAtt()
    {
        playerCharacter.AddManaRegenAttribute(1);
    }
    public void PowerAtt()
    {
        playerCharacter.AddPowerAttribute(1);
    }
    public void CritChanceAtt()
    {
        playerCharacter.AddCriticalChanceAttribute(1);
    }
    public void CritDamageAtt()
    {
        playerCharacter.AddCriticalDamageAttribute(1);
    }
    public void AttackSpeedAtt()
    {
        playerCharacter.AddAttackSpeedAttribute(1);
    }
    public void CooldownReductionAtt()
    {
        playerCharacter.AddCooldownReductionAttribute(1);
    }
    public bool CheckToolLevel(ItemType toolType, int compareTo)
    {
        if (toolType == ItemType.None)
            return true;
        foreach (var item in GetComponentsInChildren<InventoryItem>(true))
        {
            if (item.item.itemType == toolType)
            {
                return item.item.value >= compareTo;
            }
        }
        return false;
    }
    public void ReduceToolDurability(ItemType toolType, int value)
    {
        foreach (var item in GetComponentsInChildren<InventoryItem>(true))
        {
            if (item.item.itemType == toolType)
            {
                item.ChangeStacks(value);
            }
        }
    }
    public void OpenAnotherWindow(GameObject window)
    {
        if (currentOpenedWindow)
            currentOpenedWindow.SetActive(false);
        window.SetActive(true);
        currentOpenedWindow = window;
        GetComponent<TalentScreen>().UpdateTalents();
    }
}
