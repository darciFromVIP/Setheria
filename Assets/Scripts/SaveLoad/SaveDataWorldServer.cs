using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SaveDataWorldServer
{
    public bool fresh = false;
    public SaveDataWorld worldSaveData = new();
    public Dictionary<string, Dictionary<string, SaveDataWorldObject>> worldObjects = new();

    public SaveDataWorldServer() { }
    public SaveDataWorldServer(string name)
    {
        worldSaveData.worldName = name;
    }
}

public static class DictionaryReadWrite
{
    public static void WriteDictionaryStringSaveDataWorldObject(this NetworkWriter writer, Dictionary<string, SaveDataWorldObject> value)
    {
        List<SaveDataWorldSimple> list = new();
        foreach (var item in value)
        {
            var temp = new SaveDataWorldSimple();
            temp.id = item.Key;
            temp.name = item.Value.name;
            temp.positionX = item.Value.positionX;
            temp.positionY = item.Value.positionY;
            temp.positionZ = item.Value.positionZ;
            temp.rotationX = item.Value.rotationX;
            temp.rotationY = item.Value.rotationY;
            temp.rotationZ = item.Value.rotationZ;
            temp.rotationW = item.Value.rotationW;
            temp.floatData1 = item.Value.floatData1;
            temp.floatData2 = item.Value.floatData2;
            temp.floatData3 = item.Value.floatData3;
            temp.intData1 = item.Value.intData1;
            temp.intData2 = item.Value.intData2;
            temp.boolData1 = item.Value.boolData1;
            list.Add(temp);
        }
        writer.WriteList(list);
    }

    public static Dictionary<string, SaveDataWorldObject> ReadDictionaryStringSaveDataWorldObject(this NetworkReader reader)
    {
        var list = reader.ReadList<SaveDataWorldSimple>();
        var dic = new Dictionary<string, SaveDataWorldObject>();
        foreach (var item in list)
        {
            var data = new SaveDataWorldObject();
            data.name = item.name;
            data.positionX = item.positionX;
            data.positionY = item.positionY;
            data.positionZ = item.positionZ;    
            data.rotationX = item.rotationX;    
            data.rotationY = item.rotationY;
            data.rotationZ = item.rotationZ;
            data.rotationW = item.rotationW;
            data.floatData1 = item.floatData1;
            data.floatData2 = item.floatData2;
            data.floatData3 = item.floatData3;
            data.intData1 = item.intData1;
            data.intData2 = item.intData2;
            data.boolData1 = item.boolData1;
            dic.Add(item.id, data);
        }
        return dic;
    }
    public static void WriteDictionaryStringDictionaryStringSaveDataWorldObject(this NetworkWriter writer, Dictionary<string, Dictionary<string, SaveDataWorldObject>> value)
    {
        List<WorldObjectsSimple> list = new();
        foreach (var item in value)
        {
            var worldObjects = new WorldObjectsSimple();
            worldObjects.id = item.Key;

            var list2 = new List<SaveDataWorldSimple>();
            foreach (var item2 in item.Value)
            {
                var temp = new SaveDataWorldSimple();
                temp.id = item2.Key;
                temp.name = item2.Value.name;
                temp.positionX = item2.Value.positionX;
                temp.positionY = item2.Value.positionY;
                temp.positionZ = item2.Value.positionZ;
                temp.rotationX = item2.Value.rotationX;
                temp.rotationY = item2.Value.rotationY;
                temp.rotationZ = item2.Value.rotationZ;
                temp.rotationW = item2.Value.rotationW;
                temp.floatData1 = item2.Value.floatData1;
                temp.floatData2 = item2.Value.floatData2;
                temp.floatData3 = item2.Value.floatData3;
                temp.intData1 = item2.Value.intData1;
                temp.intData2 = item2.Value.intData2;
                temp.boolData1 = item2.Value.boolData1;
                list2.Add(temp);
            }
            worldObjects.data = list2;
            list.Add(worldObjects);
        }
        writer.WriteList(list);
    }

