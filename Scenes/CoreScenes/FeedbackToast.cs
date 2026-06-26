using Godot;

public partial class FeedbackToast : HBoxContainer
{
    public Label LeftLabel { get; private set; }
    public Label RightLabel { get; private set; }
    public TextureRect InputIcon { get; private set; }

    public override void _Ready()
    {
        LeftLabel   = GetNode<Label>("LeftLabel");
        RightLabel  = GetNode<Label>("RightLabel");
        InputIcon   = GetNode<TextureRect>("TextureIcon");
    }

    public void Setup(string leftText, Texture2D icon, string rightText)
    {
        LeftLabel.Text   = leftText;
        InputIcon.Texture = icon;
        RightLabel.Text  = rightText;
    }
}

