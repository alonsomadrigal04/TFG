using Godot;
using Godot.Collections;

[GlobalClass]
public partial class ObjectsLibrary : Resource
{
    [Export] public Dictionary<string, ObjectData> Itemstored = [];
}