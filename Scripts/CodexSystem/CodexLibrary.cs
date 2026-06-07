using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public class CodexLibrary
{
    public static event Action codexActualized;
    private const string CodexDataPath  = "res://Data/Codex/";
    private const string PortraitsPath  = "res://Data/Codex/Portraits/";
    private const string EyesPath  = "res://Data/Codex/Eyes/";

    private const string SupportImgPath = "res://Data/Codex/Images/";
    private const string VideoPath = "res://Data/Codex/Videos/";

    private const string ImgExtension   = ".png";
    private const string VideoExtension   = ".ogv";

    static Godot.Collections.Dictionary<string, CoreInformation> codexLibrary = [];


    public static List<CoreInformation> Characters     { get; } = [];
    public static List<CoreInformation> Terms          { get; } = [];
    public static List<CoreInformation> Localizations  { get; } = [];

    public void LoadAll()
    {
        codexLibrary.Clear();

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
        GD.Print($"[CodexLibrary] Charged — {codexLibrary.Count} entries");
        ChargeInitialEntries();
        //GD.Print($"[CodexLibrary] Charged — Characters: {Characters.Count} | Terms: {Terms.Count} | Localizations: {Localizations.Count}");
    }

    public static List<CoreInformation> GetList(CodexType type) => type switch
    {
        CodexType.Character    => Characters,
        CodexType.Term         => Terms,
        CodexType.Localization => Localizations,
        _                      => []
    };

    void LoadFromCSV(string path)
    {
        if (!FileAccess.FileExists(path))
        {
            GD.PrintErr($"[CodexLibrary] Archive not found: {path}");
            return;
        }

        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);

        ulong fileLength = file.GetLength();
        byte[] bytes = file.GetBuffer((long)fileLength);
        string fullContent = System.Text.Encoding.UTF8.GetString(bytes);

        List<string[]> rows = ParseCSV(fullContent, ';');

        for (int i = 1; i < rows.Count; i++)
        {
            string[] parts = rows[i];
            int lineIndex = i + 1;

            if (parts.Length < 4)
            {
                GD.PrintErr($"[CodexLibrary] {path}:{lineIndex} — 4 columns are expected");
                continue;
            }

            string typeStr  = parts[0].Trim();
            string title    = parts[1].Trim();
            string subtitle = parts[2].Trim();
            string content  = parts[3].Trim();
            bool hasVideo = bool.TryParse(parts[4].Trim(), out var parsedHasVideo) && parsedHasVideo;


            if (!Enum.TryParse<CodexType>(typeStr, true, out var type))
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
                Video       = hasVideo
            };

            if (type == CodexType.Character)
            {
                entry.CharacterPortrait = TryLoadResource<Texture2D>(PortraitsPath + title + ImgExtension);
                entry.CharacterEyes = TryLoadResource<Texture2D>(EyesPath + title + ImgExtension);
            }

            entry.SupportImages = LoadSupportImages(title);

            if(entry.Video)
                entry.videoStream = LoadVideo(title);
            
            codexLibrary.Add(entry.Title, entry);

            //GetList(type).Add(entry);
        }
    }

    private static List<string[]> ParseCSV(string content, char delimiter = ';')
    {
        var rows = new List<string[]>();
        var currentRow = new List<string>();
        var currentField = new System.Text.StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < content.Length; i++)
        {
            char c = content[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < content.Length && content[i + 1] == '"')
                    {
                        currentField.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    currentField.Append(c);
                }
            }
            else
            {
                if (c == '"')
                {
                    inQuotes = true;
                }
                else if (c == delimiter)
                {
                    currentRow.Add(currentField.ToString());
                    currentField.Clear();
                }
                else if (c == '\n')
                {
                    currentRow.Add(currentField.ToString().TrimEnd('\r'));
                    currentField.Clear();

                    if (currentRow.Count > 0)
                        rows.Add([.. currentRow]);

                    currentRow.Clear();
                }
                else
                {
                    currentField.Append(c);
                }
            }
        }

        if (currentField.Length > 0 || currentRow.Count > 0)
        {
            currentRow.Add(currentField.ToString());
            rows.Add([.. currentRow]);
        }

        return rows;
    }

    public static void ChargeEntry(string entryName, bool throwSignal = true)
    {
        var entry = codexLibrary[entryName];
        GetList(entry.DisplayType).Add(entry);

        if(throwSignal)
            codexActualized?.Invoke();
    }

    void ChargeInitialEntries()
    {
        ChargeEntry("Guerra de Sucesión: Parte I", false);
        ChargeEntry("Ghalendir", false);
        ChargeEntry("Quion Statt", false);
        ChargeEntry("Fled Sorindu", false);
        ChargeEntry("Treya Meiden", false);
    }

    static VideoStream LoadVideo(string title)
    {
        string videoName = title.Trim().Replace(" ", "").ToLower();
        var video = TryLoadResource<VideoStream>($"{VideoPath}{videoName}{VideoExtension}");
        if(video == null)
            GD.Print($"[Codex Library] cannot load video named {videoName}{VideoExtension}");
        return video;
    }

    static T TryLoadResource<T>(string path) where T : Resource
    {
        if (!ResourceLoader.Exists(path)) return null;
        return ResourceLoader.Load<T>(path);
    }

    static Texture2D[] LoadSupportImages(string title)
    {
        var list = new List<Texture2D>();
        for (int i = 0; ; i++)
        {
            var tex = TryLoadResource<Texture2D>($"{SupportImgPath}{title}_{i}{ImgExtension}");
            if (tex == null) break;
            list.Add(tex);
        }
        return [.. list];
    }
}