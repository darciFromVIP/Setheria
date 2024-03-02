using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;
public class HasMana : NetworkBehaviour
{
    [SyncVar] private float mana;
    [SyncVar] [SerializeField] private float maxMana;
    [SyncVar] [SerializeField] private float baseMaxMana;
    [SyncVar] [SerializeField] private float gearMaxMana = 0;
    [SyncVar] [SerializeField] private float corruptedMana = 0;
    [SyncVar] [SerializeField] private float corruption = 0;
    [SyncVar] [SerializeField] private float corruptionResistance = 0;
    [SyncVar] [SerializeField] private float manaRegen = 0.15f;
    [SyncVar] private float baseManaRegen = 0;
    [SyncVar] private float gearManaRegen = 0;
    private float manaRegenTimer = 0;

    [System.NonSerialized]
    public UnityEvent<float, float> Mana_Changed = new();
    [System.NonSerialized]
    public UnityEvent<float, float> Corrupted_Mana_Changed = new();
    [System.NonSerialized]
    public UnityEvent<float> Mana_Regen_Changed = new();

    private void Start()
    {
        mana = maxMana;
        baseMaxMana = maxMana;
    }
    private void Update()
    {
        if (isServer)
        {
            manaRegenTimer += Time.deltaTime;
            if (manaRegenTimer >= 1)
            {
                manaRegenTimer = 0;
                RpcRestoreMana(manaRegen);
                if (corruption > 0)
                {
                    float actualCorruption = corruption - corruptionResistance;
                    if (actualCorruption > 0)
                    {
                        ChangeCorruptedMana(actualCorruption);
                    }
                }
            }
        }
    }
    [Command(requiresAuthority = false)]
    public void CmdRestoreMana(float amount)
    {
        RpcRestoreMana(amount);
    }
    [ClientRpc]
    public void RpcRestoreMana(float amount)
    {
        RestoreMana(amount);
    }
    public void RestoreMana(float amount)
    {
        mana += amount;
        if (mana > maxMana)
            mana = maxMana;
        Mana_Changed.Invoke(mana, maxMana);
    }
    [Command (requiresAuthority = false)]
    public void CmdSpendMana(float amount)
    {
        RpcSpendMana(amount);
    }
    [ClientRpc]
    public void RpcSpendMana(float amount)
    {
        SpendMana(amount);
    }
    public void SpendMana(float amount)
    {
        mana -= amount;
        if (mana < 0)
            mana = 0;
        Mana_Changed.Invoke(mana, maxMana);
    }
    [Command(requiresAuthority = false)]
    public void CmdChangeBaseMaxMana(float amount)
    {
        RpcChangeBaseMaxMana(amount);
    }
    [ClientRpc]
    public void RpcChangeBaseMaxMana(float amount)
    {
        ChangeBaseMaxMana(amount);
    }
    public void ChangeBaseMaxMana(float amount)
    {
        baseMaxMana += amount;
        UpdateMaxMana();
        AdjustManaToMaxMana(GetFinalMaxMana() - amount);
    }
    [Command(requiresAuthority = false)]
    public void CmdChangeGearMaxMana(float amount)
    {
        RpcChangeGearMaxMana(amount);
    }
    [ClientRpc]
    public void RpcChangeGearMaxMana(float amount)
    {
        ChangeGearMaxMana(amount);
    }
    public void ChangeGearMaxMana(float amount)
    {
        gearMaxMana += amount;
        UpdateMaxMana();
        AdjustManaToMaxMana(GetFinalMaxMana() - amount);
    }
    [Command(requiresAuthority = false)]
    public void CmdChangeCorruptedMana(float amount)
    {
        RpcChangeCorruptedMana(amount);
    }
    [ClientRpc]
    public void RpcChangeCorruptedMana(float amount)
    {
        ChangeCorruptedMana(amount);
    }
    public void ChangeCorruptedMana(float amount)
    {
        corruptedMana += amount;
        if (corruptedMana < 0)
            corruptedMana = 0;
        while (GetFinalMaxMana() - corruptedMana < 1)
        {
            corruptedMana -= 0.1f;
        }
        UpdateMaxMana();
        Corrupted_Mana_Changed.Invoke(baseMaxMana + gearMaxMana, corruptedMana);
        AdjustManaToMaxMana(GetFinalMaxMana() - amount);
    }
    private void UpdateMaxMana()
    {
        maxMana = baseMaxMana + gearMaxMana - corruptedMana;
        Mana_Changed.Invoke(mana, maxMana);
    }
    private void AdjustManaToMaxMana(float previousMaxMana)
    {
        if (previousMaxMana > GetFinalMaxMana())
            return;
        var percentage = GetMana() / previousMaxMana;
        if (percentage < 1)
            CmdSetMana(GetFinalMaxMana() * percentage);
        else
            CmdSetMana(GetFinalMaxMana());
    }
    [Command(requiresAuthority = false)]
    public void CmdChangeBaseManaRegen(float amount)
    {
        RpcChangeBaseManaRegen(amount);
    }
    [ClientRpc]
    public void RpcChangeBaseManaRegen(float amount)
    {
        ChangeBaseManaRegen(amount);
    }
    public void ChangeBaseManaRegen(float amount)
    {
        baseManaRegen += amount;
        UpdateManaRegen();
    }
    [Command(requiresAuthority = false)]
    public void CmdChangeGearManaRegen(float amount)
    {
        RpcChangeGearManaRegen(amount);
    }
    [ClientRpc]
    public void RpcChangeGearManaRegen(float amount)
    {
        ChangeGearManaRegen(amount);
    }
    public void ChangeGearManaRegen(float amount)
    {
        gearManaRegen += amount;
        UpdateManaRegen();
    }
    private void UpdateManaRegen()
    {
        manaRegen = baseManaRegen + gearManaRegen;
        Mana_Regen_Changed.Invoke(manaRegen);
    }
    public float GetMana()
    {
        return mana;
    }
    public float GetBaseMaxMana()
    {
        return baseMaxMana;
    }
    public float GetFinalMaxMana()
    {
        return maxMana;
    }
    public float GetBaseManaRegen()
    {
        return baseManaRegen;
    }
    public float GetFinalManaRegen()
    {
        return manaRegen;
    }
    public float GetCorruptedMana()
    {
        return corruptedMana;
    }
    public void SetCorruptionResistance(float value)
    {
        corruptionResistance = value;
    }
    public void ChangeCorruption(float value)
    {
        corruption += value;
    }
    [Command(requiresAuthority = false)]
    public void CmdSetMana(float value)
    {
        RpcSetMana(value);
    }
    [ClientRpc]
    public void RpcSetMana(float value)
    {
        SetMana(value);
    }
    public void SetMana(float value)
    {
        mana = value;
        Mana_Changed.Invoke(GetMana(), GetFinalMaxMana());
    }
    public void SetMaxMana(float value)
    {
        baseMaxMana = value;
        UpdateMaxMana();
        Mana_Changed.Invoke(GetMana(), GetFinalMaxMana());
    }
    public void SetBaseManaRegen(float value)
    {
        baseManaRegen = value;
        UpdateManaRegen();
    }
}
