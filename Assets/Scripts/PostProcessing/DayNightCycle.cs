using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;
public class DayNightCycle : MonoBehaviour
{
    public Light directionalLight;
    public Volume volume;
    public DayNightCycleScriptable uiData;
    public List<PostProcessingDataScriptable> data = new();

    private int daysAlive = 0;
    private float timer = 0;
    private float progressPercentage = 0;
    private int currentIndex = 0;
    private int maxIndex;

    private int multiplier = 1;
    private void Start()
    {
        maxIndex = data.Count - 1;
    }
    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.L))
        {
            multiplier = 20;
        }
        if (Input.GetKeyUp(KeyCode.L))
        {
            multiplier = 1;
        }
#endif
        if (timer >= data[currentIndex].timeStamp)
        {
            if (currentIndex == maxIndex)
            {
                currentIndex = 0;
                timer = 0;
                daysAlive++;
                uiData.daysAliveText.text = "Day " + daysAlive;
                uiData.daysAliveText.GetComponent<Animator>().SetTrigger("FadeInAndOut");
            }
            else
                currentIndex++;
            if (currentIndex == 0)
                FindObjectOfType<AudioManager>().ChangeAmbienceParameter(AmbienceParameter.Day);
            else if (currentIndex == 2)
                FindObjectOfType<AudioManager>().ChangeAmbienceParameter(AmbienceParameter.Night);

            if (volume.profile.TryGet(out Tonemapping tonemapping))
            {
                tonemapping.mode.Override(data[currentIndex].mode);
            }
            progressPercentage = 0;
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

            timer += multiplier * Time.deltaTime;
            progressPercentage += multiplier * Time.deltaTime / (currentIndex == 0 ? data[currentIndex].timeStamp : data[currentIndex].timeStamp - data[currentIndex - 1].timeStamp);
        }
    }
}
