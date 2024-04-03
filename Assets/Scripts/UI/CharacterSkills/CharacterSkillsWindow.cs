using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class CharacterSkillsWindow : MonoBehaviour, NeedsLocalPlayerCharacter
{
    public TextMeshProUGUI hpText, mpText, levelText;
    public Slider hpSlider, mpSlider, xpSlider;
    public Image hpAnimationFill, mpAnimationFill;
    public LayoutElement corruptedHp, corruptedMp;
    public Slider cdASlider, cdDSlider, cdQSlider, cdWSlider, cdESlider, cdRSlider;
    public TextMeshProUGUI textCdA, textCdD, textCdQ, textCdW, textCdE, textCdR;
    public List<Image> skills = new();
    public Sprite lockedSkill;
    public AvailablePointsBTN attributePointsBTN, talentPointsBTN;
    private float healthLerpTimer = 0, manaLerpTimer = 0;
    private float healthFraction, manaFraction;
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
        if (hpAnimationFill.fillAmount != healthFraction && healthLerpTimer >= 0)
        {
            hpAnimationFill.fillAmount = Mathf.Lerp(hpAnimationFill.fillAmount, healthFraction, healthLerpTimer / chipTimer);
        }
        if (mpAnimationFill.fillAmount != manaFraction && manaLerpTimer >= 0)
        {
            mpAnimationFill.fillAmount = Mathf.Lerp(mpAnimationFill.fillAmount, manaFraction, manaLerpTimer / chipTimer);
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
        player.GetComponent<HasHealth>().Corrupted_Health_Changed.AddListener(UpdateCorruptedHealth);
        player.GetComponent<HasMana>().Mana_Changed.AddListener(UpdateMana);
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
    private void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (Mathf.Abs(hpSlider.value - currentHealth) > 5)
            healthLerpTimer = -1;
        hpText.text = (int)currentHealth + "/" + (int)maxHealth;
        hpSlider.maxValue = maxHealth;
        hpSlider.value = currentHealth;
        healthFraction = currentHealth / maxHealth;
        if (hpAnimationFill.fillAmount > healthFraction)
        {
            hpAnimationFill.color = Color.red;
        }
        else if (hpAnimationFill.fillAmount < healthFraction)
        {
            hpAnimationFill.color = Color.green;
        }
    }
    private void UpdateCorruptedHealth(float maxHealth, float corruptedHealth)
    {
        float percentage = corruptedHealth / maxHealth;
        var maxWidth = hpSlider.GetComponent<LayoutElement>().flexibleWidth;
        corruptedHp.preferredWidth = maxWidth * percentage;
    }
    private void UpdateMana(float currentMana, float maxMana)
    {
        if (Mathf.Abs(mpSlider.value - currentMana) > 5)
            manaLerpTimer = -1;
        mpText.text = (int)currentMana + "/" + (int)maxMana;
        mpSlider.maxValue = maxMana;
        mpSlider.value = currentMana;
        var player = playerController.GetComponent<PlayerCharacter>();
        for (int i = 0; i < skills.Count; i++)
        {
            if (currentMana < player.skills[i].manaCost)
                skills[i].color = new Color(0.3f, 0.3f, 0.3f);
            else 
                skills[i].color = new Color(1f, 1f, 1f);
        }
        manaFraction = currentMana / maxMana;
        if (mpAnimationFill.fillAmount > manaFraction)
        {
            mpAnimationFill.color = Color.red;
        }
        else if (mpAnimationFill.fillAmount < manaFraction)
        {
            mpAnimationFill.color = Color.green;
        }
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
