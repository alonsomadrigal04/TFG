using System;
using System.Collections.Generic;
using Godot;

public class CameraHandler : ICommandHandler
{
    public HashSet<string> Supportedverbs => supportedVerbs;
    static readonly HashSet<string> supportedVerbs =
    [
        "zoom",
        "shake"
    ];

    public void Execute(CommandToken commandToken)
    {
        switch (commandToken.Verb)
        {
            case "zoom":
                Zoom(commandToken);
                break;
            case "shake":
                Shake(commandToken);
                break;
            default:
                break;
        }
    }

    void Shake(CommandToken commandToken)
    {
        if (!TryParseShakeArgs(commandToken, out float duration, out int intensity))
            return;

        CameraStage.Instance.CameraShake(duration, intensity);
    }

    void Zoom(CommandToken commandToken)
    {
        if(!TryParseZoomArgs(commandToken, out Vector2 zoomPosition, out float seconds))
            return;
        
        CameraStage.Instance.CameraZoom(zoomPosition, seconds);
    }

    private bool TryParseZoomArgs(CommandToken commandToken, out Vector2 zoomPosition, out float duration)
    {
        duration = 1f;
        if(CharacterDatabase.TryGetCharacter(commandToken.Subject, out Character character))
        {
            TextureRect portraitZoomed = CharacterStage.CharactersInScene[character];
            zoomPosition = portraitZoomed.Position;
        }
        else
        {
            ScreenPosition parsedScreenPosition = ToolKit.ParseEnum<ScreenPosition>(commandToken.Subject); // TODO: If you write wrong a type trhow a non controled error
            zoomPosition = ToolKit.GetPosition(parsedScreenPosition); 
        }

        if (commandToken.Arguments.Count > 1)
            GD.PrintErr("[CameraHanlder] overload for too many arguments");
        else if (commandToken.Arguments.Count == 1)
        {
            string arg = commandToken.Arguments[0];
            if (arg.EndsWith('s'))
            {
                if(!float.TryParse(arg[..^1], out duration))
                {
                    GD.PrintErr($"[CameraHandler] Invalid duration value: {arg}");
                    return false;
                }
            }
        }
        return true;
    }

    static bool TryParseShakeArgs(CommandToken token, out float duration, out int intensity)
    {
        duration = 0.2f;
        intensity = 4;

        bool durationSet = false;
        bool intensitySet = false;

        foreach (string arg in token.Arguments)
        {
            if (arg.EndsWith('s'))
            {
                if (durationSet)
                {
                    GD.PrintErr("[CameraShake] Duration specified more than once.");
                    return false;
                }

                if (!float.TryParse(arg[..^1], out duration))
                {
                    GD.PrintErr($"[CameraShake] Invalid duration value: {arg}");
                    return false;
                }

                durationSet = true;
                continue;
            }

            if (arg.EndsWith('i'))
            {
                if (intensitySet)
                {
                    GD.PrintErr("[CameraShake] Intensity specified more than once.");
                    return false;
                }

                if (!int.TryParse(arg[..^1], out intensity))
                {
                    GD.PrintErr($"[CameraShake] Invalid intensity value: {arg}");
                    return false;
                }

                intensitySet = true;
                continue;
            }

            GD.PrintErr($"[CameraShake] Unknown argument: {arg}");
            return false;
        }

        return true;
    }
}