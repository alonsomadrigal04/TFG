using Components;
using Game.Common.Modules;
using Godot;
using System;

[GlobalClass]
public partial class ChoiceButton : Button
{
    [Signal] public delegate void SelectedSignalEventHandler(int uid);
    float hoverShakeAmount = 15f;
    float hoverShakeSpeed = 1f;
    float clickScale = 0.9f;

    Vector2 baseScale;
    Vector2 basePosition;
    float baseRotation;

    public int Uid;

    Tween hoverTween;
    Tween clickTween;

    AudioStream hoverSound;
    AudioStream pressSound;


    public override void _Ready()
    {
        baseScale = Scale;
        basePosition = Position;
        baseRotation = RotationDegrees;
        ConnectSignals();

        // pressSound = GD.Load<AudioStream>("res://Audio/Sounds/Buttons/press.wav");
        // hoverSound = GD.Load<AudioStream>("res://Audio/Sounds/Buttons/Hover2.wav");
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
        //TODO: add soudns to this
        AnimateClick();
        //AudioManager.Instance.;
        EmitSignal(nameof(SelectedSignal), Uid);
    }


    void StartHoverShake()
    {
        StopHoverShake();
        Vector2 originalPosition = Position;

        //AudioManager.PlayAudio(hoverSound);

        hoverTween = CreateTween();
        hoverTween.SetLoops();
        hoverTween.SetTrans(Tween.TransitionType.Quad)
                .SetEase(Tween.EaseType.InOut);

        // hoverTween.TweenProperty(this, "position", Position + new Vector2(Position.X, Position.Y + hoverShakeAmount), hoverShakeSpeed);
        // hoverTween.TweenProperty(this, "position", originalPosition + new Vector2(Position.X, Position.Y - hoverShakeAmount), hoverShakeSpeed);

        hoverTween.TweenProperty(this, "rotation_degrees", RotationDegrees + 5f, hoverShakeSpeed);
        hoverTween.TweenProperty(this, "rotation_degrees", RotationDegrees - 10f, hoverShakeSpeed);

    }

    void StopHoverShake()
    {
        hoverTween?.Kill();

        Position = basePosition;
        RotationDegrees = baseRotation;
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
