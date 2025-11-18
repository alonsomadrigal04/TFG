using Godot;
using System;

[GlobalClass]
public partial class ChoiceButton() : Button
{
    [Export] private float hoverShakeAmount = 1f;
    [Export] private float hoverShakeSpeed = 0.01f;
    [Export] private float clickScale = 0.9f;

    private Vector2 baseScale;
    private Vector2 basePosition;

    public int Uid;

    private Tween hoverTween;
    private Tween clickTween;

    public override void _Ready()
    {
        baseScale = Scale;
        basePosition = Position;
        ConnectSignals();
    }

    private void ConnectSignals()
    {
        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
        Pressed += OnPressed;
    }

    private void OnMouseEntered()
    {
        StartHoverShake();
    }

    private void OnMouseExited()
    {
        StopHoverShake();
    }

    private void OnPressed()
    {
        AnimateClick();
    }


    private void StartHoverShake()
    {
        StopHoverShake();

        hoverTween = CreateTween();
        hoverTween.SetLoops();
        hoverTween.SetTrans(Tween.TransitionType.Cubic);

        hoverTween.TweenCallback(Callable.From(() =>
        {
            Vector2 randomOffset = new(
                (float)GD.RandRange(-hoverShakeAmount, hoverShakeAmount),
                (float)GD.RandRange(-hoverShakeAmount, hoverShakeAmount)
            );
            Position = basePosition + randomOffset;
        }))
        .SetDelay(hoverShakeSpeed);
    }

    private void StopHoverShake()
    {
        hoverTween?.Kill();

        Position = basePosition;
    }


    private void AnimateClick()
    {
        clickTween?.Kill();
        clickTween = CreateTween();

        clickTween.TweenProperty(this, "scale", baseScale * clickScale, 0.08f)
            .SetTrans(Tween.TransitionType.Quint)
            .SetEase(Tween.EaseType.Out);

        clickTween.Parallel()
            .TweenProperty(this, "scale", baseScale, 0.08f)
            .SetTrans(Tween.TransitionType.Quint)
            .SetEase(Tween.EaseType.Out)
            .SetDelay(0.08f);
    }
}
