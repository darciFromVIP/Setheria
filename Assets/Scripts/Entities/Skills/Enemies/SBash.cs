using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "Skills/Enemies/Bash")]
public class SBash : Skill
{
    public float radius;
    public float baseDamage;
    public Projectile projectile;

    public override void Execute(Character self)
    {
        base.Execute(self);
        if (Vector3.Distance(castingEntity.transform.position, castingEntity.GetComponent<CanAttack>().enemyTarget.transform.position) > radius)
        {
            castingEntity.GetComponent<CanMove>().Moved_Within_Range.AddListener(StartCasting);
            castingEntity.GetComponent<CanMove>().MoveWithinRangeEnemy(self.GetComponent<CanAttack>().enemyTarget.transform, radius);
        }
        else
        {
            StartCasting();
        }
    }

    public override void ExecuteOnStart(Character self)
    {
    }

    public override void StopExecute()
    {
        base.StopExecute();
        castingEntity.GetComponentInChildren<AnimatorEventReceiver>().Skill1_Casted.RemoveAllListeners();
        castingEntity.GetComponent<CanMove>().Moved_Within_Range.RemoveAllListeners();
        castingEntity.skillIndicator.InterruptCasting();
        castingEntity.Stun_Begin.RemoveAllListeners();
        castingEntity.animator.SetTrigger("Reset");
    }
    protected override void StartCasting()
    {
        base.StartCasting();
        if (castingEntity.GetComponent<CanAttack>().enemyTarget == null)
        {
            castingEntity.GetComponent<CanMove>().Moved_Within_Range.RemoveListener(StartCasting);
            return;
        }
        if (castingEntity.isServer)
            castingEntity.GetComponent<Character>().CastSkill1();
        castingEntity.Stun_Begin.AddListener(StopExecute);
        castingEntity.GetComponentInChildren<AnimatorEventReceiver>().Skill1_Casted.AddListener(Cast);

        castingEntity.skillIndicator.ShowRadius(radius, true, RPG_Indicator.RpgIndicator.IndicatorColor.Enemy, 0);
        castingEntity.skillIndicator.Casting(2);
    }
    protected override void Cast()
    {
        base.Cast();
        if (castingEntity.isServer)
            castingEntity.GetComponent<EnemySkills>().CastBash();
        castingEntity.GetComponent<CanMove>().Moved_Within_Range.RemoveListener(StartCasting);
        if (!sound.IsNull)
            FindObjectOfType<AudioManager>().PlayOneShot(sound, castingEntity.GetComponent<CanAttack>().enemyTarget.transform.position);
        castingEntity.StartCooldown1();
        castingEntity.GetComponentInChildren<AnimatorEventReceiver>().Skill1_Casted.RemoveAllListeners();
        castingEntity.GetComponent<CanAttack>().CmdResumeActing();
        castingEntity.Stun_Begin.RemoveAllListeners();
    }
}
