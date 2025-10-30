using System.Collections.Generic;
using Game.Common.Modules;
using Godot;

#nullable enable

public class TagProcessor()
{
    public readonly Stack<float> speedStack = new();
    private readonly Stack<string> effectStack = new();
    private float currentSpeed = 0.02f;
    public float CurrentSpeed => currentSpeed;

    readonly string waveParams = "amp=50.0 freq=5.0 connected=1";
    readonly string shakeParams = "rate=10.0 level=10.0 connected=1";

    /// <summary>
    /// Procesa un token y actualiza el contexto de efectos (velocidad, animaciones, etc.)
    /// </summary>
    public void ProcessToken(TagToken token)
    {
        switch (token)
        {
            case OpenTagToken openTag:
                HandleOpenTag(openTag);
                break;

            case CloseTagToken closeTag:
                HandleCloseTag(closeTag);
                break;
        }
    }

    public string? HandleOpenTag(OpenTagToken token)
    {
        switch (token.Content)
        {
            case "w":
                effectStack.Push("wave");
                return $"[wave {waveParams}]";

            case "s":
                effectStack.Push("shake");
                return $"[shake {shakeParams}]";
            case "b":
                effectStack.Push("b");
                return "[b]";

            case "speed":
                if (token.Parameters != null && token.Parameters.TryGetValue("s", out string? sValue) &&
                    sValue != null && float.TryParse(sValue, System.Globalization.CultureInfo.InvariantCulture, out float newSpeed))
                {
                    speedStack.Push(currentSpeed);
                    currentSpeed = newSpeed;
                }
                return null;

            default:
                return null;
        }
    }

    public string? HandleCloseTag(CloseTagToken token)
    {
        switch (token.Content)
        {
            case "w":
                effectStack.Pop();
                return "[/wave]";
            case "s":
                effectStack.Pop();
                return "[/shake]";
            case "b":
                effectStack.Pop();
                return "[/b]";
            case "speed":
                if (speedStack.Count > 0)
                    currentSpeed = speedStack.Pop();
                return null;
            default:
                return null;
        }
    }


    public IEnumerable<string> ActiveEffects => effectStack;
}
