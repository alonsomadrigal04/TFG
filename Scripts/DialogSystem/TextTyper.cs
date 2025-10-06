using Godot;
using System;
using System.Collections.Generic;

public partial class TextTyper : Control
{
    [Export] RichTextLabel textContainer;
    int defaultTextSpeed;
    Dictionary<string, AudioStream> speakerSounds;

    public void WriteText(string text, int textSpeed, string speaker)
    {

    } 
}
