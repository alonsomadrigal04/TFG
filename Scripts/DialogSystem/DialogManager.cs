using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

public partial class DialogManager : Node
{
    DialogReader reader;
    [Export] ChoiceMaker choiceMaker;
    [Export] TextTyper textTyper;

    Dictionary<string, DialogLine> dialogLines;
    List<string> orderedUids;
     DialogLine currentLine;

     bool isInChoiceMode = false;


    public override void _EnterTree()
    {
        reader = new DialogReader();
        choiceMaker.ChoiceSelected += OnChoiceSelected;
    }

     void OnChoiceSelected(string nextUid)
    {
        isInChoiceMode = false;
        StartDialog(nextUid);
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
            string[] typePortions = line.Type.Split('/', StringSplitOptions.TrimEntries);
            switch (typePortions[0].ToLower())
            {
                case "say":
                    textTyper.WriteText(line.Text, line.Speaker);
                    if(typePortions.Length > 1)
                        FlavourAnimator.Instance.PlayFlavour(typePortions[1]);
                    DebugService.Register("Last speaker", () => line.Speaker.Name);
                    break;

                case "choice":
                    isInChoiceMode = true;
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
        if(isInChoiceMode)
            return;

        string nextUid = currentLine.Next;

        if(nextUid == "END")
        {
            EndDialog();
            return;
        }
        else if (!string.IsNullOrEmpty(nextUid) && dialogLines.ContainsKey(nextUid))
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
            EndDialog();
            return;
        }
    }

    private void EndDialog()
    {
        GD.Print("End of dialogue.");
    }
}