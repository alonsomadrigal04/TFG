using Godot;
public static class ButtonExtensions
{
    public static void SetFontSize(this Button button, int size)
    {
        button.AddThemeFontSizeOverride("font_size", size);
    }
}
