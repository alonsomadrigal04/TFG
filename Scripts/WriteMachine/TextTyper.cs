using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


public partial class TextTyper : Control
{
    [ExportGroup("Text Boxes")]
    [Export] RichTextLabel dialogBox;
    [Export] TextureRect dialogBoxFrame;

    [Export] RichTextLabel nameBox;
    [Export] SentenceCompleteHud sentenceHud;
    AudioStreamRandomizer audioStreamRandomizer;
    [ExportGroup("Text Boxes Style")]
    [Export] Godot.Collections.Dictionary<TextboxTypes, Texture2D> textBoxesStyle = [];

    public bool isTyping;
    public bool skipRequested = false;
    TagProcessor tagProcessor;
    public bool isSentenceComplete = false;
    public bool isAcceptPressed = false;

    public bool isFasterDialog = false;


    public override void _EnterTree()
    {

        audioStreamRandomizer = new AudioStreamRandomizer
        {
            RandomPitch = 1.3f,
        };
        AudioManager.Instance.Talk.Stream = audioStreamRandomizer;
        tagProcessor = new TagProcessor();
        dialogBox.Text = "";
        nameBox.Text = "";
    }

    public void CleanTextBox()
    {
        dialogBox.Text = "";
        nameBox.Text = "";
    }

    public async void WriteText(string text, Character speaker)
    {
        isTyping = true;
        isSentenceComplete = false;
        sentenceHud.StopAndReset();
        if(speaker.Name == "Narrator")
            nameBox.Text = "";
        else
            nameBox.Text = $"[color=#{speaker.TextColor.ToHtml()}]{speaker.Name}[/color]";
        dialogBox.Text = "";
        skipRequested = false;
        
        if(audioStreamRandomizer.StreamsCount >= 1)
            audioStreamRandomizer.RemoveStream(0);
        audioStreamRandomizer.AddStream(0, speaker.VoiceSample);

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
        string colorText = speaker.TextColor.ToHtml();
            if(CharacterStage.IsThinking)
                colorText = Colors.Green.ToHtml();
        //dialogBox.Text = $"[color=#{colorText}]{cleanText}{closingTags}[/color]";

        //audioModule.StopAll();
        isTyping = false;
        isSentenceComplete = true;
        sentenceHud.StartHudAnimation();
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

            string colorText = speaker.TextColor.ToHtml();
            if(CharacterStage.IsThinking)
                colorText = Colors.Green.ToHtml();
            dialogBox.Text = $"[color=#{colorText}]{visiblePart}{closingTags2}[/color]" +
                             $"[color=#ffffff00]{remainingText}[/color]";

            float waitTime = skipRequested ? 0 : GetWaitTimeForChar(c, tagProcessor.CurrentSpeed);;

            if (waitTime > 0)
                await ToSignal(GetTree().CreateTimer(waitTime), "timeout");

            if (!char.IsWhiteSpace(c) & c != '.' & c != '!' & c != '?' & !isFasterDialog)
                AudioManager.Instance.Talk.Play();

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

    float GetWaitTimeForChar(char c, float baseSpeed)
    {
        if(isFasterDialog || isAcceptPressed)
            return 0.00001f;

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

    public void ChangeTextBox(TextboxTypes textboxType)
    {
        dialogBoxFrame.Texture = textboxType switch
        {
            TextboxTypes.Transparent => textBoxesStyle[TextboxTypes.Transparent],
            TextboxTypes.Normal => textBoxesStyle[TextboxTypes.Normal],
            _ => textBoxesStyle[TextboxTypes.Normal],
        };

    }

}

public enum TextboxTypes
{
    Transparent,
    Normal
} 

public enum TextboxPosition
{
    Center,
    Down
}
