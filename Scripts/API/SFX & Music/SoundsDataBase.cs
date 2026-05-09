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

    public static AudioStream GetSound(string audioName)
    {
        if(LoadedSounds.TryGetValue(audioName, out AudioStream sound))
        {
            return sound;
        }
        return null;
    }


    public static void TryPlay(string audioName, bool music = false)
    {
        if(LoadedSounds.TryGetValue(audioName, out AudioStream sound))
        {
            if(!music)
                AudioManager.Instance.PlaySfx(sound);
            else
                AudioManager.Instance.PlayMusic(sound);
        }
        else
        {
            GD.Print(audioName + " sound was not found");
        }
    }


}