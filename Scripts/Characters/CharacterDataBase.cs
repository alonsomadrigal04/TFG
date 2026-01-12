using Godot;
using System.Collections.Generic;
using System.Linq;

public class CharacterDatabase
{
    static readonly Dictionary<string, Character> characters = [];

    public static void RegisterCharacter(Character character)
    {
        characters[character.Name] = character;
    }

    public static Character GetCharacter(string name)
    {
        if (characters.TryGetValue(name, out var character))
            return character;

        GD.PrintErr($"[CharacterDatabase] Character not found: {name}");
        return null;
    }

    public static void LoadFromJson(string path)
    {
        if (!FileAccess.FileExists(path))
        {
            GD.PrintErr($"[CharacterDatabase] File not found: {path}");
            return;
        }

        string json = FileAccess.GetFileAsString(path);
        var data = Json.ParseString(json).AsGodotDictionary();

        foreach (string key in data.Keys.Select(v => (string)v))
        {
            var entry = (Godot.Collections.Dictionary)data[key];
            var portraitSet = ResourceLoader.Load<CharacterPortraitSet>($"res://Assets/Portraits/{key}/{key}Portraits.tres");
            var character = new Character(key)
            {
                VoiceSample = GD.Load<AudioStream>(entry.GetValueOrDefault("voice_sample", "").ToString()),
                TextColor = new Color(entry.GetValueOrDefault("color", "#ffffff").ToString()),
                Portraits =  portraitSet?.Portraits ?? []
            };
            
            RegisterCharacter(character);
        }


        DebugService.Register("Qty characters in BBDD", () => characters.Count.ToString());
    }
}
