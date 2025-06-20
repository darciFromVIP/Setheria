using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PhotosynthesisButton : Button, NeedsLocalPlayerCharacter
{
    // These are only visible in Debug mode:
    public ItemScriptable plant;
    private PlayerCharacter localPlayer;
    public GameObject lockedImage, chosenFrame;
    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        (localPlayer.skills[1] as SPhotosynthesis).chosenPlant = plant;
        (localPlayer.skillInstances[1] as SPhotosynthesis).chosenPlant = plant;
        foreach (var item in FindObjectsOfType<PhotosynthesisButton>())
            item.chosenFrame.SetActive(false);
        chosenFrame.SetActive(true);
    }
    public void UnlockButton()
    {
        lockedImage.SetActive(false);
    }
    public void LockButton()
    {
        lockedImage.SetActive(true);
    }
    public void SetLocalPlayerCharacter(PlayerCharacter player)
    {
        localPlayer = player;
    }
}
