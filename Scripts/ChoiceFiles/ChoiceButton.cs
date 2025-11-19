using Godot;
using System;

[GlobalClass]
public partial class ChoiceButton : Button
{
    [Signal] public delegate void SelectedSignalEventHandler(int uid);
    [Export] float hoverShakeAmount = 1f;
    [Export] float hoverShakeSpeed = 0.01f;
    [Export] float clickScale = 0.9f;

    Vector2 baseScale;
    Vector2 basePosition;

    public int Uid;

    Tween hoverTween;
    Tween clickTween;

    public override void _Ready()
    {
        baseScale = Scale;
        basePosition = Position;
        ConnectSignals();
    }

    void ConnectSignals()
    {
        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
        Pressed += OnPressed;
    }

    void OnMouseEntered()
    {
        StartHoverShake();
    }

    void OnMouseExited()
    {
        StopHoverShake();
    }

    void OnPressed()
    {
        AnimateClick();
        EmitSignal(nameof(SelectedSignal), Uid);
    }


    void StartHoverShake()
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

    void StopHoverShake()
    {
        hoverTween?.Kill();

        Position = basePosition;
    }


    void AnimateClick()
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
