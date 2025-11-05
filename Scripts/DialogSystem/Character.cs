using Godot;
using System;

[GlobalClass]
public partial class Character(string name, AudioStream voiceSample) : Resource
{
    public string Name { get => name; private set => name = value; }
    public AudioStream VoiceSample { get; private set; } = voiceSample;
    public Color TextColor { get; set; }
    public static Character Default => new("Jaime Altozano", null);
}