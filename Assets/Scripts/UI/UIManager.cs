using System.Collections;
using System.Collections.Generic;
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
    private void Awake()
    {
        foreach (var item in windows)
        {
            windowedUIs.Add(item.GetComponent<WindowedUI>());
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
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
        }
    }
}
