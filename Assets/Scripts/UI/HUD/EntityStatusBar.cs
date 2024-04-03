using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;

public class EntityStatusBar : MonoBehaviour
{
    [SerializeField] private Slider healthBar, manaBar;
    [SerializeField] private Image hpAnimationFill, mpAnimationFill;
    [SerializeField] private LayoutElement corruptedHp, corruptedMp;
    [SerializeField] private GameObject levelBar;
    [SerializeField] private TextMeshProUGUI levelText;
    private float healthLerpTimer = 0, manaLerpTimer = 0;
    private float healthFraction, manaFraction;
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
        if (hpAnimationFill.fillAmount != healthFraction && healthLerpTimer >= 0)
        {
            hpAnimationFill.fillAmount = Mathf.Lerp(hpAnimationFill.fillAmount, healthFraction, healthLerpTimer / chipTimer);
        }
        if (mpAnimationFill.fillAmount != manaFraction && manaLerpTimer >= 0)
        {
            mpAnimationFill.fillAmount = Mathf.Lerp(mpAnimationFill.fillAmount, manaFraction, manaLerpTimer / chipTimer);
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
        Debug.Log("Character Loaded Event Hp");
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
        Debug.Log("Character Loaded Event Mp");
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
        if (Mathf.Abs(healthBar.value - currentHealth) > 5)
            healthLerpTimer = -1;
        healthBar.value = currentHealth;
        healthBar.maxValue = maxHealth;
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
    private void ChangeCorruptedHealthStatus(float maxHealth, float corruptedHealth)
    {
        float percentage = corruptedHealth / maxHealth;
        var maxWidth = healthBar.GetComponent<LayoutElement>().flexibleWidth;
        corruptedHp.preferredWidth = maxWidth * percentage;
    }
    private void ChangeManaStatus(float currentMana, float currentMaxMana)
    {
        if (Mathf.Abs(manaBar.value - currentMana) > 5)
            manaLerpTimer = -1;
        manaBar.value = currentMana;
        manaBar.maxValue = currentMaxMana;
        manaFraction = currentMana / currentMaxMana;
        if (mpAnimationFill.fillAmount > manaFraction)
        {
            mpAnimationFill.color = Color.red;
        }
        else if (mpAnimationFill.fillAmount < manaFraction)
        {
            mpAnimationFill.color = Color.green;
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
