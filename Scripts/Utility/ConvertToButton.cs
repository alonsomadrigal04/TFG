using Godot;
using Game.Common.Modules;
using System;

[Tool]
public partial class ConvertToButton : Node2D
{
    [Signal] public delegate void PressedEventHandler();

    [ExportGroup("Audio")]
    [Export] public MultiaudioPlayerModule audioModule;
    [Export] public AudioStream hoverSound;
    [Export] public AudioStream clickSound;

    [ExportGroup("Animation")]
    [Export] public float hoverScale = 1.1f;
    [Export] public float hoverDuration = 0.15f;
    [Export] public float clickScale = 0.9f;
    [Export] public float clickDuration = 0.1f;

    private Vector2 originalScale;
    private Tween tween;

    public override void _Ready()
    {
        originalScale = Scale;
        tween = GetNodeOrNull<Tween>("_Tween");
        if (tween == null)
        {
            tween = GetTree().CreateTween();
        }

        SetProcessInput(true);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouse motionEvent)
        {
            if (motionEvent is InputEventMouseMotion)
            {
                OnHover();
            }
            else if (motionEvent is InputEventMouseButton buttonEvent && buttonEvent.Pressed && buttonEvent.ButtonIndex == MouseButton.Left)
            {
                OnClick();
            }
        }
    }

    private void OnHover()
    {
        tween?.Kill();
        tween?.TweenProperty(this, "scale", originalScale * hoverScale, hoverDuration).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);

        if (audioModule != null && hoverSound != null)
            audioModule.PlaySound(hoverSound);
    }

    private void OnClick()
    {
        tween?.Kill();

        if (tween != null)
        {
            tween.TweenProperty(this, "scale", originalScale * clickScale, clickDuration)
                .SetTrans(Tween.TransitionType.Sine)
                .SetEase(Tween.EaseType.Out)
                .Finished += () =>
                {
                    tween.TweenProperty(this, "scale", originalScale, clickDuration)
                        .SetTrans(Tween.TransitionType.Sine)
                        .SetEase(Tween.EaseType.Out);
                };
        }

        if (audioModule != null && clickSound != null)
            audioModule.PlaySound(clickSound);

        EmitSignal(nameof(Pressed));
    }
}
