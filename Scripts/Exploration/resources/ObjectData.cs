using Godot;
using System;

[GlobalClass]
public partial class ObjectData : Resource
{
    [Export] public string Name { get; set; }
    [Export] public Mesh Model { get; set; }
	[Export] public Material Material { get; set; }
	[Export] public Texture2D Icon { get; set; }
    [Export] public string Description { get; set; }
    [Export] public string Comentary { get; set; }


}