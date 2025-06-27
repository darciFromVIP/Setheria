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
    public List<Skill> defaultSkillInstances = new(), shapeshiftedSkillInstances = new();
    public GameObject shapeshiftVFX;
    public EventReference defaultAttackSound, shapeshiftedAttackSound;

    private void Start()
    {
        SetAnimator(defaultModel);
        foreach (var item in shapeshiftedSkills)
        {
            shapeshiftedSkillInstances.Add(item.GetInstance());
        }
        foreach (var item in shapeshiftedSkillInstances)
        {
            item.ExecuteOnStart(GetComponent<Character>());
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
    [Command(requiresAuthority = false)]
    public void CmdPermanentShapeshift(bool defaultForm)
    {
        RpcPermanentShapeshift(defaultForm);
    }
    [ClientRpc()]
    public void RpcPermanentShapeshift(bool defaultForm)
    {
        Shapeshift(defaultForm);
        var character = GetComponent<Character>();
        character.skillInstances[1].StopExecute();
        character.skillInstances.RemoveAt(1);
        if (defaultForm)
            character.skillInstances.Insert(1, defaultSkills.Find((x) => x is SWayOfTheSapiens).GetInstance());
        else
            character.skillInstances.Insert(1, shapeshiftedSkills.Find((x) => x is SWayOfTheLupine).GetInstance());
        character.skillInstances[1].Execute(character);
        character.UpdateSkills();
    }
    [Command(requiresAuthority = false)]
    public void CmdRevertPermanentShapeshift()
    {
        RpcRevertPermanentShapeshift();
    }
    [ClientRpc()]
    public void RpcRevertPermanentShapeshift()
    {
        var character = GetComponent<Character>();
        character.skillInstances[1].StopExecute();
        character.skillInstances.RemoveAt(1);
        if (defaultModel.gameObject.activeSelf)
            character.skillInstances.Insert(1, defaultSkills.Find((x) => x is SDefensiveStance).GetInstance());
        else
            character.skillInstances.Insert(1, shapeshiftedSkills.Find((x) => x is SOneWithNature).GetInstance());
        character.skillInstances[1].ExecuteOnStart(transform);
        character.UpdateSkills();
    }
}
