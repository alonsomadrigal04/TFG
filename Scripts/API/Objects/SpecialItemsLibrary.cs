using Godot;
using Godot.Collections;

[GlobalClass]
public partial class SpecialItemsLibrary : Resource
{
    [Export] public Dictionary<string, Texture2D> Itemstored = [];
}