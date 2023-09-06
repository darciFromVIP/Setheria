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
    [SyncVar] [SerializeField] private float bonusMaxMana = 0;
    [SyncVar] [SerializeField] private float manaRegen = 0.15f;
    [SyncVar] private float baseManaRegen = 0;
    [SyncVar] private float bonusManaRegen = 0;
    private float manaRegenTimer = 0;

    [System.NonSerialized]
    public UnityEvent<float, float> Mana_Changed = new();
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
            }
        }
    }
    [Command]
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
    [Command]
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
    }
    [Command]
    public void CmdChangeBonusMaxMana(float amount)
    {
        RpcChangeBonusMaxMana(amount);
    }
    [ClientRpc]
    public void RpcChangeBonusMaxMana(float amount)
    {
        ChangeBonusMaxMana(amount);
    }
    public void ChangeBonusMaxMana(float amount)
    {
        bonusMaxMana += amount;
        UpdateMaxMana();
    }
    private void UpdateMaxMana()
    {
        maxMana = baseMaxMana + bonusMaxMana;
        Mana_Changed.Invoke(mana, maxMana);
    }
    [Command]
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
    [Command]
    public void CmdChangeBonusManaRegen(float amount)
    {
        RpcChangeBonusManaRegen(amount);
    }
    [ClientRpc]
    public void RpcChangeBonusManaRegen(float amount)
    {
        ChangeBonusManaRegen(amount);
    }
    public void ChangeBonusManaRegen(float amount)
    {
        bonusManaRegen += amount;
        UpdateManaRegen();
    }
    private void UpdateManaRegen()
    {
        manaRegen = baseManaRegen + bonusManaRegen;
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
    public float GetBonusMaxMana()
    {
        return bonusMaxMana;
    }
    public float GetFinalMaxMana()
    {
        return maxMana;
    }
    public float GetBaseManaRegen()
    {
        return baseManaRegen;
    }
    public float GetBonusManaRegen()
    {
        return bonusManaRegen;
    }
    public float GetFinalManaRegen()
    {
        return manaRegen;
    }
    public void SetMana(float value)
    {
        mana = value;
    }
    public void SetMaxMana(float value)
    {
        baseMaxMana = value;
        UpdateMaxMana();
        Mana_Changed.Invoke(mana, maxMana);
    }
    public void SetBaseManaRegen(float value)
    {
        baseManaRegen = value;
        UpdateManaRegen();
    }
    public void SetBonusManaRegen(float value)
    {
        bonusManaRegen = value;
        UpdateManaRegen();
    }
}
