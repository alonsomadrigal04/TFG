using Godot;
using System;
using System.Collections.Generic;

public class CommandRouter
{
    readonly List<ICommandHandler> commandHandlers = [
        new CameraHandler(),
        new CharacterHanlder(),
        new BackgroundHandler(),
        new ObjectHandler(),
        new UiHandler(),
        new GameStateHandler()
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

    public void SutDown()
    {
        foreach(var handler in commandHandlers)
        {
            handler.CleanEffects();
        }
    }

}