using Godot;

[GlobalClass]
public partial class DebugScreen : Control
{
    [Export] bool enable = true;
    [Export] Label fpsLabel;
    Label label;

    public override void _Ready()
    {
        label = GetNode<Label>("Label");
    }

    public override void _Process(double delta)
    {
        if (enable)
        {
            label.Text = string.Join("\n", DebugService.GetInfo());
            fpsLabel.Text = $"{Engine.GetFramesPerSecond()} FPS";
        }
    }
}


