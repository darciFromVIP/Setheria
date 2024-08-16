using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;

public class EntityStatusBar : MonoBehaviour
{
    [SerializeField] private Slider healthBar, manaBar, hpAnimationFill, mpAnimationFill;
    [SerializeField] private LayoutElement corruptedHp, corruptedMp;
    [SerializeField] private GameObject levelBar;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Color takeDamageColor = Color.red, healDamageColor = Color.green;
    private float healthLerpTimer = 0, manaLerpTimer = 0;
    private float currentHealth, currentMana;
    private float chipTimer = 2;
    private void Start()
    {
        Initialize();
    }
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
        if (healthBar.value < currentHealth && healthLerpTimer >= 0)
        {
            healthBar.value = Mathf.Lerp(healthBar.value, currentHealth, healthLerpTimer / chipTimer);
        }
        if (mpAnimationFill.value > currentMana && manaLerpTimer >= 0)
        {
            mpAnimationFill.value = Mathf.Lerp(mpAnimationFill.value, currentMana, manaLerpTimer / chipTimer);
        }
        if (manaBar.value < currentMana && manaLerpTimer >= 0)
        {
            manaBar.value = Mathf.Lerp(manaBar.value, currentMana, manaLerpTimer / chipTimer);
        }
    }
    private void Initialize()                           
    {                                                           
        var hp = GetComponentInParent<HasHealth>();
        if (hp)
        {
            if (hp.TryGetComponent(out PlayerCharacter player))
                if (player.isLoaded)
                    InitializeHpBar(hp);
                else
                    player.Character_Loaded.AddListener(InitializeHpBar);
            else
                InitializeHpBar(hp);

        }
        var mp = GetComponentInParent<HasMana>();
        if (mp)
        {
            if (mp.TryGetComponent(out PlayerCharacter player))
                if (player.isLoaded)
                    InitializeMpBar(mp);
                else
                    player.Character_Loaded.AddListener(InitializeMpBar);
            else
                InitializeMpBar(mp);
        }
        var xp = GetComponentInParent<PlayerCharacter>();
        if (xp)
        {
            if (xp.isLoaded)
                InitializeXpBar(xp);
            else
                xp.Character_Loaded.AddListener(InitializeXpBar);
        }
        var enemy = GetComponentInParent<EnemyCharacter>();
        if (enemy)
        {
            InitializeLvlBar(enemy);
        }
    }
    private void InitializeHpBar(PlayerCharacter player)
    {
        InitializeHpBar(player.GetComponent<HasHealth>());
    }
    private void InitializeHpBar(HasHealth hp)
    {
        healthBar.gameObject.SetActive(true);
        healthBar.maxValue = hp.GetBaseMaxHealth();
        healthBar.value = hp.GetHealth();
        hp.Health_Changed.AddListener(ChangeHealthStatus);
        hp.Corrupted_Health_Changed.AddListener(ChangeCorruptedHealthStatus);
    }
    private void InitializeMpBar(PlayerCharacter player)
    {
        InitializeMpBar(player.GetComponent<HasMana>());
    }
    private void InitializeMpBar(HasMana mp)
    {
        manaBar.gameObject.SetActive(true);
        manaBar.maxValue = mp.GetBaseMaxMana();
        manaBar.value = mp.GetMana();
        mp.Mana_Changed.AddListener(ChangeManaStatus);
        mp.Corrupted_Mana_Changed.AddListener(ChangeCorruptedManaStatus);
    }
    private void ChangeHealthStatus(float currentHealth, float maxHealth)
    {
        healthLerpTimer = 0;
        healthBar.maxValue = maxHealth;
        hpAnimationFill.maxValue = maxHealth;
        this.currentHealth = currentHealth;
        if (healthBar.value - currentHealth < 0)
        {
            if (hpAnimationFill.value > currentHealth)
                healthBar.value = currentHealth;
            else
            {
                hpAnimationFill.value = currentHealth;
                hpAnimationFill.fillRect.GetComponent<Image>().color = healDamageColor;
            }
        }
        else if (healthBar.value - currentHealth > 0)
        {
            if (healthBar.value < currentHealth)
                hpAnimationFill.value = currentHealth;
            else
            {
                healthBar.value = currentHealth;
                hpAnimationFill.fillRect.GetComponent<Image>().color = takeDamageColor;
            }
        }
        else
        {
            healthBar.value = currentHealth;
            hpAnimationFill.value = currentHealth;
        }
    }
    private void ChangeCorruptedHealthStatus(float maxHealth, float corruptedHealth)
    {
        float percentage = corruptedHealth / maxHealth;
        var maxWidth = healthBar.GetComponent<LayoutElement>().flexibleWidth;
        corruptedHp.preferredWidth = maxWidth * percentage;
    }
    private void ChangeManaStatus(float currentMana, float currentMaxMana)
    {
        manaLerpTimer = 0f;
        manaBar.maxValue = currentMaxMana;
        mpAnimationFill.maxValue = currentMaxMana;
        this.currentMana = currentMana;
        if (manaBar.value - currentMana < 0)
        {
            if (mpAnimationFill.value > currentMana)
                manaBar.value = currentMana;
            else
            {
                mpAnimationFill.value = currentMana;
                mpAnimationFill.fillRect.GetComponent<Image>().color = healDamageColor;
            }
        }
        else if (manaBar.value - currentMana > 0)
        {
            if (manaBar.value < currentMana)
                mpAnimationFill.value = currentMana;
            else
            {
                manaBar.value = currentMana;
                mpAnimationFill.fillRect.GetComponent<Image>().color = takeDamageColor;
            }
        }
        else
        {
            manaBar.value = currentMana;
            mpAnimationFill.value = currentMana;
        }
    }
    private void ChangeCorruptedManaStatus(float maxMana, float corruptedMana)
    {
        float percentage = corruptedMana / maxMana;
        var maxWidth = manaBar.GetComponent<LayoutElement>().flexibleWidth;
        corruptedMp.preferredWidth = maxWidth * percentage;
    }
    private void InitializeXpBar(PlayerCharacter player)
    {
        Debug.Log("Character Loaded Event Xp");
        levelBar.gameObject.SetActive(true);
        levelText.text = player.level.ToString();
        player.Level_Up.AddListener(ChangeLevelStatus);
    }
    private void InitializeLvlBar(EnemyCharacter enemy)
    {
        levelBar.gameObject.SetActive(true);
        levelText.text = enemy.level.ToString();
    }
    private void ChangeLevelStatus(int currentLevel)
    {
        levelText.text = currentLevel.ToString();
    }
}
