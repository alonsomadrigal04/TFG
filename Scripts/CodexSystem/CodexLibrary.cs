using Godot;
using System.Collections.Generic;

public class CodexLibrary
{
    private const string CodexDataPath  = "res://Data/Codex/";
    private const string PortraitsPath  = "res://Data/Codex/Portraits/";
    private const string SupportImgPath = "res://Data/Codex/Images/";
    private const string ImgExtension   = ".png";

    public List<CoreInformation> Characters     { get; } = [];
    public List<CoreInformation> Terms          { get; } = [];
    public List<CoreInformation> Localizations  { get; } = [];

    public void LoadAll()
    {
        Characters.Clear();
        Terms.Clear();
        Localizations.Clear();

        using var dir = DirAccess.Open(CodexDataPath);
        if (dir == null)
        {
            GD.PrintErr($"[CodexLibrary] Cannot oppen: {CodexDataPath}");
            return;
        }

        dir.ListDirBegin();
        string fileName = dir.GetNext();
        while (fileName != "")
        {
            if (!dir.CurrentIsDir() && fileName.EndsWith(".csv"))
                LoadFromCSV(CodexDataPath + fileName);
            fileName = dir.GetNext();
        }
        dir.ListDirEnd();

        GD.Print($"[CodexLibrary] Charged — Characters: {Characters.Count} | Terms: {Terms.Count} | Localizations: {Localizations.Count}");
    }

    public List<CoreInformation> GetList(CodexType type) => type switch
    {
        CodexType.Character    => Characters,
        CodexType.Term         => Terms,
        CodexType.Localization => Localizations,
        _                      => []
    };

    private void LoadFromCSV(string path)
    {
        if (!FileAccess.FileExists(path))
        {
            GD.PrintErr($"[CodexLibrary] Archive not found: {path}");
            return;
        }

        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        file.GetLine();

        int lineIndex = 1;
        while (!file.EofReached())
        {
            lineIndex++;
            string raw = file.GetLine().Trim();

            if (string.IsNullOrWhiteSpace(raw) || raw.StartsWith('#'))
                continue;

            string[] parts = raw.Split(';');
            if (parts.Length < 4)
            {
                GD.PrintErr($"[CodexLibrary] {path}:{lineIndex} — 4 columns are expected: '{raw}'");
                continue;
            }

            string typeStr  = parts[0].Trim();
            string title    = parts[1].Trim();
            string subtitle = parts[2].Trim();
            string content  = parts[3].Trim();

            if (!System.Enum.TryParse<CodexType>(typeStr, ignoreCase: true, out var type))
            {
                GD.PrintErr($"[CodexLibrary] {path}:{lineIndex} — unknown type: '{typeStr}'");
                continue;
            }

            if (string.IsNullOrEmpty(title))
            {
                GD.PrintErr($"[CodexLibrary] {path}:{lineIndex} — emty title, ignored.");
                continue;
            }

            var entry = new CoreInformation
            {
                DisplayType = type,
                Title       = title,
                SubTitle    = subtitle,
                Content     = content,
            };

            if (type == CodexType.Character)
                entry.CharacterPortrait = TryLoadTexture(PortraitsPath + title + ImgExtension);

            entry.SupportImages = LoadSupportImages(title);

            GetList(type).Add(entry);
        }
    }

    static Texture2D TryLoadTexture(string path)
    {
        if (!ResourceLoader.Exists(path)) return null;
        return ResourceLoader.Load<Texture2D>(path);
    }

    static Texture2D[] LoadSupportImages(string title)
    {
        var list = new List<Texture2D>();
        for (int i = 0; ; i++)
        {
            var tex = TryLoadTexture($"{SupportImgPath}{title}_{i}{ImgExtension}");
            if (tex == null) break;
            list.Add(tex);
        }
        return [.. list];
    }
}