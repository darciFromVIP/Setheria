using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class WorkProgressBar : MonoBehaviour, NeedsLocalPlayerCharacter
{
    public Slider progressBar;

    private PlayerController localPlayer;
    private void Start()
    {
        PlayerController player = GetComponentInParent<PlayerController>();
        if (player)
        {
            localPlayer = player;
            localPlayer.Working_Event.AddListener(UpdateBar);
        }
    }
    public void SetLocalPlayerCharacter(PlayerCharacter player)
    {
        if (localPlayer)
            return;
        localPlayer = player.GetComponent<PlayerController>();
        localPlayer.Working_Event.AddListener(UpdateBar);
    }
    private void UpdateBar(float value)
    {
        if (!progressBar.gameObject.activeSelf)
        {
            progressBar.gameObject.SetActive(true);
            progressBar.maxValue = value;
        }
        if (value <= 0)
            progressBar.gameObject.SetActive(false);
        progressBar.value = value;
    }
}
