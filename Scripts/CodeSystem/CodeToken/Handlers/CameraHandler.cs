using Godot;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

public class CameraHandler : ICommandHandler
{
    public HashSet<string> Supportedverbs => supportedVerbs;
    static readonly HashSet<string> supportedVerbs =
    [
        "zoom",
        "shake"
    ];

    public void Execute(CommandToken commandToken)
    {
        switch (commandToken.Verb)
        {
            case "zoom":
                Zoom(commandToken);
                break;
            case "shake":
                Shake(commandToken);
                break;
            default:
                break;
        }
    }

    private void Shake(CommandToken commandToken)
    {
        throw new NotImplementedException();
    }

    private void Zoom(CommandToken commandToken)
    {
        throw new NotImplementedException();
    }
}