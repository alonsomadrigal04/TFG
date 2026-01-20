using Godot;
using System;
using System.Linq;

public static class CommandTokenizer
{
    public static CommandToken ParseCommand(string rawCommandLine)
    {
        rawCommandLine = rawCommandLine.ToLower().Trim();
        
        string[] tokens = rawCommandLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (tokens.Length < 2)
            GD.PrintErr($"[Command Tokenizer]: '{rawCommandLine}' is not taking a Subject and a Verb");
    
        string subject = tokens[0];
        string verb = tokens[1];
        string[] args = [.. tokens.Skip(2)];
        return new CommandToken(subject, verb, args);
    }
}