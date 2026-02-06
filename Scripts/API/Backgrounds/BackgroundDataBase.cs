using System.ComponentModel.DataAnnotations.Schema;
using Godot;

public static class BackgroundDataBase
{
    public static Godot.Collections.Dictionary<string, Texture2D> LoadedBackgrounds{get; private set;} = [];

    public static void Load()
    {
        LoadedBackgrounds.Clear();

        var packBg = ResourceLoader.Load<BackgroundLibrary>("res://Assets/Backgrounds/Backgrounds.tres");

        if(packBg == null)
            GD.PushError("[BackgroundDataBase] packBg not found");
        
        LoadedBackgrounds = packBg.BackgroundsStored;

        DebugService.Register("Bg in BBDD", () => LoadedBackgrounds.Count.ToString());
    }
}