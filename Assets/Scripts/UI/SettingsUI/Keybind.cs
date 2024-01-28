using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public enum KeybindType
{
    None, Move, Target, Interact, CharacterScreen, Manual, Inventory, CameraLock, PassiveSkill, Skill1, Skill2, Skill3, Skill4, CameraUp, CameraDown, CameraLeft, CameraRight, 
    ActiveItem1, ActiveItem2, ActiveItem3, ActiveItem4, ActiveItem5, ActiveItem6, ActiveItem7, ActiveItem8, CameraRotLeft, CameraRotRight, Map
}
public class Keybind : MonoBehaviour
{
    public KeybindType keybindType;
    public Image img;
    public TextMeshProUGUI text;

    private SettingsManager settingsManager;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(ChangeThisKeybind);
        settingsManager = FindObjectOfType<SettingsManager>(true);
    }
    public void UpdateKeybind(KeyData data)
    {
        img.sprite = data.sprite;
        text.text = data.text;
    }
    private void ChangeThisKeybind()
    {
        switch (keybindType)
        {
            case KeybindType.Move:
                settingsManager.Key_Pressed.AddListener(settingsManager.ChangeMoveKey);
                break;
            case KeybindType.Target:
                settingsManager.Key_Pressed.AddListener(settingsManager.ChangeTargetKey);
                break;
            case KeybindType.Interact:
                settingsManager.Key_Pressed.AddListener(settingsManager.ChangeInteractKey);
                break;
            case KeybindType.CharacterScreen:
                settingsManager.Key_Pressed.AddListener(settingsManager.ChangeCharacterKey);
                break;
            case KeybindType.Manual:
                settingsManager.Key_Pressed.AddListener(settingsManager.ChangeManualKey);
                break;
            case KeybindType.Inventory:
                settingsManager.Key_Pressed.AddListener(settingsManager.ChangeInventoryKey);
                break;
            case KeybindType.CameraLock:
                settingsManager.Key_Pressed.AddListener(settingsManager.ChangeCameraLockKey);
                break;
            case KeybindType.PassiveSkill:
                settingsManager.Key_Pressed.AddListener(settingsManager.ChangePassiveSkillKey);
                break;
            case KeybindType.Skill1:
                settingsManager.Key_Pressed.AddListener(settingsManager.ChangeSkill1Key);
                break;
            case KeybindType.Skill2:
                settingsManager.Key_Pressed.AddListener(settingsManager.ChangeSkill2Key);
                break;
            case KeybindType.Skill3:
                settingsManager.Key_Pressed.AddListener(settingsManager.ChangeSkill3Key);
                break;
            case KeybindType.Skill4:
                settingsManager.Key_Pressed.AddListener(settingsManager.ChangeSkill4Key);
                break;
            case KeybindType.None:
                break;
            case KeybindType.CameraUp:
                settingsManager.Key_Pressed.AddListener(settingsManager.ChangeCameraUpKey);
                break;
            case KeybindType.CameraDown:
                settingsManager.Key_Pressed.AddListener(settingsManager.ChangeCameraDownKey);
                break;
            case KeybindType.CameraLeft:
                settingsManager.Key_Pressed.AddListener(settingsManager.ChangeCameraLeftKey);
                break;
            case KeybindType.CameraRight:
                settingsManager.Key_Pressed.AddListener(settingsManager.ChangeCameraRightKey);
                break;
            case KeybindType.ActiveItem1:
                settingsManager.Key_Pressed.AddListener(settingsManager.ChangeActiveItem1Key);
                break;
            case KeybindType.ActiveItem2:
                settingsManager.Key_Pressed.AddListener(settingsManager.ChangeActiveItem2Key);
                break;
            case KeybindType.ActiveItem3:
                settingsManager.Key_Pressed.AddListener(settingsManager.ChangeActiveItem3Key);
                break;
            case KeybindType.ActiveItem4:
                settingsManager.Key_Pressed.AddListener(settingsManager.ChangeActiveItem4Key);
                break;
            case KeybindType.ActiveItem5:
                settingsManager.Key_Pressed.AddListener(settingsManager.ChangeActiveItem5Key);
                break;
            case KeybindType.ActiveItem6:
                settingsManager.Key_Pressed.AddListener(settingsManager.ChangeActiveItem6Key);
                break;
            case KeybindType.ActiveItem7:
                settingsManager.Key_Pressed.AddListener(settingsManager.ChangeActiveItem7Key);
                break;
            case KeybindType.ActiveItem8:
                settingsManager.Key_Pressed.AddListener(settingsManager.ChangeActiveItem8Key);
                break;
            case KeybindType.CameraRotLeft:
                settingsManager.Key_Pressed.AddListener(settingsManager.ChangeCameraRotLeftKey);
                break;
            case KeybindType.CameraRotRight:
                settingsManager.Key_Pressed.AddListener(settingsManager.ChangeCameraRotRightKey);
                break;
            case KeybindType.Map:
                settingsManager.Key_Pressed.AddListener(settingsManager.ChangeMapKey);
                break;
            default:
                break;
        }
    }
}
