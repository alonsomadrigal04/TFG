using Godot;
using System;
using System.Collections.Generic;

public partial class TextTyper : Control
{
    [Export] RichTextLabel textBox;
    const float textSpeedDefaul = 0.1f;
    Dictionary<string, AudioStream> speakerSounds;

    public override void _Ready()
    {
        WriteText("HOLA MUNDO", null);
    }

    public async void WriteText(string text, string speaker, float textSpeed = textSpeedDefaul)
    {
        int index = 0;
        Timer timerSpeech = new() { WaitTime = textSpeed, Autostart = true };
        textBox.Text = "";
        foreach (char c in text)
        {
            await ToSignal(GetTree().CreateTimer(textSpeed), "timeout");
            timerSpeech.WaitTime = textSpeed;
            textBox.Text += text[index];
            index += 1;
        }
    }
}
