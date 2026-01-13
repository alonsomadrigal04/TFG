using Godot;
using System;
using System.Collections.Generic;

public interface ICommandHandler
{
    public HashSet<string> Supportedverbs {get;}
    public void Execute(CommandToken commandToken);
}