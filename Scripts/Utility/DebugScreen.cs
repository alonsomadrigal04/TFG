using Godot;
using System;
using System.Collections.Generic;

public partial class DebugScreen : Control
{
    [Export] bool enable = true;
    Label label;

    public override void _Ready()
    {
        label = GetNode<Label>("Label");
    }

    public override void _Process(double delta)
    {
        if(enable)
            label.Text = string.Join("\n", DebugService.GetInfo());
    }
}


