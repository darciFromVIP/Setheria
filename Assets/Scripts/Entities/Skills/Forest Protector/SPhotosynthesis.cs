using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Skills/Forest Protector/Photosynthesis")]
public class SPhotosynthesis : Skill
{
    public List<Item> possiblePlants = new();

    public override void Execute(Character self)
    {
        base.Execute(self);
        castingEntity = self;
        if (castingEntity.isServer)
            castingEntity.GetComponent<Character>().CastSkill1();
        castingEntity.GetComponent<PlayerController>().ChangeState(PlayerState.Busy);
        FindObjectOfType<AudioManager>().PlayOneShot(sound, castingEntity.transform.position);
        if (castingEntity.isOwned)
            self.GetComponentInChildren<AnimatorEventReceiver>().Skill1_Casted.AddListener(Cast);
        castingEntity.skillIndicator.ShowRadius(1, false, RPG_Indicator.RpgIndicator.IndicatorColor.Ally, 0);
        castingEntity.skillIndicator.Casting(2.13f);
    }
    protected override void Cast()
    {
        base.Cast();
        Item item = possiblePlants[Random.Range(0, possiblePlants.Count)];
        castingEntity.GetComponent<PlayerCharacter>().CreateItem(new SaveDataItem() { name = item.itemData.name, stacks = 1 }, castingEntity.transform.position + castingEntity.transform.forward);
        castingEntity.GetComponent<PlayerController>().StartCooldown1();
        castingEntity.GetComponentInChildren<AnimatorEventReceiver>().Skill1_Casted.RemoveListener(Cast);
        castingEntity.GetComponent<PlayerController>().ChangeState(PlayerState.None);
    }
    public override void ExecuteOnStart(Character self)
    {

    }

    public override void StopExecute()
    {
        castingEntity.skillIndicator.InterruptCasting();
    }

    public override void UpdateDescription()
    {
        description = GetTextIconByStat(PlayerStat.CooldownReduction) + (cooldown * castingEntity.GetComponent<CanAttack>().GetCooldownReductionModifier()).ToString("F1")
            + "\nCreates a random plant in front of Nirri. Available plants:\n";
        foreach (var item in possiblePlants)
        {
            description += "- " + item.name + "\n";
        }
        base.UpdateDescription();
    }
}
