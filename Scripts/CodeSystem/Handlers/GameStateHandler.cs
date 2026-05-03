using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Utility;

public class GameStateHandler : ICommandHandler
{
    public HashSet<string> Supportedverbs => supportedVerbs;
    public HashSet<string> supportedVerbs = [
        "affinity",
        ">",
        "<",
        "needs",
        "setflag",
        "wait"
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
            case "needs":
                ParseConditionObject(commandToken);
                break;
            case "setflag":
                ParseFlag(commandToken);
                break;
            case "wait":
                HandleWaitCondition(commandToken);
                break;
            default:
                break;
        }
    }

    static void HandleWaitCondition(CommandToken commandToken)
    {
        ActionBus.ActionStarted();

        GodotExtensions.Wait(commandToken.Arguments[0].ToFloat(), ActionBus.ActionFinished);
    }

    void ParseFlag(CommandToken commandToken)
    {
        IReadOnlyList<string> args = commandToken.Arguments;
        if(args.Count > 1)
            GD.PrintErr("[GameStateHandler] too many arguments in line");
        
        string flag = args[0];
        GameFlagsManager.AddFlag(flag);
    }

    void ParseConditionObject(CommandToken commandToken)
    {
        IReadOnlyList<string> args = commandToken.Arguments;
        if(args.Count != 3)
            GD.PrintErr("[GameStateHandler] wrong spelling try Alonso needs cafetera : A2");

        ObjectData objectData = ObjectDataBase.GetObject(commandToken.Arguments[0]);
        if(!ObjectDataBase.PlayerInventory.Contains(objectData)){
            GameManager.Instance.DialogManager.StartDialog(args[2].ToUpper());
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
                    GameManager.Instance.DialogManager.StartDialog(args[2].ToUpper());
                break;
            case "<":
                    if(targetAfinity < args[0].ToInt())
                        GameManager.Instance.DialogManager.StartDialog(args[2].ToUpper());
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
    }

    public void CleanEffects()
    {
        return;
    }
}