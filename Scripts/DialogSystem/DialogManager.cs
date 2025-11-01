using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class DialogManager : Node
{
    private DialogReader reader;
    [Export] private ChoiceMaker choiceMaker;
    [Export] private TextTyper textTyper;

    private Dictionary<string, DialogLine> dialogLines;
    private List<string> orderedUids;
    private DialogLine currentLine;

    public override void _EnterTree()
    {
        reader = new DialogReader();
    }

    /// <summary>
    /// Starts a dialog scene by loading dialog lines from a CSV file.
    /// </summary>
    public void StartDialogScene(string sceneName)
    {
        string path = $"res://Assets/Csv/{sceneName}.csv";
        dialogLines = reader.LoadFromCSV(path);
        orderedUids = reader.GetOrderedUids();

        if (orderedUids.Count > 0 )
        {
            StartDialog(orderedUids[0]);
        }
        else
        {
            GD.PrintErr("No lines found in the CSV.");
        }
    }

    /// <summary>
    /// Starts a dialog line given its UID. 
    /// </summary>
    /// <param name="uid">The unique identifier of the dialog line.</param>
    public void StartDialog(string uid)
    {
        if (textTyper.isTyping) 
            textTyper.skipRequested = true;
        else if (dialogLines.TryGetValue(uid, out var line))
        {
            currentLine = line;

            switch (line.Type.ToLower())
            {
                case "say":
                    textTyper.WriteText(line.Text, line.Speaker);
                    break;

                case "choice":
                    choiceMaker.ShowChoices(line);
                    break;

                default:
                    GD.PrintErr($"Unknown dialog line type: {line.Type}");
                    break;
            }
        }

    }

    /// <summary>
    /// Handles the request to proceed to the next line of dialog. 
    /// </summary>
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
