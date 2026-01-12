using Godot;
using System;

public class CameraHandler : ICommandHandler
{
    public bool CanHandle(CommandToken token)
    {
        switch (token.Verb)
        {
            case "Zoom":
            break;
            default:
            break;
        }
        return false;
    }

    public void Execute()
    {
        throw new NotImplementedException();
    }
}