using Godot;
using System;

public static partial class ToolKit
{
    public static Vector2 FarLeft  { get; private set; }
    public static Vector2 Left     { get; private set; }
    public static Vector2 Center   { get; private set; }
    public static Vector2 Right    { get; private set; }
    public static Vector2 FarRight { get; private set; }

    public static void InitializePositions(Vector2 ScreenSize)
    {
        var size = ScreenSize;
        float y = size.Y * 0.5f;

        FarLeft  = new Vector2(size.X * 0.1f,  y);
        Left     = new Vector2(size.X * 0.25f, y);
        Center   = new Vector2(size.X * 0.5f,  y);
        Right    = new Vector2(size.X * 0.75f, y);
        FarRight = new Vector2(size.X * 0.9f,  y);
    }

}
