using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class AnimatorEventReceiver : MonoBehaviour
{
    private CanAttack attackComp;

    [HideInInspector] public UnityEvent Skill1_Casted = new();
    [HideInInspector] public UnityEvent Skill2_Casted = new();
    [HideInInspector] public UnityEvent Skill3_Casted = new();
    [HideInInspector] public UnityEvent Skill4_Casted = new();
    [HideInInspector] public UnityEvent Skill5_Casted = new();

    private void Start()
    {
        attackComp = GetComponentInParent<CanAttack>();
    }
    public void MeleeAttack()
    {
        attackComp.MeleeAttack();
    }
    public void RangedAttack()
    {
        attackComp.RangedAttack();
    }
    public void Skill1()
    {
        Skill1_Casted.Invoke();
    }
    public void Skill2()
    {
        Skill2_Casted.Invoke();
    }
    public void Skill3()
    {
        Skill3_Casted.Invoke();
    }
    public void Skill4()
    {
        Skill4_Casted.Invoke();
    }
    public void Skill5()
    {
        Skill5_Casted.Invoke();
    }
    public void Death()
    {
        Destroy(gameObject);
    }
}
