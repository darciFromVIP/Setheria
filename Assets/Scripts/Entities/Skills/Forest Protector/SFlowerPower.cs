using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Forest Protector/Flower Power")]
public class SFlowerPower : Skill
{
    public float timedLife;
    public float baseDamage;
    public PlayerStat damageScalingStat;
    public float damageScalingValue;
    public float baseHeal;
    public PlayerStat healScalingStat;
    public float healScalingValue;
    private float finalDamage;
    private float finalHeal;
    public float range;
    public Character flowerPrefab;
    public SFlowerPowerHeal healingSkillRef;
    public EventReference GrowSound;

    private Vector3 actualPoint;
    public override void Execute(Character self)
    {
        base.Execute(self);
        self.GetComponent<PlayerController>().Ground_Left_Clicked.AddListener(StartCasting);
        self.GetComponent<PlayerController>().ChangeCastingState(CastingState.None);
        if (castingEntity.isOwned)
            castingEntity.skillIndicator.ShowRange(range, RPG_Indicator.RpgIndicator.IndicatorColor.Ally, 0);
    }
    public override Skill GetInstance()
    {
        var instance = (SFlowerPower)base.GetInstance();
        instance.baseDamage = baseDamage;
        instance.damageScalingStat = damageScalingStat;
        instance.damageScalingValue = damageScalingValue;
        instance.baseHeal = baseHeal;
        instance.healScalingStat = healScalingStat;
        instance.healScalingValue = healScalingValue;
        instance.range = range;
        instance.timedLife = timedLife;
        instance.flowerPrefab = flowerPrefab;
        instance.healingSkillRef = healingSkillRef;
        instance.GrowSound = GrowSound;
        return instance;
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
            castingEntity.GetComponent<Character>().CastSkill5();
        castingEntity.GetComponentInChildren<AnimatorEventReceiver>().Skill5_Casted.AddListener(Cast);
        castingEntity.GetComponent<Character>().RotateToPoint(point);
        if (castingEntity.isOwned)
            castingEntity.skillIndicator.Casting(2);
    }
    protected override void Cast()
    {
        base.Cast();
        if (castingEntity.isServer)
            castingEntity.GetComponent<CanHavePets>().SpawnPet(flowerPrefab.name, actualPoint, timedLife, finalDamage);
        PlayerController player = castingEntity.GetComponent<PlayerController>();
        if (castingEntity.isServer)
            player.GetComponent<HasMana>().RpcSpendMana(manaCost);
        player.StartCooldown5();
        player.GetComponentInChildren<AnimatorEventReceiver>().Skill5_Casted.RemoveListener(Cast);
    }

    public override void UpdateDescription()
    {
        finalDamage = baseDamage + GetScalingStatValue(damageScalingStat) * damageScalingValue;
        finalHeal = baseHeal + GetScalingStatValue(healScalingStat) * healScalingValue;
        healingSkillRef.heal = finalHeal;
        description = GetTextIconByStat(PlayerStat.CooldownReduction) + (cooldown * castingEntity.GetComponent<CanAttack>().GetCooldownReductionModifier()).ToString("F1")
            + " " + GetTextIconByStat(PlayerStat.MaxMana) + manaCost + "\nMakes Alkestia grow on the target point. She attacks nearby enemies, dealing <color=orange>" + finalDamage
            + "</color> damage " + "(" + baseDamage + " + " + (int)(damageScalingValue * 100) + "% " + GetTextIconByStat(damageScalingStat) + ")"
            + " and occasionaly heals nearby allies for <color=orange>" + finalHeal + "</color> (" + baseHeal + " + " + (int)(healScalingValue * 100)
            + "% " + GetTextIconByStat(healScalingStat) + ")." + " Lasts " + timedLife + " seconds.";
        base.UpdateDescription();
    }
}
