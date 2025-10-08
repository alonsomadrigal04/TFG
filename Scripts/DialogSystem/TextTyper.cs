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
        WriteText("HOLA [b]Alonso[/b] Madrigalaa Hernandez y esto es una prueba", null,3f);
    }

    public async void WriteText(string text, Character speaker, float textSpeed = textSpeedDefaul)
    {
        speaker ??= characterDefault;
        nameBox.Text = speaker.Name;
        dialogBox.Text = "";

        string colorVisible = "[color=#ffffffff]";
        string colorHidden = "[color=#ffff00]";
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
                    if (tag.StartsWith("[/"))
                    {
                        if (tagStack.Count > 0)
                            tagStack.Pop();
                    }
                    else
                    {
                        tagStack.Push(tag);
                    }

                    cleanText += tag;
                    i = closingBracket + 1;
                    continue;
                }
            }

            await ToSignal(GetTree().CreateTimer(textSpeed), "timeout");

            string visiblePart = cleanText + text[i];

            string closingTags = "";
            foreach (string openTag in tagStack)
            {
                string tagName = openTag.Trim('[', ']');
                closingTags = "[/" + tagName + "]" + closingTags;
            }

            string visibleText = $"{colorVisible}{visiblePart}{closingTags}{colorEnd}";
            string hiddenText = $"{colorHidden}{text.Substring(i + 1)}{colorEnd}";

            dialogBox.Text = visibleText + hiddenText;

            cleanText += text[i];
            i++;
        }

        audioModule.StopAll();
    }





}
