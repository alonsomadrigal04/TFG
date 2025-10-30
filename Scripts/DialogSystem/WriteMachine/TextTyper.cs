using Game.Common.Modules;
using Godot;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;


public partial class TextTyper : Control
{
    [ExportGroup("Audio")]
    [Export] MultiaudioPlayerModule audioModule;
    [Export] AudioStream sound;
    [ExportGroup("Text Boxes")]
    [Export] RichTextLabel dialogBox;
    [Export] RichTextLabel nameBox;
    [ExportGroup("preset parameters ")]
    [Export] float speedMultiplier = 1.2f;


    bool isTyping;
    bool skipRequested = false;
    TagProcessor tagProcessor;

    public override void _EnterTree()
    {
        tagProcessor = new TagProcessor();
    }


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
        isTyping = true;
        nameBox.Text = speaker;
        dialogBox.Text = "";

        string colorVisible = "[color=#ffffffff]", colorHidden = "[color=#ffffff00]", colorEnd = "[/color]", cleanText = "";

        var tokens = TagParser.Parse(text);

        int i = 0;
        while (i < tokens.Count)
        {
            var token = tokens[i];

            if (token is TextToken textToken)
            {
                int charIndex = 0;
                while (charIndex < textToken.Content.Length)
                {
                    char c = textToken.Content[charIndex];
                    string visiblePart = cleanText + c;
                    string closingTags = BuildClosingTags(tagProcessor.ActiveEffects);

                    string remainingText = textToken.Content[(charIndex + 1)..];
                    for (int j = i + 1; j < tokens.Count; j++)
                    {
                        if (tokens[j] is TextToken t)
                            remainingText += t.Content;
                    }

                    string visibleText = $"{colorVisible}{visiblePart}{closingTags}{colorEnd}";
                    string hiddenText = $"{colorHidden}{remainingText}{colorEnd}";

                    dialogBox.Text = visibleText + hiddenText;

                    float waitTime = GetWaitTimeForChar(c, tagProcessor.CurrentSpeed);
                    if (waitTime > 0)
                        await ToSignal(GetTree().CreateTimer(waitTime), "timeout");

                    if (!char.IsWhiteSpace(c))
                        audioModule.PlaySound(sound, 0.2f, (float)GD.RandRange(0.7f, 0.9f));

                    cleanText += c;
                    charIndex++;
                }
            }
            else if (token is OpenTagToken open)
            {
                string openText = tagProcessor.HandleOpenTag(open);
                if (openText != null)
                    cleanText += openText;
            }
            else if (token is CloseTagToken close)
            {
                string closeText = tagProcessor.HandleCloseTag(close);
                if (closeText != null)
                    cleanText += closeText;
            }

            i++;
        }

        audioModule.StopAll();
        isTyping = false;
    }


    private static string BuildClosingTags(IEnumerable<string> tagStack)
    {
        if (!tagStack.Any())
            return string.Empty;

        var closingTags = new System.Text.StringBuilder();

        foreach (var openTag in tagStack)
        {
            var tagName = openTag.Split(' ')[0].TrimStart('[');
            closingTags.Insert(0, "[/" + tagName.TrimEnd(']') + "]");
        }

        return closingTags.ToString();
    }

    void CompleteText()
    {
        
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
