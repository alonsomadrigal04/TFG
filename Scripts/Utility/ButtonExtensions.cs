using Godot;
public static class ButtonExtensions
{
    public static Button FontSize(this Button button, int size)
    {
        button.AddThemeFontSizeOverride("font_size", size);
        return button;
    }
}
