using Godot;

[GlobalClass]
public partial class ChapterData : Resource
{
    [Export] public string Title;
    [Export] public string Subtitle;

    [Export] public ChapterType Type;
    [Export] public string StartConversation;
    [Export] public PackedScene ExplorationEnvironment;
}