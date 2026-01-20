using Godot;
using System;
using System.Collections.Generic;

public static partial class ToolKit
{
    static readonly Dictionary<ScreenPosition, float> XPositions = [];
    public static void InitializePositions(Vector2 ScreenSize)
    {
        var size = ScreenSize;
        float y = size.Y * 0.5f;

        XPositions[ScreenPosition.FarLeft]  = 0.1f;
        XPositions[ScreenPosition.Left]     = 0.25f;
        XPositions[ScreenPosition.Center]   = 0.5f;
        XPositions[ScreenPosition.Right]    = 0.75f;
        XPositions[ScreenPosition.FarRight] = 0.9f;
    }

    /// <summary>
    /// Sets the position of a Control node based on a predefined screen position.
    /// </summary>
    /// <param name="control">The Control node to position.</param>
    /// <param name="screenPosition">The desired screen position.</param>
    public static void SetPosition(Control control, ScreenPosition screenPosition)
    {
        float x = XPositions[screenPosition];

        control.AnchorLeft = x;
        control.AnchorRight = x;
        control.AnchorBottom = 0.5f;
        control.AnchorTop = 0.5f;
    
        control.OffsetBottom = control.Size.Y / 2;
        control.OffsetTop = -control.Size.Y / 2;
        control.OffsetLeft = -control.Size.X / 2;
        control.OffsetRight = control.Size.X / 2;
    }

    /// <summary>
    /// Parses a string to an enum of type T. 
    /// </summary>
    /// <typeparam name="T">The enum type to parse to.</typeparam>
    /// <param name="value">The string representation of the enum value.</param>
    /// <returns>The parsed enum value of type T.</returns>
    public static T ParseEnum<T>(string value)
    {
        return (T)Enum.Parse(typeof(T), value, true);
    }

    /// <summary>
    /// Extracts a ScreenPosition from the arguments of a CommandToken.
    /// </summary>
    /// <param name="commandToken">The CommandToken containing the arguments.</param>
    /// <returns>The extracted ScreenPosition.</returns>
    public static ScreenPosition FromArguments(CommandToken commandToken)
    {
        if(commandToken.Arguments.Count > 1)
        {
            GD.PrintErr($"Too many arguments for screen position: {string.Join(", ", commandToken.Arguments)}");
        }
        else if(commandToken.Arguments.Count == 1)
        {
            return ParseEnum<ScreenPosition>(commandToken.Arguments[0]);
        }
            return ScreenPosition.Center;
    }

}

public enum ScreenPosition
{
    FarLeft,
    Left,
    Center,
    Right,
    FarRight
    
}