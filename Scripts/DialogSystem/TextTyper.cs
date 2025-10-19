using Game.Common.Modules;
using Godot;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;


public partial class TextTyper : Control
{
    [ExportGroup("Audio")]
    [Export] MultiaudioPlayerModule audioModule;
    [Export] AudioStream sound;
    [ExportGroup("Text Boxes")]
    [Export] RichTextLabel dialogBox;
    [Export] RichTextLabel nameBox;
    [ExportGroup("preset parameters ")]
    [Export] public string waveParams = "amp=50.0 freq=5.0 connected=1";
    [Export] public string shakeParams = "rate=10.0 level=10.0 connected=1";


    const float textSpeedDefaul = 0.01f;
    static readonly Character characterDefault = Character.Default;
    readonly Dictionary<string, AudioStream> speakerSounds;

    /// <summary>
    /// Writes text to the dialog box with a typing effect. 
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="speaker">The speaker's name.</param>
    /// <param name="textSpeed">The speed of the typing effect.</param>
    public async void WriteText(string text, string speaker, float textSpeed = textSpeedDefaul)
    {
        //speaker ??= characterDefault;
        nameBox.Text = speaker/*.Name*/;
        dialogBox.Text = "";

        string colorVisible = "[color=#ffffffff]";
        string colorHidden = "[color=#ffffff00]";
        string colorEnd = "[/color]";

        Stack<string> tagStack = new();
        string cleanText = "";
        Stack<float> speedStack = new();
        float currentSpeed = textSpeed;

        int i = 0;
        while (i < text.Length)
        {
            if (text[i] == '[')
            {
                int closingBracket = text.IndexOf(']', i);
                if (closingBracket != -1)
                {
                    string tag = text.Substring(i, closingBracket - i + 1);
                    string tagContent = tag.Trim('[', ']');
                    string tagName = tagContent.Split(' ')[0];

                    bool isClosingTag = tag.StartsWith("[/");
                    string baseTagName = tagName.TrimStart('/');

                    if (baseTagName == "speed")
                    {
                        if (!isClosingTag)
                        {
                            float newSpeed = currentSpeed;
                            var parts = tagContent.Split(' ');
                            foreach (var part in parts)
                            {
                                if (part.StartsWith("s=") &&
                                    float.TryParse(part.AsSpan(2), NumberStyles.Float, CultureInfo.InvariantCulture, out float sValue))
                                {
                                    newSpeed = sValue;
                                }
                            }

                            speedStack.Push(currentSpeed);
                            currentSpeed = newSpeed;
                        }
                        else if (speedStack.Count > 0)
                            currentSpeed = speedStack.Pop();

                        i = closingBracket + 1;
                        continue;
                    }

                    if (baseTagName == "w" || baseTagName == "s")
                    {
                        string expandedTag;

                        if (!isClosingTag)
                        {
                            expandedTag = baseTagName == "w"
                                ? $"[wave {waveParams}]"
                                : $"[shake {shakeParams}]";
                        }
                        else
                        {
                            expandedTag = baseTagName == "w" ? "[/wave]" : "[/shake]";
                        }

                        cleanText += expandedTag;

                        if (isClosingTag)
                        {
                            if (tagStack.Count > 0)
                                tagStack.Pop();
                        }
                        else
                        {
                            tagStack.Push(expandedTag);
                        }

                        i = closingBracket + 1;
                        continue;
                    }

                    if (isClosingTag)
                    {
                        if (tagStack.Count > 0)
                            tagStack.Pop();
                    }
                    else
                    {
                        tagStack.Push(tagName);
                    }

                    cleanText += tag;
                    i = closingBracket + 1;
                    continue;
                }
            }

            string visiblePart = cleanText + text[i];
            string closingTags = "";
            foreach (string openTag in tagStack)
            {
                string tagName = openTag.Split(' ')[0].TrimStart('[');
                closingTags += "[/" + tagName + "]";
            }

            string visibleText = $"{colorVisible}{visiblePart}{closingTags}{colorEnd}";
            string hiddenText = $"{colorHidden}{text[(i + 1)..]}{colorEnd}";

            dialogBox.Text = visibleText + hiddenText;

            float waitTime = GetWaitTimeForChar(text[i], currentSpeed);
            if (waitTime > 0)
                await ToSignal(GetTree().CreateTimer(waitTime), "timeout");

            if (!char.IsWhiteSpace(text[i]))
                audioModule.PlaySound(sound, 0.2f, (float)GD.RandRange(0.7f, 0.9f));

            cleanText += text[i];
            i++;
        }

        audioModule.StopAll();
    }



    static float GetWaitTimeForChar(char c, float baseSpeed)
    {
        float waitTime = baseSpeed;

        switch (c)
        {
            case '.':
            case '!':
            case '?':
                waitTime *= 15.0f;
                break;

            case ':':
                waitTime *= 5.0f;
                break;
        }

        return waitTime;
    }




}
