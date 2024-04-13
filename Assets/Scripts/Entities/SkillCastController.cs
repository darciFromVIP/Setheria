using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class SkillCastController : MonoBehaviour
{
    private Character self;
    private int currentIndex = 0;
    private bool isCasting = false;
    private bool hasTarget = false;
    private void Start()
    {
        GetComponent<CanAttack>().Target_Acquired.AddListener(StartCasting);
        GetComponent<CanAttack>().Target_Lost.AddListener(StopCasting);
        GetComponent<Entity>().On_Death.AddListener(StopCasting);
        self = GetComponent<Character>();
    }
    private void StartCasting(NetworkIdentity enemy)
    {
        if (hasTarget)
            return;
        hasTarget = true;
        if (!isCasting)
        {
            currentIndex = 0;
            Debug.Log("Starting Coroutine");
            StartCoroutine("Casting");
        }
    }
    private void StopCasting()
    {
        hasTarget = false;
        StartCoroutine(DelayedStopCasting());
    }
    private IEnumerator DelayedStopCasting()
    {
        yield return new WaitForSeconds(0.5f);
        if (!hasTarget)
        {
            isCasting = false;
            StopAllCoroutines();
        }
    }
    private IEnumerator Casting()
    {
        isCasting = true;
        while (true)
        {
            yield return new WaitForSeconds(self.skills[currentIndex].cooldown);
            self.skills[currentIndex].Execute(self);
            currentIndex++;
            if (currentIndex >= self.skills.Count)
                currentIndex = 0;
        }
    }
}
