using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Skills/Lycandruid/Wild Rage")]
public class SWildRage : Skill
{
    public float duration;
    public BuffScriptable invulnerabilityBuff;
    public Projectile projectile;
    public override void Execute(Character self)
    {
        base.Execute(self);
        castingEntity = self;
        invulnerabilityBuff.duration = duration;
        if (castingEntity.isOwned)
            castingEntity.GetComponent<Character>().CastSkill5();
        FindObjectOfType<AudioManager>().PlayOneShot(sound, castingEntity.transform.position);
        self.GetComponentInChildren<AnimatorEventReceiver>().Skill5_Casted.AddListener(Cast);
        if (castingEntity.isOwned)
        {
            castingEntity.skillIndicator.ShowRadius(1, false, RPG_Indicator.RpgIndicator.IndicatorColor.Ally, 0);
            castingEntity.skillIndicator.Casting(1.33f);
        }
        StartCasting();
    }
    public override Skill GetInstance()
    {
        var instance = (SWildRage)base.GetInstance();
        instance.duration = duration;
        instance.invulnerabilityBuff = invulnerabilityBuff;
        instance.projectile = projectile;
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
            castingEntity.GetComponent<Lycandruid>().CastWildRage();
        PlayerController player = castingEntity.GetComponent<PlayerController>();
        if (castingEntity.isServer)
            player.GetComponent<HasMana>().RpcSpendMana(manaCost);
        player.StartCooldown5();
        player.GetComponentInChildren<AnimatorEventReceiver>().Skill5_Casted.RemoveAllListeners();
        player.CmdChangeState(PlayerState.None);
    }
    public override void UpdateDescription()
    {
        invulnerabilityBuff.duration = duration;
        description = GetTextIconByStat(PlayerStat.CooldownReduction) + (cooldown * castingEntity.GetComponent<CanAttack>().GetCooldownReductionModifier()).ToString("F1")
            + " " + GetTextIconByStat(PlayerStat.MaxMana) + manaCost + "\nWolferius enters enraged state, granting himself Invulnerability." +
            " Lasts " + invulnerabilityBuff.duration + " seconds.";
        base.UpdateDescription();
    }
}
