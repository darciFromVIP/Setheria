using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class TutorialArea : MonoBehaviour
{
    public string textBeforeKey, textAfterKey;
    public KeybindType keybind;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerCharacter character))
        {
            var key = FindObjectOfType<SettingsManager>().GetDataByKeybindType(keybind);
            FindObjectOfType<SystemMessages>().AddMessage(textBeforeKey + key.text + textAfterKey, MsgType.Notice);
            Destroy(gameObject);
        }
    }
}
