using System;
using System.Collections.Generic;
using Godot;

public class UiHandler : ICommandHandler
{
    public HashSet<string> Supportedverbs => supportedVerbs;

    static readonly HashSet<string> supportedVerbs =
    [
        "hides"
    ];

    public void Execute(CommandToken commandToken)
    {
        switch (commandToken.Verb)
        {
            case "hides":
                HideUi(commandToken);
                break;
            default:
                break;
        }
    }

    void HideUi(CommandToken commandToken)
    {
        UiStage.Instance.HideTextBox();
    }
}