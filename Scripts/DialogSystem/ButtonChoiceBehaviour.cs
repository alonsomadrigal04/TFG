using Godot;
using System;

public partial class ButtonChoiceBehaviour : Button
{
    [Export] private float hoverScale = 1.05f;
    [Export] private float clickScale = 0.9f;
    [Export] private float idleScaleAmplitude = 0.02f;
    [Export] private float idleSpeed = 1.5f;

    private Vector2 baseScale;
    private Tween idleTween;

    public Vector2 RectScale { get; private set; }

    public override void _Ready()
    {
        baseScale = RectScale;
        StartIdleAnimation();
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
        AnimateScale(baseScale * hoverScale, 0.15f, Tween.TransitionType.Quint, Tween.EaseType.Out);
    }

    private void OnMouseExited()
    {
        AnimateScale(baseScale, 0.2f, Tween.TransitionType.Quint, Tween.EaseType.Out);
    }

    private void OnPressed()
    {
        AnimateClick();
    }

    private void AnimateClick()
    {
        Tween tween = CreateTween();
        tween.TweenProperty(this, "rect_scale", baseScale * clickScale, 0.08f)
            .SetTrans(Tween.TransitionType.Quint)
            .SetEase(Tween.EaseType.Out);
        tween.Parallel()
            .TweenProperty(this, "rect_scale", baseScale, 0.08f)
            .SetTrans(Tween.TransitionType.Quint)
            .SetEase(Tween.EaseType.Out)
            .SetDelay(0.08f);
    }

    private void StartIdleAnimation()
    {
        idleTween = CreateTween();
        idleTween.SetLoops();
        idleTween.TweenProperty(this, "rect_scale",
                baseScale * (1 + idleScaleAmplitude),
                idleSpeed)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
    }

    private void AnimateScale(Vector2 targetScale, float duration,
        Tween.TransitionType trans = Tween.TransitionType.Quint,
        Tween.EaseType ease = Tween.EaseType.Out)
    {
        // Cancel idle while scaling to hover
        if (idleTween != null && idleTween.IsValid())
            idleTween.Kill();

        Tween tween = CreateTween();
        tween.TweenProperty(this, "rect_scale", targetScale, duration)
            .SetTrans(trans)
            .SetEase(ease);

        // Reinicia idle al terminar
        tween.Finished += () => StartIdleAnimation();
    }
}
