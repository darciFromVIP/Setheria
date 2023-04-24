using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class SkillCastController : MonoBehaviour
{
    private Character self;
    private int currentIndex = 0;
    private bool isCasting = false;
    private void Start()
    {
        GetComponent<CanAttack>().Target_Acquired.AddListener(StartCasting);
        GetComponent<CanAttack>().Target_Lost.AddListener(StopCasting);
        GetComponent<Entity>().On_Death.AddListener(StopCasting);
        self = GetComponent<Character>();
    }
    private void StartCasting(NetworkIdentity enemy)
    {
        if (!isCasting)
        {
            currentIndex = 0;
            StartCoroutine("Casting");
        }
    }
    private void StopCasting()
    {
        StopCoroutine("Casting");
        isCasting = false;
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
