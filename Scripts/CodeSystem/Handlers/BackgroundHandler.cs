using System;
using System.Collections.Generic;
using Godot;

public class BackgroundHandler : ICommandHandler
{
    public HashSet<string> Supportedverbs => supportedVerbs;

    static readonly HashSet<string> supportedVerbs =
    [
        "bg"
    ];

    public void Execute(CommandToken commandToken)
    {
        switch (commandToken.Verb)
        {
            case "bg":
            ChangeBackground(commandToken);
            break;
        }
    }

    void ChangeBackground(CommandToken commandToken)
    {
        BackgroundStage.Instance.SetBackground(commandToken.Subject);
    }
}