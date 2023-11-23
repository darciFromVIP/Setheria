using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    public GameObject window;
    public TextMeshProUGUI label, description;
    public Animator gifAnimator;

    private bool isTutorialDisabled = false;

    public void ToggleTutorial(bool value)
    {
        isTutorialDisabled = value;
    }
    public bool IsTutorialDisabled()
    {
        return isTutorialDisabled;
    }
    public void SetNewTutorialWindow(TutorialDataScriptable tutData)
    {
        if (isTutorialDisabled)
            return;
        label.text = tutData.label;
        description.text = tutData.description;
        gifAnimator.Play(tutData.gifAnim.name);
        window.SetActive(true);
    }
}
