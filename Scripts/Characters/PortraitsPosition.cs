using Godot;
using System;

public partial class PortraitPosition : Resource
{
    public Vector2 Center;
    public Vector2 Left;

    

    public override void _Ready()
    {
        Center = GetViewport().GetVisibleRect().Size;
    }
}