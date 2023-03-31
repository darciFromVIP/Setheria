using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HeroButton : MonoBehaviour, NeedsLocalPlayer
{
    [SerializeField] private Hero hero;
    private HeroSelect heroSelect;
    private HeroDescription heroDescription;
    private bool isGettingData;
    private ClientObject localPlayer;
    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(HeroSelected);
        heroSelect = GetComponentInParent<HeroSelect>();
        heroDescription = FindObjectOfType<HeroDescription>(true);
    }
    public void OnMouseOver()
    {
        if (!isGettingData)
            StartCoroutine(UpdateHeroDescription());
    }
    private IEnumerator UpdateHeroDescription()
    {
        isGettingData = true;
        while (localPlayer.GetSaveData().Count == 0)
            yield return null;
        foreach (var item in localPlayer.GetSaveData())
        {
            if (item.hero == hero)
            {
                heroDescription.UpdateDescription(item);
            }
        }
        isGettingData = false;
    }
    private void HeroSelected()
    {
        heroSelect.SpawnPlayer(hero);
    }

    public void SetLocalPlayer(ClientObject player)
    {
        localPlayer = player;
    }
}
