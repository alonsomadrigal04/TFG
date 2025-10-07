using Godot;
using System;
using System.Reflection.Metadata.Ecma335;
public class Character(string name, AudioStream voiceSample)
{
    public string Name { get => name; private set => name = value; }
    public AudioStream VoiceSample { get; private set; } = voiceSample;
    public static Character Default => new("Jaime Altozano", null);
}