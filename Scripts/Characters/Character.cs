using Godot;
using System;

public partial class Character(string name) : Resource
{
    public string Name { get => name; private set => name = value; }
    public AudioStream VoiceSample { get; set; }
    public Color TextColor { get; set; }
    public CharacterState characterState;
    public PackedScene ActorScene;
    public static Character Default => new("Jaime Altozano");


    public void SetAffinity(int value) => characterState.Afinity += value;
}