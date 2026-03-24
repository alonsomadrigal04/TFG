using Godot;
using System;
using System.Collections.Generic;
using System.Resources;

public static partial class ToolKit
{
    static public readonly Dictionary<ScreenPosition, float> XPositions = [];
    public static void InitializePositions()
    {
        XPositions[ScreenPosition.FarLeft]  = 0.1f;
        XPositions[ScreenPosition.Left]     = 0.25f;
        XPositions[ScreenPosition.Center]   = 0.5f;
        XPositions[ScreenPosition.Right]    = 0.75f;
        XPositions[ScreenPosition.FarRight] = 0.9f;
    }

    public static Vector2 GetPosition(ScreenPosition screenPosition)
    {
        Vector2 screenSize = DisplayServer.WindowGetSize();
        return new Vector2(screenSize.X * XPositions[screenPosition], screenSize.Y * 0.5f);
    }

    public static Vector2 GetScreenSize() => DisplayServer.WindowGetSize();

    /// <summary>
    /// Adds a percentage-based offset to the position of a Control node. The offset is calculated based on the size of the screen, allowing for responsive positioning. The direction parameter can be used to specify whether the offset should be added (positive) or subtracted (negative) from the current position of the Control node.
    /// </summary>
    /// <param name="control">The Control node to offset.</param>
    /// <param name="percentageX">The percentage of the screen width to offset by.</param>
    /// <param name="percentageY">The percentage of the screen height to offset by.</param>
    /// <param name="direction">The direction of the offset (1 for positive, -1 for negative).</param>
    /// <returns>The new position of the Control node after applying the offset.</returns>
    public static Vector2 AddPercentageOffset(
    this Control control,
    float percentageX,
    float percentageY,
    float direction = 1f)
    {
        if (percentageX > 100f || percentageY > 100f)
        {
            GD.PushWarning("[Toolkit] Offset greater than 100% of screen size");
        }

        Vector2 screenSize = DisplayServer.WindowGetSize();

        Vector2 offset = new(
            screenSize.X * (percentageX * 0.01f),
            screenSize.Y * (percentageY * 0.01f)
        );

        control.Position += offset * direction;

        return control.Position;
    }

    public static ScreenPosition GetScreenSide(Vector2 position)
    {
        Vector2 screenSize = DisplayServer.WindowGetSize();
        float centerX = screenSize.X * 0.5f;

        if (position.X < centerX)
            return ScreenPosition.Left;

        return ScreenPosition.Right;
    }


    public static Vector2 GetScreenPosition(ScreenPosition screenPosition)
    {
        if (!XPositions.TryGetValue(screenPosition, out float xNorm))
        {
            GD.PrintErr($"[ToolKit] the {screenPosition} position is not recognized");
            return Vector2.Zero;
        }

        Vector2 screenSize = DisplayServer.WindowGetSize();

        return new Vector2(
            screenSize.X * xNorm,
            screenSize.Y * 0.5f
        );
    }




    /// <summary>
    /// Sets the position of a Control node based on a predefined screen position.
    /// </summary>
    /// <param name="control">The Control node to position.</param>
    /// <param name="screenPosition">The desired screen position.</param>
    public static void SetPosition(Control control, ScreenPosition screenPosition) => control.Position = GetPosition(screenPosition);

    /// <summary>
    /// Gets the anchor position for a given screen position.
    /// </summary>
    /// <param name="screenPosition">The screen position enum value.</param>
    /// <returns>The anchor position as a float.</returns>
    public static float GetAnchorPosition(ScreenPosition screenPosition)
    {
        if (XPositions.TryGetValue(screenPosition, out float value))
            return value;
        else
            GD.PrintErr($"[ToolKit] the {screenPosition} position is not recognized");
        return 0;
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