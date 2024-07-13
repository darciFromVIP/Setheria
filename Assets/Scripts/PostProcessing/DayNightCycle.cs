using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;
using UnityEngine.Events;
using Mirror;

public class DayNightCycle : NetworkBehaviour, ISaveable
{
    public Light directionalLight;
    public Volume volume;
    public DayNightCycleScriptable uiData;
    public List<PostProcessingDataScriptable> data = new();

    [SyncVar][SerializeField] public int daysAlive = 1;
    [SyncVar][SerializeField] private float timer = 0;
    [SyncVar][SerializeField] private float progressPercentage = 0;
    [SyncVar][SerializeField] private int currentIndex = 0;
    private int maxIndex;

    private int multiplier = 1;

    public UnityEvent Night_Started = new();
    public UnityEvent Day_Started = new();

    public void LoadState(SaveDataWorldObject state)
    {
        daysAlive = state.intData1;
        currentIndex = state.intData2;
        timer = state.floatData1;
        progressPercentage = state.floatData2;
        uiData.daysAliveText.text = "Day " + daysAlive;
    }

    public SaveDataWorldObject SaveState()
    {
        return new SaveDataWorldObject
        {
            intData1 = daysAlive,
            intData2 = currentIndex,
            floatData1 = timer,
            floatData2 = progressPercentage
        };
    }

    private void Start()
    {
        maxIndex = data.Count - 1;
        daysAlive = 1;
        FindObjectOfType<AudioManager>().SetDayNightCycle(this);
    }
    public bool IsNight()
    {
        return currentIndex >= 3;
    }
    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.L))
        {
            multiplier = 100;
        }
        if (Input.GetKeyUp(KeyCode.L))
        {
            multiplier = 1;
        }
#endif
        if (uiData.sphere == null || uiData.daysAliveAnimatedText == null)
            return;
        if (timer >= data[currentIndex].timeStamp)
        {
            if (currentIndex == maxIndex)
            {
                currentIndex = 0;
                timer = 0;
                daysAlive++;
                uiData.daysAliveAnimatedText.transform.localScale = new Vector3(1, 1, 1);
                uiData.daysAliveAnimatedText.text = "Day " + daysAlive;
                uiData.daysAliveAnimatedText.GetComponent<Animator>().SetTrigger("FadeInAndOut");
                uiData.daysAliveText.text = "Day " + daysAlive;
            }
            else
                currentIndex++;

            if (volume.profile.TryGet(out Tonemapping tonemapping))
            {
                tonemapping.mode.Override(data[currentIndex].mode);
            }
            progressPercentage = 0;

            if (currentIndex == 0)
            {
                FindObjectOfType<AudioManager>().ChangeAmbienceParameter(AmbienceParameter.Day);
                Day_Started.Invoke();
                if (isServer)
                    FindObjectOfType<SaveLoadSystem>().Save();
            }
            else if (currentIndex == 3)
            {
                FindObjectOfType<AudioManager>().ChangeAmbienceParameter(AmbienceParameter.Night);
                Night_Started.Invoke();
                uiData.daysAliveAnimatedText.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
                uiData.daysAliveAnimatedText.text = "Night Falls!";
                uiData.daysAliveAnimatedText.GetComponent<Animator>().SetTrigger("FadeInAndOut");
                if (isServer)
                    FindObjectOfType<SaveLoadSystem>().Save();
            }
        }
        else
        {
            directionalLight.color = Color.Lerp(currentIndex == 0 ? data[maxIndex].color : data[currentIndex - 1].color, data[currentIndex].color, progressPercentage);
            directionalLight.intensity = Mathf.Lerp(currentIndex == 0 ? data[maxIndex].intensity : data[currentIndex - 1].intensity, data[currentIndex].intensity, progressPercentage);
            uiData.sphere.transform.rotation = Quaternion.Lerp(Quaternion.Euler(currentIndex == 0 ? data[maxIndex].rotation : data[currentIndex - 1].rotation), Quaternion.Euler(data[currentIndex].rotation), progressPercentage);
            
            if (volume.profile.TryGet(out ColorAdjustments color))
            {
                color.postExposure.Override(Mathf.Lerp(currentIndex == 0 ? data[maxIndex].postExposure : data[currentIndex - 1].postExposure, data[currentIndex].postExposure, progressPercentage));
                color.contrast.Override(Mathf.Lerp(currentIndex == 0 ? data[maxIndex].contrast : data[currentIndex - 1].contrast, data[currentIndex].contrast, progressPercentage));
                color.colorFilter.Override(Color.Lerp(currentIndex == 0 ? data[maxIndex].colorFilter : data[currentIndex - 1].colorFilter, data[currentIndex].colorFilter, progressPercentage));
                color.hueShift.Override(Mathf.Lerp(currentIndex == 0 ? data[maxIndex].hueShift : data[currentIndex - 1].hueShift, data[currentIndex].hueShift, progressPercentage));
                color.saturation.Override(Mathf.Lerp(currentIndex == 0 ? data[maxIndex].saturation : data[currentIndex - 1].saturation, data[currentIndex].saturation, progressPercentage));
            }
            if (volume.profile.TryGet(out LiftGammaGain gamma))
            {
                gamma.lift.Override(Vector4.Lerp(currentIndex == 0 ? data[maxIndex].lift : data[currentIndex - 1].lift, data[currentIndex].lift, progressPercentage));
                gamma.gamma.Override(Vector4.Lerp(currentIndex == 0 ? data[maxIndex].gamma : data[currentIndex - 1].gamma, data[currentIndex].gamma, progressPercentage));
                gamma.gain.Override(Vector4.Lerp(currentIndex == 0 ? data[maxIndex].gain : data[currentIndex - 1].gain, data[currentIndex].gain, progressPercentage));
            }

            if (isServer)
            {
                timer += multiplier * Time.deltaTime;
                progressPercentage += multiplier * Time.deltaTime / (currentIndex == 0 ? data[currentIndex].timeStamp : data[currentIndex].timeStamp - data[currentIndex - 1].timeStamp);
            }
        }
    }
}
