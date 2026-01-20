using Godot;
using System;
using System.Collections.Generic;

public class CommandRouter
{
    List<ICommandHandler> commandHandlers = [
        new CameraHandler(),
        new CharacterHanlder()
    ];

    public ICommandHandler GetHandler(CommandToken token)
    {
        foreach (var handler in commandHandlers){
            if(handler.Supportedverbs.Contains(token.Verb))
                return handler;
        }
        GD.PrintErr($"[CommandRouter] '{token.Verb}' is not recognized");
        return null;
    }

}