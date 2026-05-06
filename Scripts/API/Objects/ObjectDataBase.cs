using System;
using System.Reflection.Metadata.Ecma335;
using Godot;
using Godot.Collections;

public static class ObjectDataBase
{
    public static Dictionary<string, ObjectData> LoadedSpecialItems{get; private set;} = [];
    public static System.Collections.Generic.List<ObjectData> PlayerInventory { get; private set; } = new();
    public static void Load()
    {
        LoadedSpecialItems.Clear();
        ObjectsLibrary packItems = ResourceLoader.Load<ObjectsLibrary>("res://Data/Objects/Objects.tres");

        if(packItems == null)
            GD.PushError("[ObjectDataBase] packItems not found");
        
        LoadedSpecialItems = packItems.Itemstored;

        AddObjectByName("pipa");

        DebugService.Register("Items in BBDD", () => LoadedSpecialItems.Count.ToString());
    }
    public static void AddObjectToInventory(ObjectData newObject) 
    {
        GD.Print($"{newObject} has been added to the inventory.");
        PlayerInventory.Add(newObject);
    }

    public static void AddObjectByName(string objectname)
    {
        if(!LoadedSpecialItems.TryGetValue(objectname, out ObjectData objectData))
            GD.PrintErr($"[ObjectDataBase] {objectname} not in the BBDD");
        AddObjectToInventory(objectData);
    }
    public static ObjectData GetObject(string name)
    {
        LoadedSpecialItems.TryGetValue(name, out ObjectData objectData);
        return objectData;
    }

    public static int GetInventoryLength() => PlayerInventory.Count;

}