using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;

public partial class DialogManager : Node
{
    [Export] ChoiceMaker choiceMaker;
    [Export] TextTyper textTyper;
    [Export] TextTyper dialogueBox;
    [Export] AudioManager sounds;
    Dictionary<string, DialogLine> dialogLines;
    List<string> orderedUids;
    DialogLine currentLine;

    bool isInChoiceMode = false;
    public bool IsSpeaking {get; private set;} = false;
    Character LastSpeaker = null;



    public override void _EnterTree()
    {
        choiceMaker.ChoiceSelected += OnChoiceSelected;
    }

    public bool IsTyping() => textTyper.isTyping;

    void OnChoiceSelected(string nextUid)
    {
        isInChoiceMode = false;
        StartDialog(nextUid);
    }


    /// <summary>
    /// Starts a dialog scene by loading dialog lines from a CSV file.
    /// </summary>
    public void StartDialogScene(Dialog sceneName)
    {
        IsSpeaking = true;
        dialogLines = sceneName.Conversation;
        orderedUids = sceneName.OrderedUids;
        BackgroundStage.Instance.SetBlurBg();
        textTyper.CleanTextBox();

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
            if(line.Code != "")
            {
                CodeProcessor.RunCode(line.Code);
                if (ActionBus.IsBusy)
                {
                    Action solicitatedAction = ActionBus.RunAfterActions(OnNextRequested);
                }
                else
                {
                    OnNextRequested();
                }
                return;
            }

            string[] typePortions = line.Type.Split('/', StringSplitOptions.TrimEntries);
            switch (typePortions[0].ToLower())
            {
                case "say":
                    ProcessDialogLine(line, typePortions);
                    break;

                case "choice":
                    isInChoiceMode = true;
                    choiceMaker.ShowChoices(line);
                    break;

                case "think":
                    ShowThinkLayout(line, typePortions);
                    break;

                default:
                    GD.PrintErr($"Unknown dialog line type: {line.Type}");
                    break;
            }
        }

    }

    void ShowThinkLayout(DialogLine line, string[] typePortions)
    {
        CharacterStage.Instance.SetThinkingLayout(line.Speaker, true);
        ProcessDialogLine(line, typePortions, true);
    }

    public void DesactivateDialogFrameWork()
    {
        CodeProcessor.TurnOffHandlers();
    }


    void ProcessDialogLine(DialogLine line, string[] typePortions, bool isThought = false)
    {
        if(!isThought && CharacterStage.IsThinking)
            CharacterStage.Instance.SetThinkingLayout(line.Speaker, false);
        if(UiStage.Instance.IsTextBoxHide())
            UiStage.Instance.AnimateShowTextBox();
        if(LastSpeaker == null || line.Speaker != LastSpeaker)
        {
            LastSpeaker = line.Speaker;
            ActionBus.RunAfterActions(() =>
            {
                CharacterStage.Instance.AnimateTalking(line.Speaker);   
            });
        }
        textTyper.WriteText(line.Text, line.Speaker);
        if (typePortions.Length > 1)
            FlavourAnimator.Instance.PlayFlavour(typePortions[1], line.Speaker);
        DebugService.Register("Last speaker", () => line.Speaker.Name);
    }

    public void OnNextRequested()
    {
        if (currentLine == null)
            return;

        if (isInChoiceMode)
            return;

        sounds.NextSentence.Play();

        string nextUid = currentLine.Next;
        NextUidType nextType = ParseNextUid(nextUid);

        switch (nextType)
        {
            case NextUidType.EndGame:
                DesactivateDialogFrameWork();
                EndGame();
                return;

            case NextUidType.EndChapter:
                DesactivateDialogFrameWork();
                EndChapter();
                return;

            case NextUidType.ChangeDialog:
                DesactivateDialogFrameWork();
                HandleDialogChange(nextUid);
                return;
            case NextUidType.ExploreZone:
                DesactivateDialogFrameWork();
                ChangeToExploreMode(nextUid);
                return;

            case NextUidType.DiferentLine:
                if (dialogLines.ContainsKey(nextUid))
                {
                    StartDialog(nextUid);
                    return;
                }
                break;
        }

        GoToNextOrderedLine();
    }

    void ChangeToExploreMode(string raw)
    {
        GameStateManager.Instance.ChangeState(State.Explore);
        string[] parts = raw.Split(" ", StringSplitOptions.RemoveEmptyEntries);
        GameManager.ChangeEnvironment(GameEnvironments.ParseEnvironment(parts[3]));
    }


    void GoToNextOrderedLine()
    {
        int currentIndex = orderedUids.IndexOf(currentLine.Uid);

        if (currentIndex >= 0 && currentIndex + 1 < orderedUids.Count)
        {
            StartDialog(orderedUids[currentIndex + 1]);
        }
        else
        {
            EndDialog();
        }
    }

    void HandleDialogChange(string raw)
    {
        string[] parts = raw.Split(" ", StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 4)
        {
            GD.PrintErr("[DialogManager] Invalid chapter change syntax.");
            return;
        }

        string chapterName = parts[3];

        Dialog newDialog = ConversationsDataBase.GetConversation(chapterName);

        if (newDialog == null)
        {
            GD.PrintErr($"[DialogManager] Dialog {chapterName} not found.");
            return;
        }
        ActionBus.RunAfterActions(() =>
        {
            StartDialogScene(newDialog);
        });
    }

    void EndGame()
    {
        throw new NotImplementedException();
    }

    void EndDialog()
    {
        IsSpeaking = false;
        GameStateManager.Instance.ChangeState(State.Explore);

        GD.Print("End of dialogue.");
    }

    void EndChapter()
    {
        IsSpeaking = false;
        GD.Print("Chapter ended.");

        GameManager.Instance.StartNewChapter();
    }

    static NextUidType ParseNextUid(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return NextUidType.NextLine;

        string[] parts = raw.ToLower().Split(" ", StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length >= 2)
        {
            if (parts[0] == "chapter" && parts[1] == "ends")
                return NextUidType.EndChapter;

            if (parts[0] == "game" && parts[1] == "ends")
                return NextUidType.EndGame;
        }

        if (parts.Length >= 3)
        {
            if (parts[0] == "go" && parts[1] == "to" && parts[2] == "dialog")
                return NextUidType.ChangeDialog;
            else if(parts[2] == "explorezone")
                return NextUidType.ExploreZone;
            else
                GD.PrintErr("[DialogManager] wrong command entered, try 'go to dialog [name of the dialog]'");
        }

        return NextUidType.DiferentLine;
    }

}

public enum NextUidType
{
    EndChapter,
    ChangeDialog,
    EndGame,
    NextLine,
    DiferentLine,
    ExploreZone
}