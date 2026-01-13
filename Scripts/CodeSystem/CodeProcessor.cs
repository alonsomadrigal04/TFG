using Godot;
using System;

public static class CodeProcessor
{
    public static void RunCode(string rawCommandLine)
    {
        CommandToken commandToken = CommandTokenizer.ParseCommand(rawCommandLine);
        CommandRouter commandRouter = new();
        ICommandHandler handler = commandRouter.GetHandler(commandToken);

        handler.Execute(commandToken);
    }
}