using System;
using Godot;
using Godot.Collections;

public partial class UiStage : Node
{
    public static UiStage Instance { get; private set; }

    [Export] Control textBox;

    public override void _Ready()
    {
        if (Instance != null && Instance != this)
        {
            QueueFree();
            return;
        }
        Instance = this;
    }

    public void HideTextBox()
    {
        textBox.Hide();
    }
    public void ShowTextBox()
    {
        textBox.Show();
    }

    public bool IsTextBoxHide() =>  !textBox.Visible;

    public void AnimateShowTextBox()
    {
        ActionBus.ActionStarted();
        
        Tween tween = CreateTween();
        textBox.Show();
        Vector2 originalPosition = textBox.Position;
        textBox.Position = new Vector2(originalPosition.X, originalPosition.Y + 50);

        textBox.Modulate = textBox.Modulate with { A = 0 };
        tween.SetTrans(Tween.TransitionType.Quint)   
            .SetEase(Tween.EaseType.Out);

        tween.TweenProperty(textBox, "modulate:a", 1f, 0.5f);
        tween.SetParallel().TweenProperty(textBox, "position:y", originalPosition.Y, 0.5f);

        tween.Finished += ActionBus.ActionFinished;
    }

}