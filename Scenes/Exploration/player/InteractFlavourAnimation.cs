using Godot;
using System;
using System.Drawing;
using Utility;

public partial class InteractFlavourAnimation : Sprite3D
{
    Tween activeTween;
    Vector3 originalScale;


    public override void _Ready()
    {
        originalScale = Scale;
        Hide();
    }


    public void ShowInteraction()
    {
        activeTween?.Kill();

        Scale = Vector3.Zero;
        Show();

        activeTween = CreateTween()
        .SetTrans(Tween.TransitionType.Back)
        .SetEase(Tween.EaseType.Out);

        activeTween.TweenDelegate<Vector3>(
            value => Scale = value,
            Vector3.Zero,
            originalScale,
            0.2f
        );
    }

    public void HideInteraction()
    {
        activeTween?.Kill();

        activeTween = CreateTween()
        .SetTrans(Tween.TransitionType.Quad)
        .SetEase(Tween.EaseType.In);

        activeTween.TweenDelegate<Vector3>(
            value => Scale = value,
            Scale,
            Vector3.Zero,
            0.15f
        );

        activeTween.TweenCallback(Callable.From(Hide));
    }
}