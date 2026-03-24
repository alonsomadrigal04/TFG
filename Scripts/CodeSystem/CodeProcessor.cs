using Godot;
using System;

public static class CodeProcessor
{
    public static CommandRouter commandRouter;
    public static void RunCode(string rawCommandLine)
    {
        CommandToken commandToken = CommandTokenizer.ParseCommand(rawCommandLine);
        commandRouter = new();
        ICommandHandler handler = commandRouter.GetHandler(commandToken);

        handler.Execute(commandToken);
    }

    public static void TurnOffHandlers()
    {
        commandRouter.SutDown();
    }
}