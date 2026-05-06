using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using Game.Utility;
using Godot;

public class UiHandler : ICommandHandler
{
    public HashSet<string> Supportedverbs => supportedVerbs;

    static readonly HashSet<string> supportedVerbs =
    [
        "off",
        "change",
        "uncover",
        "dramatic",
        "inventory",
        "play",
        "move",
        "stop"
    ];

    public void Execute(CommandToken commandToken)
    {
        switch (commandToken.Verb)
        {
            case "off":
                HideUi(commandToken);
                break;
            case "inventory":
                HandleInventory(commandToken);
                break;
            case "dramatic":
                HandleDramaticText(commandToken);
                break;
            case "change":
                HandleChange(commandToken);
                break;
            case "uncover":
                HandleUncover(commandToken);
                break;
            case "play":
                HandlePlay(commandToken);
                break;
            case "move":
                HandleMoveUI(commandToken);
                break;
            case "stop":
                HandleStop(commandToken);
                break;
            default:
                break;
        }
    }

    void HandleStop(CommandToken commandToken)
    {
        if(commandToken.Subject == "music")
        {
            UiStage.Instance.StopMusic();
            return;
        }
        UiStage.Instance.StopSFX();
    }

    void HandleMoveUI(CommandToken commandToken)
    {
        if(commandToken.Arguments.Count != 1)
        {
            GD.PrintErr("[UiHandler] the arguments are not valid");
            return;
        }
        ScreenPosition position = ToolKit.ParseEnum<ScreenPosition>(commandToken.Arguments[0]);
        UiStage.Instance.MoveUI(position);
    }

    void HandlePlay(CommandToken commandToken)
    {
        if(commandToken.Arguments.Count != 1)
        {
            GD.PrintErr("[UiHandler] the arguments are not valid");
            return;
        }
        if(commandToken.Subject == "music")
        {
            UiStage.Instance.PlayMusic(commandToken);
            return;
        }
        UiStage.Instance.PlaySound(commandToken);
    }

    void HandleUncover(CommandToken commandToken)
    {
        UiStage.Instance.UncoverTextBox();
    }

    void HandleChange(CommandToken commandToken)
    {
        if(commandToken.Arguments.Count != 1)
        {
            GD.PrintErr("[UI Handler] ammount of argumentts not valid");
        }
        string textStyle = char.ToUpper(commandToken.Arguments[0][0]) + commandToken.Arguments[0][1..];
        TextboxTypes style = ToolKit.ParseEnum<TextboxTypes>(textStyle);
        UiStage.Instance.ChangeBoxStyle(style);
    }

    void HandleInventory(CommandToken commandToken)
    {
        var arg = commandToken.Arguments;
        if(arg.Count != 1)
        {
            GD.Print("[UiHandler] Invalid arguments. Try 'on' or 'off' ");
            return;
        }
        if(commandToken.Arguments[0] == "on")
            UiStage.Instance.ShowInventory();
        else if (commandToken.Arguments[0] == "off")
            UiStage.Instance.HideInventory();
        else
        {
            GD.Print("[UiHandler] Invalid arguments. Try 'on' or 'off' ");
            return;
        }
    }

    void HandleDramaticText(CommandToken commandToken)
    {
        if(commandToken.Arguments.Count != 1)
        {
            GD.PrintErr("[UiHandler] the arguments are not valid");
        }
        UiStage.Instance.AnimateDramaticText(commandToken.Arguments[0]);
    }

    void HideUi(CommandToken commandToken)
    {
        UiStage.Instance.AnimateHideTextBox();
        //UiStage.Instance.HideTextBox();
    }

    public void CleanEffects()
    {
        UiStage.Instance.CleanEffects();
    }
}