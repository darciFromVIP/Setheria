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
    
    private Queue<TutorialDataScriptable> tutorialDataStack = new();
    private bool isCoroutineRunning = false;

    private bool isTutorialDisabled = false;

    public void ToggleTutorial(bool value)
    {
        isTutorialDisabled = value;
    }
    public bool IsTutorialDisabled()
    {
        return isTutorialDisabled;
    }
    public void QueueNewTutorial(TutorialDataScriptable tutData)
    {
        if (isTutorialDisabled)
            return;
        if (window.activeSelf || tutorialDataStack.Count > 0)
        {
            tutorialDataStack.Enqueue(tutData);
            if (!isCoroutineRunning)
                StartCoroutine(DelayedSetNewWindow());
            return;
        }
        SetNewTutorialWindow(tutData);
    }
    private void SetNewTutorialWindow(TutorialDataScriptable tutData)
    {
        if (isTutorialDisabled)
            return;
        label.text = tutData.label;
        description.text = tutData.description;
        window.SetActive(true);
        gifAnimator.Play(tutData.gifAnim.name);
    }
    private IEnumerator DelayedSetNewWindow()
    {
        isCoroutineRunning = true;
        while (window.activeSelf)
        {
            yield return null;
        }
        isCoroutineRunning = false;
        SetNewTutorialWindow(tutorialDataStack.Dequeue());
        if (tutorialDataStack.Count > 0)
        {
            StartCoroutine(DelayedSetNewWindow());
        }
    }
}
