using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;
public class Character : Entity
{
    [SerializeField] protected float baseRotateSpeed;
    protected Transform rotateTarget;

    [HideInInspector] public UnityEvent Stop_Acting = new();

    protected int animHash_Skill1 = Animator.StringToHash("Skill1");
    protected int animHash_Skill2 = Animator.StringToHash("Skill2");
    protected int animHash_Skill3 = Animator.StringToHash("Skill3");
    protected int animHash_Skill4 = Animator.StringToHash("Skill4");
    protected int animHash_Skill5 = Animator.StringToHash("Skill5");

    protected override void Start()
    {
        base.Start();
        if (TryGetComponent(out CanAttack attackComp))
        {
            attackComp.Target_Acquired.AddListener(RotateTargetAcquired);
            attackComp.Target_Lost.AddListener(RotateTargetLost);
        }
    }
    protected void Update()
    {
        if (rotateTarget)
            RotateToTarget();
    }
    protected void RotateToTarget()
    {
        Vector3 targetDir = new Vector3(rotateTarget.position.x, 0, rotateTarget.position.z) - new Vector3(transform.position.x, 0, transform.position.z);
        float step = baseRotateSpeed * Time.deltaTime;
        Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0f);
        transform.rotation = Quaternion.LookRotation(newDir);
    }
    public void RotateToPoint(Vector3 point)
    {
        StartCoroutine(RotateToPointCoro(point));
    }
    private IEnumerator RotateToPointCoro(Vector3 point)
    {
        Vector3 targetDir = new Vector3(point.x, 0, point.z) - new Vector3(transform.position.x, 0, transform.position.z);
        do
        {
            float step = baseRotateSpeed * Time.deltaTime;
            Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0f);
            transform.rotation = Quaternion.LookRotation(newDir);
            yield return null;
        } while (Quaternion.Angle(Quaternion.LookRotation(new Vector3(transform.forward.x, 0, transform.forward.z)), Quaternion.LookRotation(targetDir)) > 10);
    }
    private void RotateTargetAcquired(NetworkIdentity target)
    {
        rotateTarget = target.transform;
    }
    private void RotateTargetLost()
    {
        rotateTarget = null;
    }
    public void CastSkill1()
    {
        animator.SetTrigger(animHash_Skill1);
        StopActing();
    }
    public void CastSkill2()
    {
        animator.SetTrigger(animHash_Skill2);
        StopActing();
    }
    public void CastSkill3()
    {
        animator.SetTrigger(animHash_Skill3);
        StopActing();
    }
    public void CastSkill4()
    {
        animator.SetTrigger(animHash_Skill4);
        StopActing();
    }
    public void CastSkill5()
    {
        animator.SetTrigger(animHash_Skill5);
        StopActing();
    }
    private void StopActing()
    {
        Stop_Acting.Invoke();
    }
}
