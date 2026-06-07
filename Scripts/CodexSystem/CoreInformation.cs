using Godot;

public enum CodexType
{
    Character,
    Term,
    Localization
}

[GlobalClass]
public partial class CoreInformation : Resource
{
    [Export] public CodexType DisplayType;
    [Export] public string Title = "";
    [Export] public string SubTitle = "";
    [Export] public string Content = "";
    [Export] public bool Video = false;
    [Export] public Texture2D CharacterPortrait;
    [Export] public Texture2D CharacterEyes;

    [Export] public Texture2D[] SupportImages = [];
    [Export] public VideoStream videoStream;
}