using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using FMODUnity;

public interface IUsesAnimator
{
    public void SetNewAnimator(Animator animator);
}
public class Shapeshifter : NetworkBehaviour
{
    public Animator defaultModel, shapeshiftedModel;
    public List<Skill> defaultSkills, shapeshiftedSkills;
    private List<Skill> defaultSkillInstances = new(), shapeshiftedSkillInstances = new();
    public GameObject shapeshiftVFX;
    public EventReference defaultAttackSound, shapeshiftedAttackSound;

    private void Start()
    {
        SetAnimator(defaultModel);
        foreach (var item in shapeshiftedSkills)
        {
            shapeshiftedSkillInstances.Add(item.GetInstance());
        }
    }
    public void SetDefaultSkillInstances()
    {
        defaultSkillInstances = GetComponent<Character>().skillInstances;
    }
    [Command(requiresAuthority = false)]
    public void CmdShapeshift(bool defaultForm)
    {
        RpcShapeshift(defaultForm);
    }
    [ClientRpc]
    public void RpcShapeshift(bool defaultForm)
    {
        Shapeshift(defaultForm);
    }
    private void Shapeshift(bool defaultForm)
    {
        if (defaultForm)
        {
            shapeshiftedModel.gameObject.SetActive(false);
            defaultModel.gameObject.SetActive(true);
            SetAnimator(defaultModel);
            GetComponent<Character>().skillInstances = defaultSkillInstances;
            GetComponent<CanAttack>().attackSound = defaultAttackSound;
        }
        else
        {
            shapeshiftedModel.gameObject.SetActive(true);
            defaultModel.gameObject.SetActive(false);
            SetAnimator(shapeshiftedModel);
            GetComponent<Character>().skillInstances = shapeshiftedSkillInstances;
            GetComponent<CanAttack>().attackSound = shapeshiftedAttackSound;
        }
        shapeshiftVFX.SetActive(true);
        if (isOwned)
        {
            GetComponent<Character>().UpdateSkills();
        }
    }
    private void SetAnimator(Animator animator)
    {
        foreach (var item in GetComponents<IUsesAnimator>())
        {
            item.SetNewAnimator(animator);
        }
    }
}
