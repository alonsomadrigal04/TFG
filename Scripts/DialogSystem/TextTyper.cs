using Game.Common.Modules;
using Godot;
using System;
using System.Collections.Generic;

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
        WriteText("HOLA MUNDO", null);
        
    }

    public async void WriteText(string text, Character speaker, float textSpeed = textSpeedDefaul)
    {
        speaker ??= characterDefault;

        int index = 0;
        Timer timerSpeech = new() { WaitTime = textSpeed, Autostart = true };
        dialogBox.Text = "";
        nameBox.Text = speaker.Name;

        foreach (char c in text)
        {
            await ToSignal(GetTree().CreateTimer(textSpeed), "timeout");
            audioModule.PlaySound(sound, 1, GD.Randi() % 10 + 1);
            timerSpeech.WaitTime = textSpeed;
            dialogBox.Text += text[index];
            index += 1;
        }

        audioModule.StopAll();
    }
}
