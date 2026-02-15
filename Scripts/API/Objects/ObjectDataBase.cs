using Godot;
using Godot.Collections;

public static class ObjectDataBase
{
    public static Dictionary<string, Texture2D> LoadedSpecialItems{get; private set;} = [];
    public static void Load()
    {
        LoadedSpecialItems.Clear();
        var packItems = ResourceLoader.Load<SpecialItemsLibrary>("res://Data/Objects/SpecialItems.tres");

        if(packItems == null)
            GD.PushError("[ObjectDataBase] packItems not found");
        
        LoadedSpecialItems = packItems.Itemstored;

        DebugService.Register("Items in BBDD", () => LoadedSpecialItems.Count.ToString());
    }
    
}