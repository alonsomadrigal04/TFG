using Godot;

[GlobalClass]
public partial class NpcData : Resource
{
    [Export] public string Name { get; set; }
    [Export] public Texture2D WordPortrait { get; set; }
}