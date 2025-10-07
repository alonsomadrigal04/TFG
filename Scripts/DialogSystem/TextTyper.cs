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
        WriteText("H[b]O[/b]LA [b]Alonso[/b] Madrigalaa Hernandez y esto es una prueba", null,1f);
    }

    public async void WriteText(string text, Character speaker, float textSpeed = textSpeedDefaul)
    {
        speaker ??= characterDefault;
        nameBox.Text = speaker.Name;
        dialogBox.Text = "";

        const string COLOR_VISIBLE = "[color=#ffffffff]";
        const string COLOR_HIDDEN = "[color=#ffffff03]";
        const string COLOR_END = "[/color]";

        var visibleBuilder = new System.Text.StringBuilder();
        var tagStack = new Stack<string>();

        int i = 0;
        while (i < text.Length)
        {
            if (text[i] == '[')
            {
                int closing = text.IndexOf(']', i);
                if (closing != -1)
                {
                    string tag = text.Substring(i, closing - i + 1);
                    if (tag.StartsWith("[/"))
                    {
                        if (tagStack.Count > 0)
                            tagStack.Pop();
                    }
                    else
                    {
                        tagStack.Push(tag);
                    }
                    visibleBuilder.Append(tag);
                    i = closing + 1;
                    continue;
                }
            }
            await ToSignal(GetTree().CreateTimer(textSpeed), "timeout");

            string openTags = string.Concat(tagStack);

            string visibleText = openTags + COLOR_VISIBLE + visibleBuilder + text[i] + COLOR_END;
            string hiddenText = string.Concat(openTags, COLOR_HIDDEN, text.AsSpan(i + 1), COLOR_END);

            foreach (var tag in tagStack)
            {
                string closeTag = string.Concat("[/", tag.AsSpan(1));
                visibleText += closeTag;
                hiddenText += closeTag;
            }

            dialogBox.Text = visibleText + hiddenText;
            visibleBuilder.Append(text[i]);
            i++;
        }

        audioModule.StopAll();
    }


}
