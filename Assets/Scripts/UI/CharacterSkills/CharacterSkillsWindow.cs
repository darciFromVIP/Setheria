using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class CharacterSkillsWindow : MonoBehaviour, NeedsLocalPlayerCharacter
{
    public TextMeshProUGUI hpText, mpText, levelText;
    public Slider hpSlider, mpSlider, xpSlider;
    public Slider cdASlider, cdDSlider, cdQSlider, cdWSlider, cdESlider, cdRSlider;
    public TextMeshProUGUI textCdA, textCdD, textCdQ, textCdW, textCdE, textCdR;
    public List<Image> skills = new();
    public Sprite lockedSkill;
    public AvailablePointsBTN attributePointsBTN, talentPointsBTN;

    private PlayerController playerController;

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
        player.GetComponent<HasMana>().Mana_Changed.AddListener(UpdateMana);
        player.Skills_Changed.AddListener(UpdateSkills);
        UpdateSkills(player.skills);
        playerController.GetComponent<CanAttack>().Has_Attacked.AddListener(StartCooldownA);
        playerController.Cooldown_D_Started.AddListener(StartCooldownD);
        playerController.Cooldown_Q_Started.AddListener(StartCooldownQ);
        playerController.Cooldown_W_Started.AddListener(StartCooldownW);
        playerController.Cooldown_E_Started.AddListener(StartCooldownE);
        playerController.Cooldown_R_Started.AddListener(StartCooldownR);
        attributePointsBTN.Initialize(player);
        talentPointsBTN.Initialize(player);
    }
    private void UpdateHealth(float currentHealth, float maxHealth)
    {
        hpText.text = (int)currentHealth + "/" + (int)maxHealth;
        hpSlider.maxValue = maxHealth;
        hpSlider.value = currentHealth;
    }
    private void UpdateMana(float currentMana, float maxMana)
    {
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
        cdDSlider.maxValue = playerController.cooldownD;
        while (playerController.cooldownD > 0)
        {
            cdDSlider.value = playerController.cooldownD;
            if (playerController.cooldownD >= 1)
                textCdD.text = ((int)playerController.cooldownD).ToString();
            else
                textCdD.text = playerController.cooldownD.ToString("F1");
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
        cdQSlider.maxValue = playerController.cooldownQ;
        while (playerController.cooldownQ > 0)
        {
            cdQSlider.value = playerController.cooldownQ;
            if (playerController.cooldownQ >= 1)
                textCdQ.text = ((int)playerController.cooldownQ).ToString();
            else
                textCdQ.text = playerController.cooldownQ.ToString("F1"); 
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
        cdWSlider.maxValue = playerController.cooldownW;
        while (playerController.cooldownW > 0)
        {
            cdWSlider.value = playerController.cooldownW;
            if (playerController.cooldownW >= 1)
                textCdW.text = ((int)playerController.cooldownW).ToString();
            else
                textCdW.text = playerController.cooldownW.ToString("F1"); 
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
        cdESlider.maxValue = playerController.cooldownE;
        while (playerController.cooldownE > 0)
        {
            cdESlider.value = playerController.cooldownE;
            if (playerController.cooldownE >= 1)
                textCdE.text = ((int)playerController.cooldownE).ToString();
            else
                textCdE.text = playerController.cooldownE.ToString("F1"); 
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
        cdRSlider.maxValue = playerController.cooldownR;
        while (playerController.cooldownR > 0)
        {
            cdRSlider.value = playerController.cooldownR;
            if (playerController.cooldownR >= 1)
                textCdR.text = ((int)playerController.cooldownR).ToString();
            else
                textCdR.text = playerController.cooldownR.ToString("F1"); 
            yield return null;
        }
        cdRSlider.gameObject.SetActive(false);
    }
    public void CastD()
    {
        playerController.CmdExecuteSkill1();
    }
    public void CastQ()
    {
        playerController.CmdExecuteSkill2();
    }
    public void CastW()
    {
        playerController.CmdExecuteSkill3();
    }
    public void CastE()
    {
        playerController.CmdExecuteSkill4();
    }
    public void CastR()
    {
        playerController.CmdExecuteSkill5();
    }
}
