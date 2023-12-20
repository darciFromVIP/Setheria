using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TentButton : MonoBehaviour
{
    public Button btn;
    private Structure currentTent;
    public void ShowBTN(Structure tent)
    {
        btn.gameObject.SetActive(true);
        currentTent = tent;
        btn.onClick.AddListener(ButtonPressed);
    }
    private void ButtonPressed()
    {
        currentTent.Interact();
    }
    public void HideBTN()
    {
        currentTent = null;
        btn.onClick.RemoveAllListeners();
        btn.gameObject.SetActive(false);
    }
}
