using Godot;
using Godot.Collections;

public static class BackgroundDataBase
{
    public static Dictionary<string, Texture2D> LoadedBackgrounds{get; private set;} = [];

    public static void Load()
    {
        LoadedBackgrounds.Clear();

        var packBg = ResourceLoader.Load<BackgroundLibrary>("res://Data/Backgrounds/Backgrounds.tres");

        if(packBg == null)
            GD.PushError("[BackgroundDataBase] packBg not found");
        
        LoadedBackgrounds = packBg.BackgroundsStored;

        DebugService.Register("Bg in BBDD", () => LoadedBackgrounds.Count.ToString());
    }
}