using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Actions/Gain Profession")]
public class AGainProfession : ActionTemplate
{
    public TalentTreeType professionType;
    public int amountGained;
    public override void ActionFinished()
    {
       
    }

    public override void Execute()
    {
        var prof = FindObjectOfType<GameManager>().localPlayerCharacter.professions;
        switch (professionType)
        {
            case TalentTreeType.Special:
                break;
            case TalentTreeType.Gathering:
                prof.AddGathering(amountGained);
                break;
            case TalentTreeType.Cooking:
                prof.AddCooking(amountGained);
                break;
            case TalentTreeType.Alchemy:
                prof.AddAlchemy(amountGained);
                break;
            case TalentTreeType.Fishing:
                prof.AddFishing(amountGained);
                break;
            case TalentTreeType.Exploration:
                prof.AddExploration(amountGained);
                break;
            default:
                break;
        }
        Action_Finished.Invoke();
    }

    public override bool TestExecute()
    {
        var prof = FindObjectOfType<GameManager>().localPlayerCharacter.professions;
        if (prof.GetProfessionExperience(professionType) > 0)
        {
            FindObjectOfType<SystemMessages>().AddMessage("You can't use this anymore.", MsgType.Error);
            return false;
        }
        else
            return true;
    }
}
