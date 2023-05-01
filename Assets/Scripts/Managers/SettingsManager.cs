using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System;
public struct KeyData
{
    public Sprite sprite;
    public string text;
}
[Serializable]
public class Settings
{
    public KeyCode move, target, interact, characterScreen, manual, inventory, cameraLock;
    public Settings()
    {
        move = KeyCode.Mouse1;
        target = KeyCode.Mouse0;
        interact = KeyCode.Mouse1;
        characterScreen = KeyCode.C;
        manual = KeyCode.N;
        inventory = KeyCode.I;
        cameraLock = KeyCode.Space;
    }
}
public class SettingsManager : MonoBehaviour
{
    private string fullPath;

    public Settings settings = new();

    public Sprite key, space, enter, mouseLeft, mouseWheel, mouseRight;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        fullPath = Application.persistentDataPath + "/User/Settings";
        var temp = LoadFile();
        if (temp == null)
            SaveFile();
        else
            settings = temp;
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
        if (e.isKey || e.isMouse)
        {
            Debug.Log("Detected key code: " + e.keyCode);           // e.keyCode = keyCode, e.button = 0 = Left Mouse, 1 = Right Mouse, 2 = Middle Mouse
        }
    }
    public KeyData GetDataByKey(KeyCode key)
    {
        KeyData result = new();
        switch (key)
        {
            case KeyCode.Backspace:
                result.sprite = this.key;
                result.text = "Backspace";
                break;
            case KeyCode.Delete:
                break;
            case KeyCode.Tab:
                break;
            case KeyCode.Return:
                break;
            case KeyCode.Pause:
                break;
            case KeyCode.Escape:
                break;
            case KeyCode.Space:
                break;
            case KeyCode.Keypad0:
                break;
            case KeyCode.Keypad1:
                break;
            case KeyCode.Keypad2:
                break;
            case KeyCode.Keypad3:
                break;
            case KeyCode.Keypad4:
                break;
            case KeyCode.Keypad5:
                break;
            case KeyCode.Keypad6:
                break;
            case KeyCode.Keypad7:
                break;
            case KeyCode.Keypad8:
                break;
            case KeyCode.Keypad9:
                break;
            case KeyCode.KeypadPeriod:
                break;
            case KeyCode.KeypadDivide:
                break;
            case KeyCode.KeypadMultiply:
                break;
            case KeyCode.KeypadMinus:
                break;
            case KeyCode.KeypadPlus:
                break;
            case KeyCode.KeypadEnter:
                break;
            case KeyCode.KeypadEquals:
                break;
            case KeyCode.UpArrow:
                break;
            case KeyCode.DownArrow:
                break;
            case KeyCode.RightArrow:
                break;
            case KeyCode.LeftArrow:
                break;
            case KeyCode.Insert:
                break;
            case KeyCode.Home:
                break;
            case KeyCode.End:
                break;
            case KeyCode.PageUp:
                break;
            case KeyCode.PageDown:
                break;
            case KeyCode.F1:
                break;
            case KeyCode.F2:
                break;
            case KeyCode.F3:
                break;
            case KeyCode.F4:
                break;
            case KeyCode.F5:
                break;
            case KeyCode.F6:
                break;
            case KeyCode.F7:
                break;
            case KeyCode.F8:
                break;
            case KeyCode.F9:
                break;
            case KeyCode.F10:
                break;
            case KeyCode.F11:
                break;
            case KeyCode.F12:
                break;
            case KeyCode.F13:
                break;
            case KeyCode.F14:
                break;
            case KeyCode.F15:
                break;
            case KeyCode.Alpha0:
                break;
            case KeyCode.Alpha1:
                break;
            case KeyCode.Alpha2:
                break;
            case KeyCode.Alpha3:
                break;
            case KeyCode.Alpha4:
                break;
            case KeyCode.Alpha5:
                break;
            case KeyCode.Alpha6:
                break;
            case KeyCode.Alpha7:
                break;
            case KeyCode.Alpha8:
                break;
            case KeyCode.Alpha9:
                break;
            case KeyCode.Quote:
                break;
            case KeyCode.Comma:
                break;
            case KeyCode.Minus:
                break;
            case KeyCode.Period:
                break;
            case KeyCode.Slash:
                break;
            case KeyCode.Semicolon:
                break;
            case KeyCode.Equals:
                break;
            case KeyCode.LeftBracket:
                break;
            case KeyCode.Backslash:
                break;
            case KeyCode.RightBracket:
                break;
            case KeyCode.BackQuote:
                break;
            case KeyCode.A:
                break;
            case KeyCode.B:
                break;
            case KeyCode.C:
                break;
            case KeyCode.D:
                break;
            case KeyCode.E:
                break;
            case KeyCode.F:
                break;
            case KeyCode.G:
                break;
            case KeyCode.H:
                break;
            case KeyCode.I:
                break;
            case KeyCode.J:
                break;
            case KeyCode.K:
                break;
            case KeyCode.L:
                break;
            case KeyCode.M:
                break;
            case KeyCode.N:
                break;
            case KeyCode.O:
                break;
            case KeyCode.P:
                break;
            case KeyCode.Q:
                break;
            case KeyCode.R:
                break;
            case KeyCode.S:
                break;
            case KeyCode.T:
                break;
            case KeyCode.U:
                break;
            case KeyCode.V:
                break;
            case KeyCode.W:
                break;
            case KeyCode.X:
                break;
            case KeyCode.Y:
                break;
            case KeyCode.Z:
                break;
            case KeyCode.Numlock:
                break;
            case KeyCode.CapsLock:
                break;
            case KeyCode.ScrollLock:
                break;
            case KeyCode.RightShift:
                break;
            case KeyCode.LeftShift:
                break;
            case KeyCode.RightControl:
                break;
            case KeyCode.LeftControl:
                break;
            case KeyCode.RightAlt:
                break;
            case KeyCode.LeftAlt:
                break;
            case KeyCode.Print:
                break;
            case KeyCode.Mouse0:
                break;
            case KeyCode.Mouse1:
                break;
            case KeyCode.Mouse2:
                break;
            default:
                break;
        }
        return result;
    }
    public KeyData GetMoveData()
    {
        return GetDataByKey(settings.move);
    }
    public void ChangeMoveKey(KeyCode key)
    {
        settings.move = key;
    }
    public void ChangeTargetKey(KeyCode key)
    {
        settings.target = key;
    }
    public void ChangeInteractKey(KeyCode key)
    {
        settings.interact = key;
    }
    public void ChangeCharacterKey(KeyCode key)
    {
        settings.characterScreen = key;
    }
    public void ChangeManualKey(KeyCode key)
    {
        settings.manual = key;
    }
    public void ChangeInventoryKey(KeyCode key)
    {
        settings.inventory = key;
    }
    public void ChangeCameraLockKey(KeyCode key)
    {
        settings.cameraLock = key;
    }
}
