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
        "transparent",
        "uncover",
        "dramatic",
        "inventory",
        "play",
        "move"
    ];

    public void Execute(CommandToken commandToken)
    {
        switch (commandToken.Verb)
        {
            case "off":
                HideUi(commandToken);
                break;
            case "inventory":
                HideInventory(commandToken);
                break;
            case "dramatic":
                HandleDramaticText(commandToken);
                break;
            case "transparent":
                HandleTransparent(commandToken);
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
            default:
                break;
        }
    }

    void HandleMoveUI(CommandToken commandToken)
    {
        UiStage.Instance.MoveUI();
    }

    void HandlePlay(CommandToken commandToken)
    {
        if(commandToken.Arguments.Count != 1)
        {
            GD.PrintErr("[UiHandler] the arguments are not valid");
            return;
        }
        UiStage.Instance.PlaySound(commandToken);
    }

    void HandleUncover(CommandToken commandToken)
    {
        UiStage.Instance.UncoverTextBox();
    }

    void HandleTransparent(CommandToken commandToken)
    {
        UiStage.Instance.SetTransparent();
    }

    void HideInventory(CommandToken commandToken)
    {
        UiStage.Instance.HideInventory();
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
        UiStage.Instance.HideTextBox();
    }

    public void CleanEffects()
    {
        UiStage.Instance.CleanEffects();
    }
}