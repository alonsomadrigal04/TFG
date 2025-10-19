using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class DialogManager : Node
{
    private DialogReader reader;
    [Export] private TextTyper textTyper;

    private Dictionary<string, DialogLine> dialogLines;
    private List<string> orderedUids;
    private DialogLine currentLine;

    public override void _EnterTree()
    {
        reader = new DialogReader();
    }

    public void StartDialogScene(string sceneName)
    {
        string path = $"res://Assets/Csv/{sceneName}.csv";
        dialogLines = reader.LoadFromCSV(path);
        orderedUids = reader.GetOrderedUids();

        if (orderedUids.Count > 0)
        {
            StartDialog(orderedUids[0]);
        }
        else
        {
            GD.PrintErr("No lines found in the CSV.");
        }
    }

    public void StartDialog(string uid)
    {
        if (dialogLines.TryGetValue(uid, out var line))
        {
            currentLine = line;
            textTyper.WriteText(line.Text, line.Speaker, 0.05f);
        }
        else
        {
            GD.PrintErr($"The line with UID {uid} was not found");
        }
    }

    public void OnNextRequested()
    {
        if (currentLine == null)
            return;

        string nextUid = currentLine.Next;

        if (!string.IsNullOrEmpty(nextUid) && dialogLines.ContainsKey(nextUid))
        {
            StartDialog(nextUid);
            return;
        }

        int currentIndex = orderedUids.IndexOf(currentLine.Uid);
        if (currentIndex != -1 && currentIndex + 1 < orderedUids.Count)
        {
            string nextInOrder = orderedUids[currentIndex + 1];
            StartDialog(nextInOrder);
        }
        else
        {
            GD.Print("End of dialogue.");
        }
    }
}
