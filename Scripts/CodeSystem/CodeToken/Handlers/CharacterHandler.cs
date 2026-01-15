using Godot;
using System;
using System.Collections.Generic;

public class CharacterHanlder : ICommandHandler
{
    public HashSet<string> Supportedverbs => supportedVerbs;
    public HashSet<string> supportedVerbs = [
        "is",
        "move",
        "appears"
    ];

    public void Execute(CommandToken commandToken)
    {
        switch (commandToken.Verb)
        {
            case "is":
                ChangePortrait(commandToken);
                break;
            case "move":
                MoveCharacter(commandToken);
                break;
            case "appears":
                SummonCharacter(commandToken);
                break;
            default:
                break;
            
        }
    }

    private void SummonCharacter(CommandToken commandToken)
    {
        Character character = CharacterDatabase.GetCharacter(commandToken.Subject);
        CharacterStage.Instance.CharacterAppears(character, ToolKit.Center);
    }

    private void MoveCharacter(CommandToken commandToken)
    {
        throw new NotImplementedException();
    }

    private void ChangePortrait(CommandToken commandToken)
    {
        throw new NotImplementedException();
    }
}