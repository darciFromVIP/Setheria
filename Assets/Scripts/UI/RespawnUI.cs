using Mono.CecilX.Cil;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RespawnUI : MonoBehaviour
{
    public Button heartstoneBTN, startPointBTN, baseBTN;
    public Slider deathBar;
    public TextMeshProUGUI deathTimerText;
    public GameObject window;

    public float deathTime;
    private float deathCountdown;
    private Heartstone closestHeartstone;
    private PlayerCharacter localPlayer;
    public void Show()
    {
        if (window.activeSelf)
            return;
        deathCountdown = deathTime;
        deathBar.maxValue = deathTime;
        window.SetActive(true);
        StartCoroutine(StartDeathTimer());
    }
    private IEnumerator StartDeathTimer()
    {
        while (deathCountdown > 0)
        {
            deathCountdown -= Time.deltaTime;
            deathTimerText.text = deathCountdown.ToString("F0");
            deathBar.value = deathCountdown;
            yield return null;
        }
        deathBar.value = 0;

        float distance = 9999;
        localPlayer = FindObjectOfType<GameManager>().localPlayerCharacter;
        foreach (var item in FindObjectsOfType<Heartstone>())
        {
            if (item.respawnActivated)
            {
                float temp = Vector3.Distance(localPlayer.transform.position, item.transform.position);
                if (temp < distance)
                {
                    distance = temp;
                    closestHeartstone = item;
                }
            }
        }
        if (closestHeartstone != null)
        {
            heartstoneBTN.interactable = true;
        }
        startPointBTN.interactable = true;
    }
    public void RespawnByHeartstone()
    {
        localPlayer.CmdRevive(closestHeartstone.transform.position - (closestHeartstone.transform.forward * 3), 0.2f);
        window.SetActive(false);
    }
    public void RespawnByStartPoint()
    {
        localPlayer.CmdRevive(FindObjectOfType<WorldGenerator>().globalStartingPoint.position, 0.2f);
        window.SetActive(false);
    }
}
