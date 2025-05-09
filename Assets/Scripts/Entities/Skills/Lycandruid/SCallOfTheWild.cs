using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Skills/Lycandruid/Call of the Wild")]
public class SCallOfTheWild : Skill
{
    public float timedLife;
    public float basePower;
    public PlayerStat powerScalingStat;
    public float powerScalingValue;
    private float finalPower;
    public int baseNumberOfWolves;
    public Character wolfPrefab;
    public override void Execute(Character self)
    {
        base.Execute(self);
        castingEntity = self;
        if (castingEntity.isOwned)
            castingEntity.GetComponent<Character>().CastSkill5();
        FindObjectOfType<AudioManager>().PlayOneShot(sound, castingEntity.transform.position);
        self.GetComponentInChildren<AnimatorEventReceiver>().Skill5_Casted.AddListener(Cast);
        if (castingEntity.isOwned)
        {
            castingEntity.skillIndicator.ShowRadius(1, false, RPG_Indicator.RpgIndicator.IndicatorColor.Ally, 0);
            castingEntity.skillIndicator.Casting(2.36f);
        }
        StartCasting();
    }
    public override Skill GetInstance()
    {
        var instance = (SCallOfTheWild)base.GetInstance();
        instance.timedLife = timedLife;
        instance.basePower = basePower;
        instance.powerScalingStat = powerScalingStat;
        instance.powerScalingValue = powerScalingValue;
        instance.baseNumberOfWolves = baseNumberOfWolves;
        instance.wolfPrefab = wolfPrefab;
        return instance;
    }
    public override void StopExecute()
    {
        base.StopExecute();
    }
    protected override void Cast()
    {
        base.Cast();
        if (castingEntity.isServer)
            for (int i = 0; i < baseNumberOfWolves; i++)
            {
                var pos = castingEntity.transform.position - (castingEntity.transform.forward * 2) - (castingEntity.transform.right * 4) + (castingEntity.transform.right * i * 2);
                castingEntity.GetComponent<CanHavePets>().SpawnPet(wolfPrefab.name, pos, timedLife, finalPower);
            }

        PlayerController player = castingEntity.GetComponent<PlayerController>();
        if (castingEntity.isServer)
            player.GetComponent<HasMana>().RpcSpendMana(manaCost);
        player.StartCooldown5();
        player.GetComponentInChildren<AnimatorEventReceiver>().Skill5_Casted.RemoveAllListeners();
        player.CmdChangeState(PlayerState.None);
    }
    public override void UpdateDescription()
    {
        finalPower = basePower + (GetScalingStatValue(powerScalingStat) * powerScalingValue);
        description = GetTextIconByStat(PlayerStat.CooldownReduction) + (cooldown * castingEntity.GetComponent<CanAttack>().GetCooldownReductionModifier()).ToString("F1")
            + " " + GetTextIconByStat(PlayerStat.MaxMana) + manaCost + "\nWolferius unleashes a resonating howl to call his pack. Summons " + baseNumberOfWolves +
            " wolves to aid him in battle. Each wolf has <color=orange>" + finalPower.ToString("F0") + "</color> (" + basePower.ToString("F0") + " + "
            + (powerScalingValue * 100).ToString("F0") + "% " + GetTextIconByStat(powerScalingStat) + ") power and lasts for " + timedLife + " seconds.";
        base.UpdateDescription();
    }
}
