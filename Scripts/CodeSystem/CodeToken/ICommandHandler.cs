using Godot;
using System;

public interface ICommandHandler
{
    public bool CanHandle(CommandToken token); // Maybe this will replaced for some dicctionary or a GetHandle method
    public void Execute();
}