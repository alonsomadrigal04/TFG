using Godot;
using System;
[GlobalClass]
public partial class AudioManager : Node
{
    [ExportGroup("FLAVOUR TEXT")]
    [Export] public AudioStreamPlayer2D question {get; set;}
    [Export] public AudioStreamPlayer2D exclamation {get; set;}

    [ExportGroup("DECISION MAKING")]
    [Export] public AudioStreamPlayer2D Hover {get; set;}
    [Export] public AudioStreamPlayer2D Press {get; set;}

    [ExportGroup("VSN SOUNDS")]
    [Export] public AudioStreamPlayer2D Shake {get; set;}
    [Export] public AudioStreamPlayer2D Talk {get; set;}
    [Export] public AudioStreamPlayer2D Impact {get; set;}
    [Export] public AudioStreamPlayer2D Flashback {get; set;}
    [Export] public AudioStreamPlayer2D Flash {get; set;}

    [ExportGroup("UI SOUNDS")]
    [Export] public AudioStreamPlayer2D NextSentence {get; set;}

}
