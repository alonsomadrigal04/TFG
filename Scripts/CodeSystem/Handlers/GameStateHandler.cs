using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class GameStateHandler : ICommandHandler
{
    public HashSet<string> Supportedverbs => supportedVerbs;
    public HashSet<string> supportedVerbs = [
        "affinity",
        ">",
        "<"
    ];

    public void Execute(CommandToken commandToken)
    {
        switch (commandToken.Verb)
        {
            case "affinity":
                ChangeAffinity(commandToken);
                break;
            case ">":
                ParseCondition(commandToken);
                break;
            case "<":
                ParseCondition(commandToken);
                break;
            default:
                break;
        }
    }

    void ParseCondition(CommandToken commandToken)
    {
        string raw = commandToken.Subject;
        string[] parts = raw.ToLower().Split("_", StringSplitOptions.RemoveEmptyEntries);
        if(parts.Length != 2)
            GD.PrintErr("[GameStateHandler] wrong spelling try Alonso_Afinity");
        
        Character character = CharacterDatabase.GetCharacter(parts[0]);
        int targetAfinity = character.characterState.Afinity;

        IReadOnlyList<string> args = commandToken.Arguments;
        if(args.Count != 3)
            GD.PrintErr($"[GameStateHandler] insuficient data in commands {commandToken.Arguments}");

        switch (commandToken.Verb)
        {
            case ">":
                    if(targetAfinity > args[0].ToInt())
                    GameManager.Instance.dialogManager.StartDialog(args[2].ToUpper());
                break;
            case "<":
                    if(targetAfinity < args[0].ToInt())
                        GameManager.Instance.dialogManager.StartDialog(args[2].ToUpper());
                break;
            default:
                break;
        }


    }

    static void ChangeAffinity(CommandToken commandToken)
    {
        IReadOnlyList<string> args = commandToken.Arguments;
        if(args.Count > 1 )
            GD.PrintErr("[GAMESTATEHANDLER] too many arguments in line");
        Character character = CharacterDatabase.GetCharacter(commandToken.Subject);
        UiStage.Instance.AnimateRemember(character.Name);
        character.SetAffinity(args[0].ToInt());
        GD.Print(character.characterState.Afinity);
    }

    public void CleanEffects()
    {
        return;
    }
}