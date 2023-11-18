using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System;
using UnityEngine.Events;
public struct KeyData
{
    public Sprite sprite;
    public string text;
}
[Serializable]
public class Settings
{
    public KeyCode move, target, interact, characterScreen, manual, inventory, cameraLock, passiveSkill, skill1, skill2, skill3, skill4, cameraUp, cameraDown, cameraLeft, cameraRight,
        activeItem1, activeItem2, activeItem3, activeItem4, activeItem5, activeItem6, activeItem7, activeItem8, cameraRotLeft, cameraRotRight;
    public float masterVolume, musicVolume, ambienceVolume, SFXVolume;
    public Settings()
    {
        move = KeyCode.Mouse1;
        target = KeyCode.Mouse0;
        interact = KeyCode.Mouse1;
        characterScreen = KeyCode.C;
        manual = KeyCode.N;
        inventory = KeyCode.I;
        cameraLock = KeyCode.Space;
        passiveSkill = KeyCode.Alpha1;
        skill1 = KeyCode.Alpha2;
        skill2 = KeyCode.Alpha3;
        skill3 = KeyCode.Alpha4;
        skill4 = KeyCode.Alpha5;
        cameraDown = KeyCode.S;
        cameraUp = KeyCode.W;
        cameraLeft = KeyCode.A;
        cameraRight = KeyCode.D;
        cameraRotLeft = KeyCode.Q;
        cameraRotRight = KeyCode.E;
        activeItem1 = KeyCode.F1;
        activeItem2 = KeyCode.F2;
        activeItem3 = KeyCode.F3;
        activeItem4 = KeyCode.F4;
        activeItem5 = KeyCode.F5;
        activeItem6 = KeyCode.F6;
        activeItem7 = KeyCode.F7;
        activeItem8 = KeyCode.F8;
        masterVolume = 1.0f;
        musicVolume = 1.0f;
        ambienceVolume = 1.0f;
        SFXVolume = 1.0f;
    }
}
public class SettingsManager : MonoBehaviour
{
    private string fullPath;

    public Settings settings;

    public Sprite key, space, enter, mouseLeft, mouseWheel, mouseRight;

    public UnityEvent<KeyCode> Key_Pressed = new();
    public UnityEvent Key_Changed = new();