    public static Dictionary<string, Dictionary<string, SaveDataWorldObject>> ReadDictionaryStringDictionaryStringSaveDataWorldObject(this NetworkReader reader)
    {
        var list = reader.ReadList<WorldObjectsSimple>();
        var dic = new Dictionary<string, Dictionary<string, SaveDataWorldObject>>();
        foreach (var item in list)
        {
            var dic2 = new Dictionary<string, SaveDataWorldObject>();
            foreach (var item2 in item.data)
            {
                var data = new SaveDataWorldObject();
                data.name = item2.name;
                data.positionX = item2.positionX;
                data.positionY = item2.positionY;
                data.positionZ = item2.positionZ;
                data.rotationX = item2.rotationX;
                data.rotationY = item2.rotationY;
                data.rotationZ = item2.rotationZ;
                data.rotationW = item2.rotationW;
                data.floatData1 = item2.floatData1;
                data.floatData2 = item2.floatData2;
                data.floatData3 = item2.floatData3;
                data.intData1 = item2.intData1;
                data.intData2 = item2.intData2;
                data.boolData1 = item2.boolData1;
                dic2.Add(item2.id, data);
            }
            dic.Add(item.id, dic2);
        }
        return dic;
    }
    public static void WriteWorldObjectsSimple(this NetworkWriter writer, WorldObjectsSimple value)
    {
        writer.WriteString(value.id);
        writer.WriteList(value.data);
    }
    public static WorldObjectsSimple ReadWorldObjectsSimple(this NetworkReader reader)
    {
        var result = new WorldObjectsSimple();
        result.id = reader.ReadString();
        result.data = reader.ReadList<SaveDataWorldSimple>();
        return result;
    }
    public static void WriteSaveDataWorldSimple(this NetworkWriter writer, SaveDataWorldSimple value)
    {
        writer.WriteString(value.id);
        writer.WriteString(value.name);
        writer.WriteFloat(value.positionX);
        writer.WriteFloat(value.positionY);
        writer.WriteFloat(value.positionZ);
        writer.WriteFloat(value.rotationX);
        writer.WriteFloat(value.rotationY);
        writer.WriteFloat(value.rotationZ);
        writer.WriteFloat(value.rotationW);
        writer.WriteFloat(value.floatData1);
        writer.WriteFloat(value.floatData2);
        writer.WriteFloat(value.floatData3);
        writer.WriteInt(value.intData1);
        writer.WriteInt(value.intData2);
        writer.WriteBool(value.boolData1);
    }
    public static SaveDataWorldSimple ReadSaveDataWorldSimple(this NetworkReader reader)
    {
        var value = new SaveDataWorldSimple();
        value.id = reader.ReadString();
        value.name = reader.ReadString();
        value.positionX = reader.ReadFloat();
        value.positionY = reader.ReadFloat();
        value.positionZ = reader.ReadFloat();
        value.rotationX = reader.ReadFloat();
        value.rotationY = reader.ReadFloat();
        value.rotationZ = reader.ReadFloat();
        value.rotationW = reader.ReadFloat();
        value.floatData1 = reader.ReadFloat();
        value.floatData2 = reader.ReadFloat();
        value.floatData3 = reader.ReadFloat();
        value.intData1 = reader.ReadInt();
        value.intData2 = reader.ReadInt();
        value.boolData1 = reader.ReadBool();
        return value;
    }
}
[Serializable]
public class SaveDataWorldSimple
{
    public string id;
    public string name;
    public float positionX;
    public float positionY;
    public float positionZ;
    public float rotationX;
    public float rotationY;
    public float rotationZ;
    public float rotationW;
    public int intData1, intData2;
    public float floatData1, floatData2, floatData3;
    public bool boolData1;
}
[Serializable]
public class WorldObjectsSimple
{
    public string id;
    public List<SaveDataWorldSimple> data;
}
