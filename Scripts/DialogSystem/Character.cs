using Godot;
using System;
using System.Reflection.Metadata.Ecma335;
public class Character(string name, AudioStream voiceSample)
{
    public string Name { get; set; } = name;
    string name => Name;
    public AudioStream VoiceSample { get; set; } = voiceSample;
    public static Character Default => new("Jaime Altozano", null);
}