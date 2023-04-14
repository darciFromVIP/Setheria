using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
[CreateAssetMenu(menuName = "Actions/Build Structure")]
public class BuildStructure : ActionTemplate
{
    public GameObject structurePrefab;
    public GameObject structureGhostPrefab;

    public override void ActionFinished()
    {
        Action_Finished.Invoke();
    }

    public override void Execute()
    {
        CanBuild temp = FindObjectOfType<GameManager>().localPlayerCharacter.GetComponent<CanBuild>();
        temp.buildStructureAction = this;
        temp.Structure_Built.AddListener(ActionFinished);
        temp.BeginBuildingStructure(this);
    }
    public void StopExecute(CanBuild canBuildComp)
    {
        canBuildComp.Structure_Built.RemoveListener(ActionFinished);
    }
    public override bool TestExecute()
    {
        return true;
    }
}
