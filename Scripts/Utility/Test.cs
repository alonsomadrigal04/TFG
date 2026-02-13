using Godot;
using Godot.Collections;
using System;

public partial class Test : Node
{
    [Export] Dictionary<string, Resource> semen;


    public override void _Ready()
    {
        base._Ready();
    }

}
