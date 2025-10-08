using Game.Common.Modules;
using Godot;
using System;
using System.Collections.Generic;
using System.Data.Common;

public partial class TextTyper : Control
{   
    [Export] MultiaudioPlayerModule audioModule;
    [Export] AudioStream sound;
    [Export] RichTextLabel dialogBox;
    [Export] RichTextLabel nameBox;
    
    const float textSpeedDefaul = 0.1f;
    static readonly Character characterDefault = Character.Default;
    Dictionary<string, AudioStream> speakerSounds;

    public override void _Ready()
    {
        WriteText("Hola, soy [b] una [i] prueba[/i] y [wave amp=50.0 freq=5.0 connected=1]espero[/wave][/b] que funcione, bien.", null,0.02f);
    }

    public async void WriteText(string text, Character speaker, float textSpeed = textSpeedDefaul)
    {
        speaker ??= characterDefault;
        nameBox.Text = speaker.Name;
        dialogBox.Text = "";

        string colorVisible = "[color=#ffffffff]";
        string colorHidden = "[color=#ffffff00]";
        string colorEnd = "[/color]";

        Stack<string> tagStack = new();
        string cleanText = "";

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

                    if (tag.StartsWith("[/"))
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

            if (text[i] != ' ' && text[i] != '\n' && text[i] != '\t')
            {
                await ToSignal(GetTree().CreateTimer(textSpeed), "timeout");
            }

            string visiblePart = cleanText + text[i];

            string closingTags = "";
            foreach (string openTag in tagStack)
            {
                closingTags += "[/" + openTag + "]";
            }

            string visibleText = $"{colorVisible}{visiblePart}{closingTags}{colorEnd}";
            string hiddenText = $"{colorHidden}{text.Substring(i + 1)}{colorEnd}";

            dialogBox.Text = visibleText + hiddenText;

            if (text[i] != ' ' && text[i] != '\n' && text[i] != '\t')
            {
                audioModule.PlaySound(sound, 0.2f, (float)GD.RandRange(0.7f, 0.9f));
            }

            cleanText += text[i];
            i++;
        }

        audioModule.StopAll();
    }

}
