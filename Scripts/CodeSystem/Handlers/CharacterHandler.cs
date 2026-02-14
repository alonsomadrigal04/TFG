using Godot;
using System;
using System.Collections.Generic;

public class CharacterHanlder : ICommandHandler
{
    public HashSet<string> Supportedverbs => supportedVerbs;
    public HashSet<string> supportedVerbs = [
        "is",
        "moves",
        "appears",
        "disappears",
        "flips"
    ];

    public void Execute(CommandToken commandToken)
    {
        switch (commandToken.Verb)
        {
            case "is":
                ChangePortrait(commandToken);
                break;
            case "moves":
                MoveCharacter(commandToken);
                break;
            case "appears":
                SummonCharacter(commandToken);
                break;
            case "disappears":
                QuitCharacter(commandToken);
                break;
            case "flips":
                FlipsCharacter(commandToken);
                break;
            default:
                break;
            
        }
    }

    void FlipsCharacter(CommandToken commandToken)
    {
        Character character = CharacterDatabase.GetCharacter(commandToken.Subject);
        CharacterStage.Instance.FlipCharacterHorizontally(character);
    }


    static void QuitCharacter(CommandToken commandToken)
    {
        Character character = CharacterDatabase.GetCharacter(commandToken.Subject);
        CharacterStage.Instance.CharacterDisappears(character);
    }

    static void SummonCharacter(CommandToken commandToken)
    {
        ScreenPosition position = ToolKit.FromArguments(commandToken);
        Character character = CharacterDatabase.GetCharacter(commandToken.Subject);
        CharacterStage.Instance.CharacterAppears(character, position);
    }

    static void MoveCharacter(CommandToken commandToken)
    {
        ScreenPosition position = ToolKit.FromArguments(commandToken);
        Character character = CharacterDatabase.GetCharacter(commandToken.Subject);
        CharacterStage.Instance.MovePortrait(character, position);
    }

    void ChangePortrait(CommandToken commandToken)
    {
        throw new NotImplementedException();
    }


}