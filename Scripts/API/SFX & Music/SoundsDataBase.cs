using Godot;
using Godot.Collections;

public static class SoundsDataBase
{
    public static Dictionary<string, AudioStream> LoadedSounds{get; private set;} = [];
    
    public static void Load()
    {
        LoadedSounds.Clear();

        var packSounds = ResourceLoader.Load<SoundsLibrary>("res://Audio/Sounds&Music.tres");

        if(packSounds == null)
            GD.PushError("[BackgroundDataBase] packBg not found");
        
        LoadedSounds = packSounds.SoundsStored;

        DebugService.Register("Sounds in BBDD", () => LoadedSounds.Count.ToString());
    }
}