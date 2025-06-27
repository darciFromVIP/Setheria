using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Lycandruid/Way of the Lupine")]
public class SWayOfTheLupine : Skill
{
    public override void Execute(Character self)
    {
        castingEntity = self;
        self.GetComponent<CanAttack>().CmdChangePowerMultiplier(0.25f);
    }
    public override void StopExecute()
    {
        base.StopExecute();
        castingEntity.GetComponent<CanAttack>().CmdChangePowerMultiplier(-0.25f);
    }
    public override void ExecuteOnStart(Character self)
    {
        base.ExecuteOnStart(self);
    }
    public override void UpdateDescription()
    {
        description = "Wolferius embraced the wolf, unlocking following features:" +
            "\n- Enemy Beasts will ignore Wolferius unless he attacks first." +
            "\n- Wolferius gains +25% <sprite=3>" +
            "\n- Wolf form can now interact with everything." +
            "\n- Wolferius cannot shapeshift back to human anymore. ";
        base.UpdateDescription();
    }
}
