using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class CharacterScreen : WindowWithCategories, NeedsLocalPlayerCharacter, WindowedUI
{
    public GameObject window;

    private PlayerCharacter playerCharacter;

    public TextMeshProUGUI powerText, attackSpeedText, criticalChanceText, criticalDamageText, cooldownReductionText, healthText, healthRegenText, manaText, manaRegenText, armorText;
    public TextMeshProUGUI levelText, xpText, attributePointsText, nameText;
    public Slider xpSlider;
    public Transform modelPoint;
    public HeroModelDatabase modelDatabase;
    public List<Button> attributeButtons;
    public CharacterSkillsWindow characterSkills;

    private SettingsManager settingsManager;
    public InputEnabledScriptable inputEnabled;

    public EventScriptable Character_Stats_Screen_Toggled;

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
    private void Start()
    {
        HideWindow();
        settingsManager = FindObjectOfType<SettingsManager>();
    }
    private void Update()
    {
        if (!inputEnabled.inputEnabled)
            return;
        if (Input.GetKeyDown(settingsManager.settings.characterScreen))
        {
            ToggleWindow();
            if (window.activeSelf)
                CheckIfStatsAreOpened();
        }
        if (Input.GetKeyDown(settingsManager.settings.manual) && window.activeSelf)
            HideWindow();
    }
    public void ToggleWindow()
    {
        FindObjectOfType<Tooltip>(true).Hide();
        window.SetActive(!window.activeSelf);
        if (window.activeSelf)
            characterSkills.HideGraphics();
        else
            characterSkills.ShowGraphics();
        GetComponentInChildren<TalentScreen>(true).UpdateTalents();
    }
    public void HideWindow()
    {
        characterSkills.ShowGraphics();
        FindObjectOfType<Tooltip>(true).Hide();
        window.SetActive(false);
    }
    public void ShowWindow()
    {
        characterSkills.HideGraphics();
        FindObjectOfType<Tooltip>(true).Hide();
        window.SetActive(true);
    }

    public bool IsActive()
    {
        return window.activeSelf;
    }
    private void UpdateXp(int currentXp, int maxXp)
    {
        xpText.text = "<sprite=14>" + currentXp.ToString() + "/" + maxXp.ToString();
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
        armorText.text = value.ToString("F2") + "%";
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
    public override void OpenAnotherWindow(GameObject window)
    {
        base.OpenAnotherWindow(window);
        GetComponentInChildren<TalentScreen>(true).UpdateTalents();
        CheckIfStatsAreOpened();
    }
    private void CheckIfStatsAreOpened()
    {
        if (currentOpenedWindow.name == "CharacterStatsWindow")
            Character_Stats_Screen_Toggled.boolEvent.Invoke(true);
        else
            Character_Stats_Screen_Toggled.boolEvent.Invoke(false);
    }
}
