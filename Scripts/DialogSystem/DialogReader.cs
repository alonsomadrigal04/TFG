using Godot;
using System;
using System.Collections.Generic;

public class DialogReader
{
     readonly Dictionary<string, DialogLine> lines = [];
     readonly List<string> orderedUids = [];

    public Dictionary<string, DialogLine> LoadFromCSV(string path)
    {
        lines.Clear();
        orderedUids.Clear();

        if (!FileAccess.FileExists(path))
        {
            GD.PrintErr("File not found: " + path);
            return null;
        }

        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        file.GetLine(); // Skip header

        int index = 0;
        while (!file.EofReached())
        {
            string line = file.GetLine().Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            string[] parts = line.Split(';');
            if (parts.Length < 5)
            {
                GD.PrintErr($"[DialogReader] Invalid CSV format at line {index}: '{line}'");
                continue;
            }

            DialogLine dialogLine = new()
            {
                Uid = parts[0].Trim(),
                Type = parts[1].Trim(),
                Speaker = parts[1].Trim() == "say" ? CharacterDatabase.GetCharacter(parts[2].Trim()) : null,
                Text = parts[3].Trim(),
                Next = string.IsNullOrWhiteSpace(parts[4]) ? null : parts[4].Trim()
            };


            lines[dialogLine.Uid] = dialogLine;
            orderedUids.Add(dialogLine.Uid);
            index++;
        }

        DebugService.Register("Qty of lines", () => lines.Count.ToString());
        return lines;
    }

    public List<string> GetOrderedUids() => [.. orderedUids];
}
