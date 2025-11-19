using Game.Common.Modules;
using Godot;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;


public partial class TextTyper : Control
{
    [ExportGroup("Audio")]
    [Export] MultiaudioPlayerModule audioModule;
    //[Export] AudioStream sound;
    [ExportGroup("Text Boxes")]
    [Export] RichTextLabel dialogBox;
    [Export] RichTextLabel nameBox;
    [ExportGroup("preset parameters ")]
    [Export] float speedMultiplier = 1.2f;


    public bool isTyping;
    public bool skipRequested = false;
    TagProcessor tagProcessor;

    public override void _EnterTree()
    {
        tagProcessor = new TagProcessor();
    }

    public async void WriteText(string text, Character speaker)
    {
        isTyping = true;
        nameBox.Text = $"[color=#{speaker.TextColor.ToHtml()}]{speaker.Name}[/color]";
        dialogBox.Text = "";
        skipRequested = false;

        string cleanText = "";
        var tokens = TagParser.Parse(text);

        for (int i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];

            if (token is TextToken textToken)
            {
                cleanText = await WriteTextToken(textToken, cleanText, tokens, i, speaker);
            }
            else
            {
                ProcessTag(token, ref cleanText);
            }
        }

        string closingTags = BuildClosingTags(tagProcessor.ActiveEffects);
        dialogBox.Text = $"[color=#{speaker.TextColor.ToHtml()}]{cleanText}{closingTags}[/color]";

        audioModule.StopAll();
        isTyping = false;
    }

     async Task<string> WriteTextToken(TextToken textToken, string cleanText, List<TagToken> tokens, int tokenIndex, Character speaker)
    {
        int charIndex = 0;
        while (charIndex < textToken.Content.Length)
        {

            char c = textToken.Content[charIndex];
            string visiblePart = cleanText + c;
            string closingTags2 = BuildClosingTags(tagProcessor.ActiveEffects);
            string remainingText = BuildRemainingText(tokens, tokenIndex, charIndex + 1);


            dialogBox.Text = $"[color=#{speaker.TextColor.ToHtml()}]{visiblePart}{closingTags2}[/color]" +
                             $"[color=#ffffff00]{remainingText}[/color]";

            float waitTime = skipRequested ? 0 : GetWaitTimeForChar(c, tagProcessor.CurrentSpeed);;

            if (waitTime > 0)
                await ToSignal(GetTree().CreateTimer(waitTime), "timeout");

            if (!char.IsWhiteSpace(c))
                audioModule.PlaySound(speaker.VoiceSample, 0.2f, (float)GD.RandRange(0.7f, 0.9f));

            cleanText += c;
            charIndex++;
        }

        return cleanText;
    }


     string BuildRemainingText(List<TagToken> tokens, int currentTokenIndex, int charIndexInToken)
    {
        string remainingText = "";

        if (tokens[currentTokenIndex] is TextToken t)
            remainingText = t.Content[charIndexInToken..];

        for (int j = currentTokenIndex + 1; j < tokens.Count; j++)
            if (tokens[j] is TextToken t2)
                remainingText += t2.Content;

        return remainingText;
    }

     void ProcessTag(TagToken token, ref string cleanText)
    {
        switch(token)
        {
            case OpenTagToken open:
                string openText = tagProcessor.HandleOpenTag(open);
                if (openText != null) cleanText += openText;
                break;
            case CloseTagToken close:
                string closeText = tagProcessor.HandleCloseTag(close);
                if (closeText != null) cleanText += closeText;
                break;
        }
    }

     static string BuildClosingTags(IEnumerable<string> tagStack)
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
