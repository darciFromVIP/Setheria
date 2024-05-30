using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "Skills/Enemies/Stunning Dash")]
public class SStunningDash : Skill
{
    public float range;
    public float baseDuration;
    public BuffScriptable stunBuff;
    public Projectile projectile;

    public List<Character> enemiesHit;
    public override void Execute(Character self)
    {
        base.Execute(self);
        enemiesHit.Clear();
        enemiesHit = new List<Character>();
        if (Vector3.Distance(castingEntity.transform.position, castingEntity.GetComponent<CanAttack>().enemyTarget.transform.position) > range)
        {
            castingEntity.GetComponent<CanMove>().Moved_Within_Range.AddListener(StartCasting);
            castingEntity.GetComponent<CanMove>().MoveWithinRangeEnemy(self.GetComponent<CanAttack>().enemyTarget.transform, range);
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
        castingEntity.GetComponent<Character>().transform.rotation = Quaternion.LookRotation(castingEntity.GetComponent<CanAttack>().enemyTarget.transform.position - castingEntity.transform.position);
        castingEntity.RemoveRotateTarget();

        castingEntity.skillIndicator.ShowLine(castingEntity.GetComponent<CapsuleCollider>().radius, range, true, RPG_Indicator.RpgIndicator.IndicatorColor.Enemy, 0);
        castingEntity.skillIndicator.Casting(2);
    }
    protected override void Cast()
    {
        castingEntity.StartCoroutine(Dash());
    }
    private IEnumerator Dash()
    {
        var moveComp = castingEntity.GetComponent<CanMove>();
        Collider[] hits = new Collider[5];

        Vector3 pointToDash = castingEntity.transform.TransformPoint(Vector3.forward * range);

        float timer = 0;
        while (Vector3.Distance(castingEntity.transform.position, pointToDash) > 0.1f)
        {
            timer += Time.deltaTime;
            if (timer > 2)
                break;
            castingEntity.transform.position = Vector3.MoveTowards(castingEntity.transform.position, pointToDash, Time.deltaTime * 10);
            Physics.OverlapSphereNonAlloc(castingEntity.transform.position, castingEntity.GetComponent<CapsuleCollider>().radius, hits, castingEntity.enemyLayers);
            foreach (var item in hits)
            {
                if (item != null)
                {
                    if (item.TryGetComponent(out Character character))
                    {
                        enemiesHit.Add(character);
                    }
                }
            }
            
            if (enemiesHit.Count > 0)
            {
                break;
            }
            yield return null;
        }
        stunBuff.duration = baseDuration;
        CastEffect();
    }
    protected void CastEffect()
    {
        base.Cast();
        if (castingEntity.isServer)
            castingEntity.GetComponent<EnemySkills>().CastStunningDash();
        castingEntity.GetComponent<CanMove>().Moved_Within_Range.RemoveListener(StartCasting);
        if (!sound.IsNull)
            FindObjectOfType<AudioManager>().PlayOneShot(sound, castingEntity.GetComponent<CanAttack>().enemyTarget.transform.position);
        castingEntity.StartCooldown1();
        castingEntity.GetComponentInChildren<AnimatorEventReceiver>().Skill1_Casted.RemoveAllListeners();
        castingEntity.SetRotateTarget(castingEntity.GetComponent<CanAttack>().enemyTarget.transform);
        castingEntity.GetComponent<CanAttack>().CmdResumeActing();
        castingEntity.Stun_Begin.RemoveAllListeners();
    }
}
