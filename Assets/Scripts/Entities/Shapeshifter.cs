using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public interface IUsesAnimator
{
    public void SetNewAnimator(Animator animator);
}
public class Shapeshifter : NetworkBehaviour
{
    public Animator defaultModel, shapeshiftedModel;
    public List<Skill> defaultSkills, shapeshiftedSkills;
    public GameObject shapeshiftVFX;

    private void Start()
    {
        SetAnimator(defaultModel);
    }
    [Command(requiresAuthority = false)]
    public void CmdShapeshift(bool defaultForm)
    {
        RpcShapeshift(defaultForm);
    }
    [ClientRpc]
    private void RpcShapeshift(bool defaultForm)
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
            GetComponent<Character>().skills = defaultSkills;
        }
        else
        {
            shapeshiftedModel.gameObject.SetActive(true);
            defaultModel.gameObject.SetActive(false);
            SetAnimator(shapeshiftedModel);
            GetComponent<Character>().skills = shapeshiftedSkills;
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
