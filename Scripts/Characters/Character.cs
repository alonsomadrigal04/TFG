using Godot;
using System;

[GlobalClass]
public partial class Character(string name) : Resource
{
    
    [Export] public string Name { get => name; private set => name = value; }
    [Export] public AudioStream VoiceSample { get; set; }
    [Export] public Color TextColor { get; set; }
    public CharacterState characterState;
    [Export] public PackedScene ActorScene;
    public static Character Default => new("Jaime Altozano");


    public void SetAffinity(int value) => characterState.Afinity += value;
}