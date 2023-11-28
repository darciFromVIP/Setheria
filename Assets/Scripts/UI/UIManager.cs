using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public interface WindowedUI
{
    public void ShowWindow();
    public void HideWindow();
    public bool IsActive();
}
public class UIManager : MonoBehaviour
{
    public List<GameObject> windows = new();
    private List<WindowedUI> windowedUIs = new();
    public GameObject menuWindow;
    public GameObject dayNightSphere;
    public TextMeshProUGUI dayText;
    public DayNightCycleScriptable dayNightUI;

    public InputEnabledScriptable inputEnabled;
    private void Awake()
    {
        foreach (var item in windows)
        {
            windowedUIs.Add(item.GetComponent<WindowedUI>());
        }
        dayNightUI.sphere = dayNightSphere;
        dayNightUI.daysAliveText = dayText;
        inputEnabled.inputEnabled = true;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleWindows();
        }
    }
    public void ToggleWindows()
    {
        for (int i = 0; i < windowedUIs.Count; i++)
        {
            if (windowedUIs[i].IsActive())
            {
                windowedUIs[i].HideWindow();
                return;
            }
        }
        menuWindow.SetActive(!menuWindow.activeSelf);
        if (menuWindow.activeSelf)
            inputEnabled.inputEnabled = false;
        else
            inputEnabled.inputEnabled = true;
    }
}
