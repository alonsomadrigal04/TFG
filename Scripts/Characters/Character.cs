using Godot;
using System;

[GlobalClass]
public partial class Character(string name) : Resource
{
    public string Name { get => name; private set => name = value; }
    public AudioStream VoiceSample { get; set; }
    public Color TextColor { get; set; }
    public Texture2D[] Portraits;
    public static Character Default => new("Jaime Altozano");
}