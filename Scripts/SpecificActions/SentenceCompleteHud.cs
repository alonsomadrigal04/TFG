using Godot;
using System;
using Utility;

public partial class SentenceCompleteHud : TextureRect
{
    [Export] float startOffsetY = -20f;
    [Export] float startDuration = 0.25f;

    [Export] float idleAmplitude = 6f;
    [Export] float idleDuration = 0.6f;

    Vector2 initialPosition;
    Tween startTween;
    Tween idleTween;

    public override void _Ready()
    {
        initialPosition = Position;
        ResetHud();
    }

    public void StartHudAnimation()
    {
        KillTweens();

        Position = initialPosition + new Vector2(0, startOffsetY);
        Modulate = new Color(1, 1, 1, 0);

        startTween = CreateTween();

        startTween.TweenDelegate<float>(
            value =>
            {
                var c = Modulate;
                c.A = value;
                Modulate = c;
            },
            0f,
            1f,
            startDuration
        );

        startTween.Parallel().TweenDelegate<Vector2>(
            value => Position = value,
            Position,
            initialPosition,
            startDuration
        );

        startTween.Finished += StartIdleLoop;
    }

    private void StartIdleLoop()
    {
        KillIdleTween();

        idleTween = CreateTween();
        idleTween.SetLoops();

        idleTween.TweenDelegate<Vector2>(
            value => Position = value,
            initialPosition,
            initialPosition + new Vector2(0, idleAmplitude),
            idleDuration
        ).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);

        idleTween.TweenDelegate<Vector2>(
            value => Position = value,
            initialPosition + new Vector2(0, idleAmplitude),
            initialPosition,
            idleDuration
        ).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
    }

    public void StopAndReset()
    {
        KillTweens();
        ResetHud();
    }

    void ResetHud()
    {
        Position = initialPosition + new Vector2(0, startOffsetY);
        Modulate = new Color(1, 1, 1, 0);
    }

    void KillTweens()
    {
        KillIdleTween();

        if (startTween != null && startTween.IsRunning())
            startTween.Kill();
    }

    void KillIdleTween()
    {
        if (idleTween != null && idleTween.IsRunning())
            idleTween.Kill();
    }
}