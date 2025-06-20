using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Skills/Forest Protector/Photosynthesis")]
public class SPhotosynthesis : Skill
{
    public List<ItemScriptable> possiblePlants = new();
    public ItemScriptable chosenPlant;
    public override void Execute(Character self)
    {
        base.Execute(self);
        castingEntity = self;
        if (castingEntity.isOwned)
            castingEntity.GetComponent<Character>().CastSkill1();
        FindObjectOfType<AudioManager>().PlayOneShot(sound, castingEntity.transform.position);
        if (castingEntity.isOwned)
            self.GetComponentInChildren<AnimatorEventReceiver>().Skill1_Casted.AddListener(Cast);
        if (castingEntity.isOwned)
        {
            castingEntity.skillIndicator.ShowRadius(1, false, RPG_Indicator.RpgIndicator.IndicatorColor.Ally, 0);
            castingEntity.skillIndicator.Casting(2.13f);
        }
        StartCasting();
    }
    public override Skill GetInstance()
    {
        var instance = (SPhotosynthesis)base.GetInstance();
        instance.possiblePlants = possiblePlants;
        return instance;
    }
    protected override void Cast()
    {
        base.Cast();
        ItemScriptable item = chosenPlant ? chosenPlant : possiblePlants[Random.Range(0, possiblePlants.Count)];
        castingEntity.GetComponent<PlayerCharacter>().CreateItem(new SaveDataItem() { name = item.name, stacks = 1 }, castingEntity.transform.position + castingEntity.transform.forward);
        castingEntity.GetComponent<PlayerController>().StartCooldown1();
        castingEntity.GetComponentInChildren<AnimatorEventReceiver>().Skill1_Casted.RemoveListener(Cast);
        castingEntity.GetComponent<PlayerController>().CmdChangeState(PlayerState.None);
    }
    public override void ExecuteOnStart(Character self)
    {

    }

    public override void StopExecute()
    {
        base.StopExecute();
    }

    public override void UpdateDescription()
    {
        description = GetTextIconByStat(PlayerStat.CooldownReduction) + (cooldown * castingEntity.GetComponent<CanAttack>().GetCooldownReductionModifier()).ToString("F1")
            + "\nCreates a chosen plant in front of Nirri: " + (chosenPlant ? chosenPlant.name : "Random") + "\nChoose your plant in the Talent screen.";
        base.UpdateDescription();
    }
}
