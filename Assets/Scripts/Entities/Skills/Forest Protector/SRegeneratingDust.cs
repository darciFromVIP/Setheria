using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Forest Protector/Regenerating Dust")]
public class SRegeneratingDust : Skill
{
    public float baseHealing;
    public PlayerStat healingScalingStat;
    public float healingScalingValue;
    [HideInInspector] public float finalHealing;
    public float aoeRadius;
    public float range;
    public Projectile healingProjectile;

    [HideInInspector] public Vector3 actualPoint;
    public override Skill GetInstance()
    {
        var instance = (SRegeneratingDust)base.GetInstance();
        instance.baseHealing = baseHealing;
        instance.healingScalingStat = healingScalingStat;
        instance.healingScalingValue = healingScalingValue;
        instance.aoeRadius = aoeRadius;
        instance.range = range;
        instance.healingProjectile = healingProjectile;
        return instance;
    }
    public override void Execute(Character self)
    {
        base.Execute(self);
        self.GetComponent<PlayerController>().Ground_Left_Clicked.AddListener(StartCasting);
        self.GetComponent<PlayerController>().ChangeCastingState(CastingState.AllyOnly);
        if (castingEntity.isOwned)
            self.skillIndicator.ShowArea(aoeRadius, range, true, RPG_Indicator.RpgIndicator.IndicatorColor.Ally, 0);
    }
    public override void ExecuteOnStart(Character self)
    {
        
    }
    public override void StopExecute()
    {
        base.StopExecute();
    }
    protected override void StartCasting(Vector3 point)
    {
        base.StartCasting(point);
        FindObjectOfType<AudioManager>().PlayOneShot(sound, castingEntity.transform.position);
        actualPoint = Vector3.MoveTowards(castingEntity.transform.position, point, range);
        castingEntity.GetComponent<PlayerController>().Ground_Left_Clicked.RemoveListener(StartCasting);
        if (castingEntity.isOwned)
            castingEntity.GetComponent<Character>().CastSkill2();
        castingEntity.GetComponent<CharacterVFXReference>().skill2.SetActive(true);
        castingEntity.GetComponentInChildren<AnimatorEventReceiver>().Skill2_Casted.AddListener(Cast);
        castingEntity.GetComponent<Character>().RotateToPoint(point);
        if (castingEntity.isOwned)
            castingEntity.skillIndicator.Casting(0.8f);
    }
    protected override void Cast()
    {
        base.Cast();
        if (castingEntity.isServer)
            castingEntity.GetComponent<ForestProtector>().CastRegeneratingDust();
        PlayerController player = castingEntity.GetComponent<PlayerController>();
        if (castingEntity.isServer)
            player.GetComponent<HasMana>().RpcSpendMana(manaCost);
        player.StartCooldown2();
        player.GetComponentInChildren<AnimatorEventReceiver>().Skill2_Casted.RemoveListener(Cast);
    }

    public override void UpdateDescription()
    {
        finalHealing = baseHealing + GetScalingStatValue(healingScalingStat) * healingScalingValue;
        description = GetTextIconByStat(PlayerStat.CooldownReduction) + (cooldown * castingEntity.GetComponent<CanAttack>().GetCooldownReductionModifier()).ToString("F1") 
            + " " + GetTextIconByStat(PlayerStat.MaxMana) + manaCost + "\nSprays magical healing dust to a target area, healing <color=orange>" + finalHealing
            + "</color> damage " + "(" + baseHealing + " + " + (int)(healingScalingValue * 100) + "% " + GetTextIconByStat(healingScalingStat) + ")"
            + " to all allies hit, including self.";
        base.UpdateDescription();
    }
}
