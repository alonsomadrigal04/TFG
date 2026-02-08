using System;
using System.Collections.Generic;
using Godot;

public class BackgroundHandler : ICommandHandler
{
    public HashSet<string> Supportedverbs => supportedVerbs;

    static readonly HashSet<string> supportedVerbs =
    [
        "bg",
        "trans",
        "flashback"
    ];

    public void Execute(CommandToken commandToken)
    {
        switch (commandToken.Verb)
        {
            case "bg":
                SetBackground(commandToken);
                break;
            case "trans":
                MakeTransition(commandToken);
                break;
            case "flashback":
                MakeFlashback(commandToken);
                break;
            default:
                break;
        }
    }

    private void MakeFlashback(CommandToken commandToken)
    {
        //TODO: parse duration in commandToken
        string bgname = commandToken.Subject;
        if(!BackgroundDataBase.LoadedBackgrounds.TryGetValue(bgname, out Texture2D newBg))
            GD.PrintErr($"[BackgroundStage] there is no {bgname} background");
        BackgroundStage.Instance.MakeFlashback(newBg);
    }

    void MakeTransition(CommandToken commandToken)
    {
        string bgname = commandToken.Subject;
        if(commandToken.Arguments.Count < 1)
            GD.PrintErr("[BackgroundHandler] not especificated type of transition");

        if(!BackgroundDataBase.LoadedBackgrounds.TryGetValue(bgname, out Texture2D newBg))
            GD.PrintErr($"[BackgroundStage] there is no {bgname} background");
        switch (commandToken.Arguments[0])
        {
            case "blur":
                BackgroundStage.Instance.BlurTransition(newBg);
                break;
            case "flash":
                BackgroundStage.Instance.FlashTransition(newBg);
                break;
            default:
                BackgroundStage.Instance.BlurTransition(newBg);
                break;
        }
    }

    void SetBackground(CommandToken commandToken)
    {
        string bgname = commandToken.Subject;
        if(!BackgroundDataBase.LoadedBackgrounds.TryGetValue(bgname, out Texture2D newBg))
            GD.PrintErr($"[BackgroundStage] there is no {bgname} background");

        BackgroundStage.Instance.SetBackground(newBg);
    }
}