    private void Awake()
    {
        settings = new();
        DontDestroyOnLoad(gameObject);
        fullPath = Application.persistentDataPath + "/User/Settings";
        var temp = LoadFile();
        if (temp == null)
            SaveFile();
        else
            settings = temp;
        ChangeMasterVolume(settings.masterVolume);
        ChangeMusicVolume(settings.musicVolume);
        ChangeAmbienceVolume(settings.ambienceVolume);
        ChangeSFXVolume(settings.SFXVolume);
    }
    public void SaveFile()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            string dataToStore = JsonConvert.SerializeObject(settings);
            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("Error occured while trying to save data to file: " + fullPath + "\n" + e);
        }
    }
    public Settings LoadFile()
    {
        if (File.Exists(fullPath))
        {
            try
            {
                string dataToLoad;
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }
                Debug.Log(dataToLoad);
                return JsonConvert.DeserializeObject<Settings>(dataToLoad);

            }
            catch (Exception e)
            {
                Debug.Log("Error occured while trying to load data from file: " + fullPath + "\n" + e);
                return null;
            }
        }
        return null;
    }
    private void OnGUI()
    {
        Event e = Event.current;
        if (e.isKey)
        {
            Key_Pressed.Invoke(e.keyCode);           // e.keyCode = keyCode, e.button = 0 = Left Mouse, 1 = Right Mouse, 2 = Middle Mouse
        }
        else if (e.isMouse)
        {
            if (e.button == 0)
                Key_Pressed.Invoke(KeyCode.Mouse0);
            else if (e.button == 1)
                Key_Pressed.Invoke(KeyCode.Mouse1);
            else if (e.button == 2)
                Key_Pressed.Invoke(KeyCode.Mouse2);
        }
    }
    public KeyData GetDataByKey(KeyCode key)
    {
        KeyData result = new();
        result.sprite = this.key;
        switch (key)
        {
            case KeyCode.Backspace:
                result.text = "Backspace";
                break;
            case KeyCode.Delete:
                result.text = "Delete";
                break;
            case KeyCode.Tab:
                result.text = "Tab";
                break;
            case KeyCode.Return:
                result.sprite = enter;
                result.text = "";
                break;
            case KeyCode.Pause:
                result.text = "Pause";
                break;
            case KeyCode.Escape:
                break;
            case KeyCode.Space:
                result.sprite = space;
                result.text = "";
                break;
            case KeyCode.Keypad0:
                result.text = "Num0";
                break;
            case KeyCode.Keypad1:
                result.text = "Num1";
                break;
            case KeyCode.Keypad2:
                result.text = "Num2";
                break;
            case KeyCode.Keypad3:
                result.text = "Num3";
                break;
            case KeyCode.Keypad4:
                result.text = "Num4";
                break;
            case KeyCode.Keypad5:
                result.text = "Num5";
                break;
            case KeyCode.Keypad6:
                result.text = "Num6";
                break;
            case KeyCode.Keypad7:
                result.text = "Num7";
                break;
            case KeyCode.Keypad8:
                result.text = "Num8";
                break;
            case KeyCode.Keypad9:
                result.text = "Num9";
                break;
            case KeyCode.KeypadPeriod:
                result.text = "Num.";
                break;
            case KeyCode.KeypadDivide:
                result.text = "Num/";
                break;
            case KeyCode.KeypadMultiply:
                result.text = "Num*";
                break;
            case KeyCode.KeypadMinus:
                result.text = "Num-";
                break;
            case KeyCode.KeypadPlus:
                result.text = "Num+";
                break;
            case KeyCode.KeypadEnter:
                result.text = "NumEnter";
                break;
            case KeyCode.KeypadEquals:
                result.text = "Num=";
                break;
            case KeyCode.UpArrow:
                result.text = "Up";
                break;
            case KeyCode.DownArrow:
                result.text = "Down";
                break;
            case KeyCode.RightArrow:
                result.text = "Right";
                break;
            case KeyCode.LeftArrow:
                result.text = "Left";
                break;
            case KeyCode.Insert:
                result.text = "Insert";
                break;
            case KeyCode.Home:
                result.text = "Home";
                break;
            case KeyCode.End:
                result.text = "End";
                break;
            case KeyCode.PageUp:
                result.text = "PgUp";
                break;
            case KeyCode.PageDown:
                result.text = "PgDown";
                break;
            case KeyCode.F1:
                result.text = "F1";
                break;
            case KeyCode.F2:
                result.text = "F2";
                break;
            case KeyCode.F3:
                result.text = "F3";
                break;
            case KeyCode.F4:
                result.text = "F4";
                break;
            case KeyCode.F5:
                result.text = "F5";
                break;
            case KeyCode.F6:
                result.text = "F6";
                break;
            case KeyCode.F7:
                result.text = "F7";
                break;
            case KeyCode.F8:
                result.text = "F8";
                break;
            case KeyCode.F9:
                result.text = "F9";
                break;
            case KeyCode.F10:
                result.text = "F10";
                break;
            case KeyCode.F11:
                result.text = "F11";
                break;
            case KeyCode.F12:
                result.text = "F12";
                break;
            case KeyCode.F13:
                result.text = "F13";
                break;
            case KeyCode.F14:
                result.text = "F14";
                break;
            case KeyCode.F15:
                result.text = "F15";
                break;
            case KeyCode.Alpha0:
                result.text = "0";
                break;
            case KeyCode.Alpha1:
                result.text = "1";
                break;
            case KeyCode.Alpha2:
                result.text = "2";
                break;
            case KeyCode.Alpha3:
                result.text = "3";
                break;
            case KeyCode.Alpha4:
                result.text = "4";
                break;
            case KeyCode.Alpha5:
                result.text = "5";
                break;
            case KeyCode.Alpha6:
                result.text = "6";
                break;
            case KeyCode.Alpha7:
                result.text = "7";
                break;
            case KeyCode.Alpha8:
                result.text = "8";
                break;
            case KeyCode.Alpha9:
                result.text = "9";
                break;
            case KeyCode.Quote:
                result.text = "'";
                break;
            case KeyCode.Comma:
                result.text = ",";
                break;
            case KeyCode.Minus:
                result.text = "-";
                break;
            case KeyCode.Period:
                result.text = ".";
                break;
            case KeyCode.Slash:
                result.text = "/";
                break;
            case KeyCode.Semicolon:
                result.text = ";";
                break;
            case KeyCode.Equals:
                result.text = "=";
                break;
            case KeyCode.LeftBracket:
                result.text = "[";
                break;
            case KeyCode.Backslash:
                result.text = "\\";
                break;
            case KeyCode.RightBracket:
                result.text = "]";
                break;
            case KeyCode.BackQuote:
                result.text = "`";
                break;
            case KeyCode.A:
                result.text = "A";
                break;
            case KeyCode.B:
                result.text = "B";
                break;
            case KeyCode.C:
                result.text = "C";
                break;
            case KeyCode.D:
                result.text = "D";
                break;
            case KeyCode.E:
                result.text = "E";
                break;
            case KeyCode.F:
                result.text = "F";
                break;
            case KeyCode.G:
                result.text = "G";
                break;
            case KeyCode.H:
                result.text = "H";
                break;
            case KeyCode.I:
                result.text = "I";
                break;
            case KeyCode.J:
                result.text = "J";
                break;
            case KeyCode.K:
                result.text = "K";
                break;
            case KeyCode.L:
                result.text = "L";
                break;
            case KeyCode.M:
                result.text = "M";
                break;
            case KeyCode.N:
                result.text = "N";
                break;
            case KeyCode.O:
                result.text = "O";
                break;
            case KeyCode.P:
                result.text = "P";
                break;
            case KeyCode.Q:
                result.text = "Q";
                break;
            case KeyCode.R:
                result.text = "R";
                break;
            case KeyCode.S:
                result.text = "S";
                break;
            case KeyCode.T:
                result.text = "T";
                break;
            case KeyCode.U:
                result.text = "U";
                break;
            case KeyCode.V:
                result.text = "V";
                break;
            case KeyCode.W:
                result.text = "W";
                break;
            case KeyCode.X:
                result.text = "X";
                break;
            case KeyCode.Y:
                result.text = "Y";
                break;
            case KeyCode.Z:
                result.text = "Z";
                break;
            case KeyCode.Numlock:
                result.text = "NumLock";
                break;
            case KeyCode.CapsLock:
                result.text = "CapsLock";
                break;
            case KeyCode.ScrollLock:
                result.text = "ScrollLock";
                break;
            case KeyCode.RightShift:
                result.text = "RightShift";
                break;
            case KeyCode.LeftShift:
                result.text = "LeftShift";
                break;
            case KeyCode.RightControl:
                result.text = "RightCtrl";
                break;
            case KeyCode.LeftControl:
                result.text = "LeftCtrl";
                break;
            case KeyCode.RightAlt:
                result.text = "RightAlt";
                break;
            case KeyCode.LeftAlt:
                result.text = "LeftAlt";
                break;
            case KeyCode.Print:
                result.text = "Print";
                break;
            case KeyCode.Mouse0:
                result.sprite = mouseLeft;
                result.text = "";
                break;
            case KeyCode.Mouse1:
                result.sprite = mouseRight;
                result.text = "";
                break;
            case KeyCode.Mouse2:
                result.sprite = mouseWheel;
                result.text = "";
                break;
            default:
                break;
        }
        return result;
    }
    public KeyData GetDataByKeybindType(KeybindType type)
    {
        switch (type)
        {
            case KeybindType.Move:
                return GetDataByKey(settings.move);
            case KeybindType.Target:
                return GetDataByKey(settings.target);
            case KeybindType.Interact:
                return GetDataByKey(settings.interact);
            case KeybindType.CharacterScreen:
                return GetDataByKey(settings.characterScreen);
            case KeybindType.Manual:
                return GetDataByKey(settings.manual);
            case KeybindType.Inventory:
                return GetDataByKey(settings.inventory);
            case KeybindType.CameraLock:
                return GetDataByKey(settings.cameraLock);
            case KeybindType.PassiveSkill:
                return GetDataByKey(settings.passiveSkill);
            case KeybindType.Skill1:
                return GetDataByKey(settings.skill1);
            case KeybindType.Skill2:
                return GetDataByKey(settings.skill2);
            case KeybindType.Skill3:
                return GetDataByKey(settings.skill3);
            case KeybindType.Skill4:
                return GetDataByKey(settings.skill4);
            case KeybindType.None:
                break;
            case KeybindType.CameraUp:
                return GetDataByKey(settings.cameraUp);
            case KeybindType.CameraDown:
                return GetDataByKey(settings.cameraDown);
            case KeybindType.CameraLeft:
                return GetDataByKey(settings.cameraLeft);
            case KeybindType.CameraRight:
                return GetDataByKey(settings.cameraRight);
            case KeybindType.ActiveItem1:
                return GetDataByKey(settings.activeItem1);
            case KeybindType.ActiveItem2:
                return GetDataByKey(settings.activeItem2);
            case KeybindType.ActiveItem3:
                return GetDataByKey(settings.activeItem3);
            case KeybindType.ActiveItem4:
                return GetDataByKey(settings.activeItem4);
            case KeybindType.ActiveItem5:
                return GetDataByKey(settings.activeItem5);
            case KeybindType.ActiveItem6:
                return GetDataByKey(settings.activeItem6);
            case KeybindType.ActiveItem7:
                return GetDataByKey(settings.activeItem7);
            case KeybindType.ActiveItem8:
                return GetDataByKey(settings.activeItem8);
            case KeybindType.CameraRotLeft:
                return GetDataByKey(settings.cameraRotLeft);
            case KeybindType.CameraRotRight:
                return GetDataByKey(settings.cameraRotRight);
            default:
                break;
        }
        return new KeyData();
    }
    public float GetDataByAudioSlider(AudioSliderType type)
    {
        switch (type)
        {
            case AudioSliderType.None:
                break;
            case AudioSliderType.MasterVolume:
                return settings.masterVolume;
            case AudioSliderType.MusicVolume:
                return settings.musicVolume;
            case AudioSliderType.AmbienceVolume:
                return settings.ambienceVolume;
            case AudioSliderType.SFXVolume:
                return settings.SFXVolume;
            default:
                break;
        }
        return 1;
    }
    public void ChangeMoveKey(KeyCode key)
    {
        settings.move = key;
        KeyChanged();
    }
    public void ChangeTargetKey(KeyCode key)
    {
        settings.target = key;
        KeyChanged();
    }
    public void ChangeInteractKey(KeyCode key)
    {
        settings.interact = key;
        KeyChanged();
    }
    public void ChangeCharacterKey(KeyCode key)
    {
        settings.characterScreen = key;
        KeyChanged();
    }
    public void ChangeManualKey(KeyCode key)
    {
        settings.manual = key;
        KeyChanged();
    }
    public void ChangeInventoryKey(KeyCode key)
    {
        settings.inventory = key;
        KeyChanged();
    }
    public void ChangeCameraLockKey(KeyCode key)
    {
        settings.cameraLock = key;
        KeyChanged();
    }
    public void ChangePassiveSkillKey(KeyCode key)
    {
        settings.passiveSkill = key;
        KeyChanged();
    }
    public void ChangeSkill1Key(KeyCode key)
    {
        settings.skill1 = key;
        KeyChanged();
    }
    public void ChangeSkill2Key(KeyCode key)
    {
        settings.skill2 = key;
        KeyChanged();
    }
    public void ChangeSkill3Key(KeyCode key)
    {
        settings.skill3 = key;
        KeyChanged();
    }
    public void ChangeSkill4Key(KeyCode key)
    {
        settings.skill4 = key;
        KeyChanged();
    }
    public void ChangeCameraUpKey(KeyCode key)
    {
        settings.cameraUp = key;
        KeyChanged();
    }
    public void ChangeCameraDownKey(KeyCode key)
    {
        settings.cameraDown = key;
        KeyChanged();
    }
    public void ChangeCameraLeftKey(KeyCode key)
    {
        settings.cameraLeft = key;
        KeyChanged();
    }
    public void ChangeCameraRightKey(KeyCode key)
    {
        settings.cameraRight = key;
        KeyChanged();
    }
    public void ChangeActiveItem1Key(KeyCode key)
    {
        settings.activeItem1 = key;
        KeyChanged();
    }
    public void ChangeActiveItem2Key(KeyCode key)
    {
        settings.activeItem2 = key;
        KeyChanged();
    }
    public void ChangeActiveItem3Key(KeyCode key)
    {
        settings.activeItem3 = key;
        KeyChanged();
    }
    public void ChangeActiveItem4Key(KeyCode key)
    {
        settings.activeItem4 = key;
        KeyChanged();
    }
    public void ChangeActiveItem5Key(KeyCode key)
    {
        settings.activeItem5 = key;
        KeyChanged();
    }
    public void ChangeActiveItem6Key(KeyCode key)
    {
        settings.activeItem6 = key;
        KeyChanged();
    }
    public void ChangeActiveItem7Key(KeyCode key)
    {
        settings.activeItem7 = key;
        KeyChanged();
    }
    public void ChangeActiveItem8Key(KeyCode key)
    {
        settings.activeItem8 = key;
        KeyChanged();
    }
    public void ChangeCameraRotLeftKey(KeyCode key)
    {
        settings.cameraRotLeft = key;
        KeyChanged();
    }
    public void ChangeCameraRotRightKey(KeyCode key)
    {
        settings.cameraRotRight = key;
        KeyChanged();
    }
    private void KeyChanged()
    {
        Key_Pressed.RemoveAllListeners();
        Key_Changed.Invoke();
        FindObjectOfType<ControlsWindow>().UpdateKeybinds();
        SaveFile();
    }
    private void VolumeChanged()
    {
        SaveFile();
    }
    public void ChangeMasterVolume(float value)
    {
        settings.masterVolume = value;
        FindObjectOfType<AudioManager>().ChangeMasterVolume(value);
        VolumeChanged();
    }
    public void ChangeMusicVolume(float value)
    {
        settings.musicVolume = value;
        FindObjectOfType<AudioManager>().ChangeMusicVolume(value);
        VolumeChanged();
    }
    public void ChangeAmbienceVolume(float value)
    {
        settings.ambienceVolume = value;
        FindObjectOfType<AudioManager>().ChangeAmbienceVolume(value);
        VolumeChanged();
    }
    public void ChangeSFXVolume(float value)
    {
        settings.SFXVolume = value;
        FindObjectOfType<AudioManager>().ChangeSFXVolume(value);
        VolumeChanged();
    }
}
