using Godot;
using Godot.Collections;

[GlobalClass]
public partial class SoundsLibrary : Resource
{
    [Export] public Dictionary<string, AudioStream> SoundsStored = [];
}