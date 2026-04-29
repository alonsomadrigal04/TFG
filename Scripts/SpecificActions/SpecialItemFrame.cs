using Godot;
using System;

public partial class SpecialItemFrame : Control
{
    [Export] public TextureRect itemIcon;
    [Export] public TextureRect itemFrame;

    public void SetItemIcon(Texture2D newIcon) => itemIcon.Texture = newIcon;
    public void SetFrameIcon(Texture2D newIcon) => itemIcon.Texture = newIcon;

}
