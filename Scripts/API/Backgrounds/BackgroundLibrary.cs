using Godot;
using Godot.Collections;

[GlobalClass]
public partial class BackgroundLibrary : Resource
{
    [Export] public Dictionary<string, Texture2D> BackgroundsStored = [];
}