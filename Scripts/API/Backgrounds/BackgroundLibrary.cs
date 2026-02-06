using Godot;

[GlobalClass]
public partial class BackgroundLibrary : Resource
{
    [Export] public Godot.Collections.Dictionary<string, Texture2D> BackgroundsStored = [];
}