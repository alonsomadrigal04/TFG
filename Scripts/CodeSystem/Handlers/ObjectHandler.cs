using Godot;
using System;
using System.Collections.Generic;

public class ObjectHanlder : ICommandHandler
{
    public HashSet<string> Supportedverbs => supportedVerbs;
    public HashSet<string> supportedVerbs = [
        "shows"
    ];

    public void Execute(CommandToken commandToken)
    {
        switch (commandToken.Verb)
        {
            case "shows":
                ObjectAppear(commandToken);
                break;
            default:
                break;
        }
    }

    void ObjectAppear(CommandToken commandToken)
    {
        
    }
}