using Godot;
using System.Collections.Generic;

public class CharacterDatabase
{
    private static readonly Dictionary<string, Character> characters = [];

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

        foreach (string key in data.Keys)
        {
            var entry = (Godot.Collections.Dictionary)data[key];
            var character = new Character(key, GD.Load<AudioStream>(entry.GetValueOrDefault("voice_sample", "").ToString()))
            {
                TextColor = new Color(entry.GetValueOrDefault("color", "#ffffff").ToString())
            };
            GD.Print(character.TextColor);
            RegisterCharacter(character);
        }

        GD.Print($"[CharacterDatabase] Loaded {characters.Count} characters.");
    }
}
