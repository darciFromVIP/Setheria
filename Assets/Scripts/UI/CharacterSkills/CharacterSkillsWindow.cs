using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class CharacterSkillsWindow : MonoBehaviour, NeedsLocalPlayerCharacter
{
    public TextMeshProUGUI hpText, mpText, levelText, healthRegenText, manaRegenText;
    public Slider hpSlider, mpSlider, xpSlider, hpAnimationFill, mpAnimationFill;
    public LayoutElement corruptedHp, corruptedMp;
    public Slider cdASlider, cdDSlider, cdQSlider, cdWSlider, cdESlider, cdRSlider;
    public TextMeshProUGUI textCdA, textCdD, textCdQ, textCdW, textCdE, textCdR;
    public List<Image> skills = new();
    public Sprite lockedSkill;
    public AvailablePointsBTN attributePointsBTN, talentPointsBTN;
    public Color takeDamageColor = Color.red, healDamageColor = Color.green;
    private float healthLerpTimer = 0, manaLerpTimer = 0;
    private float currentHealth, currentMana;
    private float chipTimer = 2;

    private PlayerController playerController;
    private void Update()
    {
        if (healthLerpTimer < chipTimer)
        {
            healthLerpTimer += Time.deltaTime;
        }
        if (manaLerpTimer < chipTimer)
        {
            manaLerpTimer += Time.deltaTime;
        }
        if (hpAnimationFill.value > currentHealth && healthLerpTimer >= 0)
        {
            hpAnimationFill.value = Mathf.Lerp(hpAnimationFill.value, currentHealth, healthLerpTimer / chipTimer);
        }
        if (hpSlider.value < currentHealth && healthLerpTimer >= 0)
        {
            hpSlider.value = Mathf.Lerp(hpSlider.value, currentHealth, healthLerpTimer / chipTimer);
        }
        if (mpAnimationFill.value > currentMana && manaLerpTimer >= 0)
        {
            mpAnimationFill.value = Mathf.Lerp(mpAnimationFill.value, currentMana, manaLerpTimer / chipTimer);
        }
        if (mpSlider.value < currentMana && manaLerpTimer >= 0)
        {
            mpSlider.value = Mathf.Lerp(mpSlider.value, currentMana, manaLerpTimer / chipTimer);
        }
    }
    public void HideGraphics()
    {
        foreach (var item in GetComponentsInChildren<Transform>(true))
        {
            item.gameObject.layer = 0;
        }
    }
    public void ShowGraphics()
    {
        foreach (var item in GetComponentsInChildren<Transform>(true))
        {
            item.gameObject.layer = 5;
        }
    }
    public void SetLocalPlayerCharacter(PlayerCharacter player)
    {
        playerController = player.GetComponent<PlayerController>();
        player.Xp_Changed.AddListener(UpdateXP);
        player.Level_Up.AddListener(UpdateLevel);
        player.GetComponent<HasHealth>().Health_Changed.AddListener(UpdateHealth);
        player.GetComponent<HasHealth>().Health_Regen_Changed.AddListener(UpdateHealthRegen);
        player.GetComponent<HasHealth>().Corrupted_Health_Changed.AddListener(UpdateCorruptedHealth);
        player.GetComponent<HasMana>().Mana_Changed.AddListener(UpdateMana);
        player.GetComponent<HasMana>().Mana_Regen_Changed.AddListener(UpdateManaRegen);
        player.GetComponent<HasMana>().Corrupted_Mana_Changed.AddListener(UpdateCorruptedMana);
        player.Skills_Changed.AddListener(UpdateSkills);
        UpdateSkills(player.skills);
        playerController.GetComponent<CanAttack>().Has_Attacked.AddListener(StartCooldownA);
        playerController.Cooldown_1_Started.AddListener(StartCooldownD);
        playerController.Cooldown_2_Started.AddListener(StartCooldownQ);
        playerController.Cooldown_3_Started.AddListener(StartCooldownW);
        playerController.Cooldown_4_Started.AddListener(StartCooldownE);
        playerController.Cooldown_5_Started.AddListener(StartCooldownR);
        attributePointsBTN.Initialize(player);
        talentPointsBTN.Initialize(player);
    }
    public void SetHealthMana(float health, float maxHealth, float mana, float maxMana)
    {
        hpAnimationFill.maxValue = maxHealth;
        hpAnimationFill.value = health;
        hpSlider.maxValue = maxHealth;
        hpSlider.value = health;
        mpAnimationFill.maxValue = maxMana;
        mpAnimationFill.value = mana;
        mpSlider.maxValue = maxMana;
        mpSlider.value = mana;
    }
    private void UpdateHealth(float currentHealth, float maxHealth)
    {
        healthLerpTimer = 0f;
        hpSlider.maxValue = maxHealth;
        hpAnimationFill.maxValue = maxHealth;
        this.currentHealth = currentHealth;
        hpText.text = (int)currentHealth + "/" + (int)maxHealth;
        if (hpSlider.value - currentHealth < 0)
        {
            if (hpAnimationFill.value > currentHealth)
                hpSlider.value = currentHealth;
            else
            {
                hpAnimationFill.value = currentHealth;
                hpAnimationFill.fillRect.GetComponent<Image>().color = healDamageColor;
            }
        }
        else if (hpSlider.value - currentHealth > 0)
        {
            if (hpSlider.value < currentHealth)
                hpAnimationFill.value = currentHealth;
            else
            {
                hpSlider.value = currentHealth;
                hpAnimationFill.fillRect.GetComponent<Image>().color = takeDamageColor;
            }
        }
        else
        {
            hpSlider.value = currentHealth;
            hpAnimationFill.value = currentHealth;
        }
    }
    private void UpdateHealthRegen(float regen)
    {
        if (regen > 0)
            healthRegenText.text = "+";
        else
            healthRegenText.text = "-";
        healthRegenText.text += regen;
    }
    private void UpdateCorruptedHealth(float maxHealth, float corruptedHealth)
    {
        float percentage = corruptedHealth / maxHealth;
        var maxWidth = hpSlider.GetComponent<LayoutElement>().flexibleWidth;
        corruptedHp.preferredWidth = maxWidth * percentage;
    }
    private void UpdateMana(float currentMana, float maxMana)
    {
        manaLerpTimer = 0f;
        mpSlider.maxValue = maxMana;
        mpAnimationFill.maxValue = maxMana;
        this.currentMana = currentMana;
        mpText.text = (int)currentMana + "/" + (int)maxMana;
        var player = playerController.GetComponent<PlayerCharacter>();
        for (int i = 0; i < skills.Count; i++)
        {
            if (currentMana < player.skills[i].manaCost)
                skills[i].color = new Color(0.3f, 0.3f, 0.3f);
            else
                skills[i].color = new Color(1f, 1f, 1f);
        }
        if (mpSlider.value - currentMana < 0)
        {
            if (mpAnimationFill.value > currentMana)
                mpSlider.value = currentMana;
            else
            {
                mpAnimationFill.value = currentMana;
                mpAnimationFill.fillRect.GetComponent<Image>().color = healDamageColor;
            }
        }
        else if (mpSlider.value - currentMana > 0)
        {
            if (mpSlider.value < currentMana)
                mpAnimationFill.value = currentMana;
            else
            {
                mpSlider.value = currentMana;
                mpAnimationFill.fillRect.GetComponent<Image>().color = takeDamageColor;
            }
        }
        else
        {
            mpSlider.value = currentMana;
            mpAnimationFill.value = currentMana;
        }
    }
    private void UpdateManaRegen(float regen)
    {
        if (regen > 0)
            manaRegenText.text = "+";
        else
            manaRegenText.text = "-";
        manaRegenText.text += regen;
    }
    private void UpdateCorruptedMana(float maxMana, float corruptedMana)
    {
        float percentage = corruptedMana / maxMana;
        var maxWidth = mpSlider.GetComponent<LayoutElement>().flexibleWidth;
        corruptedMp.preferredWidth = maxWidth * percentage;
    }
    private void UpdateXP(int currentXp, int maxXp)
    {
        xpSlider.maxValue = maxXp;
        xpSlider.value = currentXp;
        xpSlider.GetComponentInChildren<TooltipTrigger>().header = currentXp + "/" + maxXp;
    }
    private void UpdateLevel(int level)
    {
        levelText.text = level.ToString();
    }
    private void UpdateSkills(List<Skill> playerSkills)
    {
        for (int i = 0; i < skills.Count; i++)
        {
            if (playerSkills[i].unlocked)
            {
                skills[i].sprite = playerSkills[i].icon;
                skills[i].GetComponent<TooltipTrigger>().SetText(playerSkills[i].name, playerSkills[i].description, playerSkills[i].icon);
            }
            else
            {
                skills[i].sprite = lockedSkill;
                skills[i].GetComponent<TooltipTrigger>().SetText("Locked Skill", "Unlock this skill by selecting its corresponding talent in the Combat talent tree.", lockedSkill);
            }
            if (skills[i].TryGetComponent(out Button btn))
                btn.interactable = playerSkills[i].unlocked;
        }
    }
    private void StartCooldownA()
    {
        StartCoroutine(UpdateCooldownA());
    }
    private IEnumerator UpdateCooldownA()
    {
        var attackComp = playerController.GetComponent<CanAttack>();
        cdASlider.gameObject.SetActive(true);
        cdASlider.maxValue = attackComp.attackSpeedTimer;
        while (attackComp.attackSpeedTimer > 0)
        {
            cdASlider.value = attackComp.attackSpeedTimer;
            if (attackComp.attackSpeedTimer >= 1)
                textCdA.text = ((int)attackComp.attackSpeedTimer).ToString();
            else
                textCdA.text = attackComp.attackSpeedTimer.ToString("F1");
            yield return null;
        }
        cdASlider.gameObject.SetActive(false);
    }
    private void StartCooldownD()
    {
        StartCoroutine(UpdateCooldownD());
    }
    private IEnumerator UpdateCooldownD()
    {
        cdDSlider.gameObject.SetActive(true);
        cdDSlider.maxValue = playerController.cooldown1;
        while (playerController.cooldown1 > 0)
        {
            cdDSlider.value = playerController.cooldown1;
            if (playerController.cooldown1 >= 1)
                textCdD.text = ((int)playerController.cooldown1).ToString();
            else
                textCdD.text = playerController.cooldown1.ToString("F1");
            yield return null;
        }
        cdDSlider.gameObject.SetActive(false);
    }
    private void StartCooldownQ()
    {
        StartCoroutine(UpdateCooldownQ());
    }
    private IEnumerator UpdateCooldownQ()
    {
        cdQSlider.gameObject.SetActive(true);
        cdQSlider.maxValue = playerController.cooldown2;
        while (playerController.cooldown2 > 0)
        {
            cdQSlider.value = playerController.cooldown2;
            if (playerController.cooldown2 >= 1)
                textCdQ.text = ((int)playerController.cooldown2).ToString();
            else
                textCdQ.text = playerController.cooldown2.ToString("F1"); 
            yield return null;
        }
        cdQSlider.gameObject.SetActive(false);
    }
    private void StartCooldownW()
    {
        StartCoroutine(UpdateCooldownW());
    }
    private IEnumerator UpdateCooldownW()
    {
        cdWSlider.gameObject.SetActive(true);
        cdWSlider.maxValue = playerController.cooldown3;
        while (playerController.cooldown3 > 0)
        {
            cdWSlider.value = playerController.cooldown3;
            if (playerController.cooldown3 >= 1)
                textCdW.text = ((int)playerController.cooldown3).ToString();
            else
                textCdW.text = playerController.cooldown3.ToString("F1"); 
            yield return null;
        }
        cdWSlider.gameObject.SetActive(false);
    }
    private void StartCooldownE()
    {
        StartCoroutine(UpdateCooldownE());
    }
    private IEnumerator UpdateCooldownE()
    {
        cdESlider.gameObject.SetActive(true);
        cdESlider.maxValue = playerController.cooldown4;
        while (playerController.cooldown4 > 0)
        {
            cdESlider.value = playerController.cooldown4;
            if (playerController.cooldown4 >= 1)
                textCdE.text = ((int)playerController.cooldown4).ToString();
            else
                textCdE.text = playerController.cooldown4.ToString("F1"); 
            yield return null;
        }
        cdESlider.gameObject.SetActive(false);
    }
    private void StartCooldownR()
    {
        StartCoroutine(UpdateCooldownR());
    }
    private IEnumerator UpdateCooldownR()
    {
        cdRSlider.gameObject.SetActive(true);
        cdRSlider.maxValue = playerController.cooldown5;
        while (playerController.cooldown5 > 0)
        {
            cdRSlider.value = playerController.cooldown5;
            if (playerController.cooldown5 >= 1)
                textCdR.text = ((int)playerController.cooldown5).ToString();
            else
                textCdR.text = playerController.cooldown5.ToString("F1"); 
            yield return null;
        }
        cdRSlider.gameObject.SetActive(false);
    }
    public void CastD()
    {
        playerController.AttemptExecuteSkill1();
    }
    public void CastQ()
    {
        playerController.AttemptExecuteSkill2();
    }
    public void CastW()
    {
        playerController.AttemptExecuteSkill3();
    }
    public void CastE()
    {
        playerController.AttemptExecuteSkill4();
    }
    public void CastR()
    {
        playerController.AttemptExecuteSkill5();
    }
}
