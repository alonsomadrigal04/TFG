using Godot;
using System;

public static class CodeProcessor
{

    public static void RunCode(string rawCommandLine)
    {
        CommandToken commandToken = CommandTokenizer.ParseCommand(rawCommandLine);
    }
}