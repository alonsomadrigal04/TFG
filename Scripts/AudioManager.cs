using Godot;
using System;

public partial class AudioManager : Node
{
    [Export] public AudioStreamPlayer2D Hover {get; set;}
    [Export] public AudioStreamPlayer2D Press {get; set;}
    [Export] public AudioStreamPlayer2D Shake {get; set;}
    [Export] public AudioStreamPlayer2D Talk {get; set;}
    [Export] public AudioStreamPlayer2D Impact {get; set;}
    [Export] public AudioStreamPlayer2D Flashback {get; set;}
    [Export] public AudioStreamPlayer2D Flash {get; set;}
    [Export] public AudioStreamPlayer2D NextSentence {get; set;}

}
