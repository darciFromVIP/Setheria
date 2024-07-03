using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;
using FMOD.Studio;
using UnityEngine.UIElements;
using FMODUnity;

public class HasHealth : NetworkBehaviour, ISaveable
{
    [SerializeField]
    private float health;
    [Header("Don't set this on player characters! They have their data in the code.")]
    [SerializeField]
    private float maxHealth;
    private float baseMaxHealth;
    private float gearMaxHealth = 0;
    private float corruptedHealth = 0;
    private float corruption = 0;
    private float corruptionResistance = 0;
    [SerializeField]
    private float healthRegen = 0.25f;
    private float baseHealthRegen;
    private float gearHealthRegen = 0;
    private float healthRegenTimer = 0;
    [SerializeField] private float baseArmor = 0;
    private float gearArmor = 0;
    private float finalArmor;
    public EventReference soundOnDeath;
    public EventReference soundOnHit;
    public bool debug;

    [SerializeField] private bool isInvulnerable;

    [System.NonSerialized]
    public UnityEvent<float, float> Health_Changed = new();
    [System.NonSerialized]
    public UnityEvent<float, float> Corrupted_Health_Changed = new();
    [System.NonSerialized]
    public UnityEvent<float> Health_Regen_Changed = new();
    [System.NonSerialized]
    public UnityEvent<float> Armor_Changed = new();
    [System.NonSerialized]
    public UnityEvent On_Death = new();
    [System.NonSerialized]
    public UnityEvent<HasHealth> Target_Received = new();
    [System.NonSerialized]
    public UnityEvent<HasHealth> Received_Target_Lost = new();
    [System.NonSerialized]
    public UnityEvent<NetworkIdentity> Damage_Taken = new();
    [System.NonSerialized]
    public UnityEvent<NetworkIdentity, float> Damage_Taken_Amount = new();
    private void Awake()
    {
        health = maxHealth;
        baseMaxHealth = maxHealth;
        baseHealthRegen = healthRegen;
    }
    private void Update()
    {
        if (isServer)
        {
            healthRegenTimer += Time.deltaTime;
            if (healthRegenTimer >= 1)
            {
                healthRegenTimer = 0;
                if (healthRegen > 0)
                    RpcHealDamage(healthRegen, true);
                else if (healthRegen < 0)
                    RpcTakeDamage(-healthRegen, true, GetComponent<NetworkIdentity>(), false, false);
                if (corruption > 0)
                {
                    float actualCorruption = corruption - corruptionResistance;
                    if (actualCorruption > 0)
                    {
                        RpcChangeCorruptedHealth(actualCorruption);
                    }
                }
            }
        }
    }
    [Command(requiresAuthority = false)]
    public void CmdHealDamage(float heal, bool isRegen)
    {
        RpcHealDamage(heal, isRegen);
    }
    [ClientRpc]
    public void RpcHealDamage(float heal, bool isRegen)
    {
        HealDamage(heal, isRegen);
    }
    public void HealDamage(float heal, bool isRegen)
    {
        health += heal;
        if (debug)
            Debug.Log(health);
        if (!isRegen && isServer)
            FindObjectOfType<FloatingText>().CmdSpawnFloatingText("+" + ((int)heal).ToString(), transform.position, FloatingTextType.Healing);
        if (health > maxHealth)
            health = maxHealth;
        Health_Changed.Invoke(health, maxHealth);
    }
    [Command(requiresAuthority = false)]
    public void CmdTakeDamage(float damage, bool ignoreArmor, NetworkIdentity owner, bool isCritical, bool interruptCrafting)
    {
        RpcTakeDamage(damage, ignoreArmor, owner, isCritical, interruptCrafting);
    }
    [ClientRpc]
    public void RpcTakeDamage(float damage, bool ignoreArmor, NetworkIdentity owner, bool isCritical, bool interruptCrafting)
    {
        TakeDamage(damage, ignoreArmor, owner, isCritical, interruptCrafting);
    }
    public void TakeDamage(float damage, bool ignoreArmor, NetworkIdentity owner, bool isCritical, bool interruptCrafting)
    {
        if (health <= 0)
            return;
        Damage_Taken_Amount.Invoke(owner, damage);
        Damage_Taken.Invoke(owner);
        if (isInvulnerable)
            return;
        if (TryGetComponent(out PlayerController player) && interruptCrafting)
        {
            if (player.state == PlayerState.Working)
                player.ChangeState(PlayerState.None);
        }
        float finalDmg = damage;
        if (!ignoreArmor)
            finalDmg = damage * (1 - (GetFinalArmor() / 100));

        health -= finalDmg;
        if (debug)
            Debug.Log(health);
        if (!soundOnHit.IsNull)
            FindObjectOfType<AudioManager>().PlayOneShot(soundOnHit, transform.position);
        if (isServer && finalDmg >= 1)
            FindObjectOfType<FloatingText>().ServerSpawnFloatingText("-" + ((int)finalDmg).ToString(), transform.position + Vector3.up, isCritical ? FloatingTextType.CriticalDamage : FloatingTextType.Damage);
        if (health <= 0)
            OnDeath();
        Health_Changed.Invoke(health, maxHealth);
    }
    [Command(requiresAuthority = false)]
    public void CmdChangeBaseMaxHealth(float amount)
    {
        RpcChangeBaseMaxHealth(amount);
    }
    [ClientRpc]
    public void RpcChangeBaseMaxHealth(float amount)
    {
        ChangeBaseMaxHealth(amount);
    }
    public void ChangeBaseMaxHealth(float amount, bool additive = true)
    {
        if (additive)
            baseMaxHealth += amount;
        else
            baseMaxHealth = amount;
        UpdateMaxHealth();
        AdjustHealthToMaxHealth(GetFinalMaxHealth() - amount);
    }
    [Command(requiresAuthority = false)]
    public void CmdChangeGearMaxHealth(float amount)
    {
        RpcChangeGearMaxHealth(amount);
    }
    [ClientRpc]
    public void RpcChangeGearMaxHealth(float amount)
    {
        ChangeGearMaxHealth(amount);
    }
    public void ChangeGearMaxHealth(float amount)
    {
        gearMaxHealth += amount;
        UpdateMaxHealth();
        AdjustHealthToMaxHealth(GetFinalMaxHealth() - amount);
    }
    [Command(requiresAuthority = false)]
    public void CmdChangeCorruptedHealth(float amount)
    {
        RpcChangeCorruptedHealth(amount);
    }
    [ClientRpc]
    public void RpcChangeCorruptedHealth(float amount)
    {
        ChangeCorruptedHealth(amount);
    }
    public void ChangeCorruptedHealth(float amount)
    {
        corruptedHealth += amount;
        var cap = (baseMaxHealth + gearMaxHealth) * 0.7f;
        if (corruptedHealth < 0)
            corruptedHealth = 0;
        if (corruptedHealth > cap)
            corruptedHealth = cap;
        UpdateMaxHealth();
        Corrupted_Health_Changed.Invoke(baseMaxHealth + gearMaxHealth, corruptedHealth);
        if (corruptedHealth != cap)
            AdjustHealthToMaxHealth(GetFinalMaxHealth() + amount);
    }
    private void UpdateMaxHealth()
    {
        maxHealth = baseMaxHealth + gearMaxHealth - corruptedHealth;
        Health_Changed.Invoke(health, maxHealth);
    }
    private void AdjustHealthToMaxHealth(float previousMaxHealth)
    {
        var percentage = GetHealth() / previousMaxHealth;
        if (percentage < 1)
            CmdSetHealth(GetFinalMaxHealth() * percentage);
        else
            CmdSetHealth(GetFinalMaxHealth());
    }
    [Command(requiresAuthority = false)]
    public void CmdChangeBaseHealthRegen(float amount)
    {
        RpcChangeBaseHealthRegen(amount);
    }
    [ClientRpc]
    public void RpcChangeBaseHealthRegen(float amount)
    {
        ChangeBaseHealthRegen(amount);
    }
    public void ChangeBaseHealthRegen(float amount, bool additive = true)
    {
        if (additive)
            baseHealthRegen += amount;
        else
            baseHealthRegen = amount;
        UpdateHealthRegen();
    }
    [Command(requiresAuthority = false)]
    public void CmdChangeGearHealthRegen(float amount)
    {
        RpcChangeGearHealthRegen(amount);
    }
    [ClientRpc]
    public void RpcChangeGearHealthRegen(float amount)
    {
        ChangeGearHealthRegen(amount);
    }
    public void ChangeGearHealthRegen(float amount)
    {
        gearHealthRegen += amount;
        UpdateHealthRegen();
    }
    private void UpdateHealthRegen()
    {
        healthRegen = baseHealthRegen + gearHealthRegen;
        Health_Regen_Changed.Invoke(healthRegen);
    }
    [Command(requiresAuthority = false)]
    public void CmdChangeArmor(float amount)
    {
        RpcChangeArmor(amount);
    }
    [ClientRpc]
    public void RpcChangeArmor(float amount)
    {
        ChangeArmor(amount);
    }
    public void ChangeArmor(float value)
    {
        baseArmor += value;
        finalArmor = baseArmor + gearArmor;
        Armor_Changed.Invoke(finalArmor);
    }
    [Command(requiresAuthority = false)]
    public void CmdChangeGearArmor(float amount)
    {
        RpcChangeGearArmor(amount);
    }
    [ClientRpc]
    public void RpcChangeGearArmor(float amount)
    {
        ChangeGearArmor(amount);
    }
    public void ChangeGearArmor(float value)
    {
        gearArmor += value;
        finalArmor = baseArmor + gearArmor;
        Armor_Changed.Invoke(finalArmor);
    }
    public float GetBaseArmor()
    {
        return baseArmor;
    }
    public float GetFinalArmor()
    {
        return finalArmor;
    }
    public float GetHealth()
    {
        return health;
    }
    public float GetBaseMaxHealth()
    {
        return baseMaxHealth;
    }
    public float GetFinalMaxHealth()
    {
        return maxHealth;
    }
    public float GetBaseHealthRegen()
    {
        return baseHealthRegen;
    }
    public float GetFinalHealthRegen()
    {
        return healthRegen;
    }
    [Command(requiresAuthority = false)]
    public void CmdSetHealth(float value)
    {
        RpcSetHealth(value);
    }
    [ClientRpc]
    public void RpcSetHealth(float value)
    {
        SetHealth(value);
    }
    public void SetHealth(float value)
    {
        health = value;
        if (debug)
            Debug.Log(health);
        Health_Changed.Invoke(health, maxHealth);
    }
    public void SetBaseMaxHealth(float value)
    {
        baseMaxHealth = value;
        UpdateMaxHealth();
    }
    public void SetBonusMaxHealth(float value)
    {
        gearMaxHealth = value;
        UpdateMaxHealth();
    }
    public void SetBaseHealthRegen(float value)
    {
        baseHealthRegen = value;
        UpdateHealthRegen();
    }
    public void SetBonusHealthRegen(float value)
    {
        gearHealthRegen += value;
        UpdateHealthRegen();
    }
    public void SetArmor(float value)
    {
        baseArmor = value;
        finalArmor = baseArmor + gearArmor;
        Armor_Changed.Invoke(finalArmor);
    }
    public void SetCorruptionResistance(float value)
    {
        corruptionResistance = value;
    }
    public void ChangeCorruption(float value)
    {
        corruption += value;
    }
    public float GetCorruptedHealth()
    {
        return corruptedHealth;
    }
    public float GetCorruption()
    {
        return corruption;
    }
    private void OnDeath()
    {
        health = 0;
        if (!soundOnDeath.IsNull)
            FindObjectOfType<AudioManager>().PlayOneShot(soundOnDeath, transform.position);
        On_Death.Invoke();
    }
    public void SetInvulnerability(bool value)
    {
        isInvulnerable = value;
    }
    public SaveDataWorldObject SaveState()
    {
        return new SaveDataWorldObject
        {
            floatData1 = health
        };
    }

    public void LoadState(SaveDataWorldObject state)
    {
        health = state.floatData1;
    }
}
