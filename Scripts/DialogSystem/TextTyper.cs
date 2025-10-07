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
        WriteText("HOLA MUNDO... me llamo Alonso Madrigalaa Hernandez y esto es una prueba", null,2);
    }

    public async void WriteText(string text, Character speaker, float textSpeed = textSpeedDefaul)
    {
        speaker ??= characterDefault;
        nameBox.Text = speaker.Name;

        text += "[/color]";
        text = "[color=#ffffff03]" + text;

        dialogBox.Text = "[color=#ffffffff][/color] " + text;
        var currentText = new System.Text.StringBuilder("[color=#ffffffff][/color]" + text);
        int index2 = 42;
        for (int indexToDisplay = 17; indexToDisplay < text.Length - 8; indexToDisplay++)
        {
            await ToSignal(GetTree().CreateTimer(textSpeed), "timeout");
            audioModule.PlaySound(sound, 1, (float)GD.RandRange(0.9f, 1f));
            currentText[indexToDisplay] = currentText[index2];
            GD.Print("pongo " + currentText[index2]);
            currentText = currentText.Remove(index2, 1);
            dialogBox.Text = currentText.ToString();
        }
        audioModule.StopAll();
    }
}
