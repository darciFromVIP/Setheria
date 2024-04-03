using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

[CreateAssetMenu(menuName = "Skills/Aura")]
public class SAura : Skill
{
    private List<Character> enteredCharacters = new();
    public List<BuffScriptable> buffsApplied;
    public LayerMask layerMask;
    public float radius;
    public override void ExecuteOnStart(Transform self)
    {
        base.ExecuteOnStart(self);
        var collider = self.Find("Aura").GetComponent<Collider>();
        if (collider is SphereCollider)
            (collider as SphereCollider).radius = radius;
        collider.GetComponent<AuraEventReceiver>().On_Trigger_Stay.AddListener(OnTriggerStay);
        collider.GetComponent<AuraEventReceiver>().On_Trigger_Exit.AddListener(OnTriggerExit);
    }
    public override void ExecuteOnStart(Structure self)
    {
        base.ExecuteOnStart(self);
        self.On_Death.AddListener(OnDeath);
        var collider = self.transform.Find("Aura").GetComponent<Collider>();
        if (collider is SphereCollider)
            (collider as SphereCollider).radius = radius;
        collider.GetComponent<AuraEventReceiver>().On_Trigger_Stay.AddListener(OnTriggerStay);
        collider.GetComponent<AuraEventReceiver>().On_Trigger_Exit.AddListener(OnTriggerExit);
    }
    private void OnTriggerStay(Collider other)
    {
        if (other == null)
            return;
        if (layerMask == (layerMask | (1 << other.gameObject.layer)))
        {
            if (other.TryGetComponent(out Character character))
            {
                foreach (var item in buffsApplied)
                {
                    if (!enteredCharacters.Contains(character))
                        enteredCharacters.Add(character);
                    character.RpcAddBuff(item.name);
                }
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other == null)
            return;
        if (layerMask == (layerMask | (1 << other.gameObject.layer)))
        {
            if (other.TryGetComponent(out Character character))
            {
                foreach (var item in buffsApplied)
                {
                    if (enteredCharacters.Contains(character))
                        enteredCharacters.Remove(character);
                    character.RpcRemoveBuff(item.name);
                }
            }
        }
    }
    private void OnDeath()
    {
        foreach (var item in enteredCharacters)
        {
            foreach (var item2 in buffsApplied)
            {
                item.RpcRemoveBuff(item2.name);
            }
        }
    }
}
