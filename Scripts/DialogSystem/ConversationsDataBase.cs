using Godot;
using System;
using System.Collections.Generic;

public static class ConversationsDataBase
{
    public static Dictionary<string, Dialog> LoadedConversations { get; private set; } = [];

    private const string DialogFolderPath = "res://Data/Csv";

    public static void Load()
    {
        LoadedConversations.Clear();

        if (!DirAccess.DirExistsAbsolute(DialogFolderPath))
        {
            GD.PrintErr("[ConversationsDataBase] Dialog folder not found.");
            return;
        }

        using var dir = DirAccess.Open(DialogFolderPath);

        dir.ListDirBegin();
        string fileName;

        while ((fileName = dir.GetNext()) != "")
        {
            if (dir.CurrentIsDir())
                continue;

            if (!fileName.EndsWith(".csv"))
                continue;

            string fullPath = $"{DialogFolderPath}/{fileName}";
            string key = System.IO.Path.GetFileNameWithoutExtension(fileName);

            var reader = new DialogReader();
            reader.LoadFromCSV(fullPath);
            var dialogueLines = reader.Lines; 

            if (dialogueLines == null || dialogueLines.Count == 0)
            {
                GD.PrintErr($"[ConversationsDataBase] Failed loading dialog: {fileName}");
                continue;
            }

            Dialog conversation = new(reader.Lines, reader.OrderedUids);

            LoadedConversations[key] = conversation;
        }

        dir.ListDirEnd();

        DebugService.Register("Conversations in BBDD", () => LoadedConversations.Count.ToString());
    }

    public static Dialog GetConversation(string name)
    {
        if (!LoadedConversations.TryGetValue(name, out Dialog value))
        {
            GD.PrintErr($"[ConversationsDataBase] Conversation not found: {name}");
            return null;
        }

        return value;
    }

}