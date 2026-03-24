using Godot;
using System;
using System.Threading;
using System.Threading.Tasks;
using Utility;

public partial class ScreenTransition : ColorRect
{
    Tween transitionTween;
    [Export] AudioStreamPlayer fadeIn;
    [Export] AudioStreamPlayer fadeOut;


    void TransitionEffectParameterCall(float value)
    {
        if (this?.Material is not ShaderMaterial mat)
            return;

        mat.SetShaderParameter("progress", value);
        mat.SetShaderParameter("background_threshold", Mathf.Abs(1.0f - value * 2.0f) - 0.5f);
        mat.SetShaderParameter("color_threshold", Mathf.Min(1.0f, Mathf.Abs(-4.0f + value * 8.0f)) * 0.48f);
    }

    public async Task FadeIn(CancellationToken cancelationToken = default)
    {
        fadeIn.Play();
        transitionTween?.Kill();
        transitionTween = null;

        if (Material is ShaderMaterial mat)
            mat.SetShaderParameter("seed", GD.Randf());

        Show();

        transitionTween = CreateTween();

        transitionTween.TweenMethod(
            Callable.From<float>(TransitionEffectParameterCall),
            0.0f,
            0.5f,
            0.5f
        );
        await transitionTween.AwaitFinished(true, cancelationToken);
    }

    public async Task FadeOut(CancellationToken token = default)
    {
        fadeOut.Play();
        transitionTween?.Kill();
        transitionTween = null;

        transitionTween = CreateTween();

        transitionTween.TweenMethod(
            Callable.From<float>(TransitionEffectParameterCall),
            0.5f,
            1.0f,
            0.5f
        );

        await transitionTween.AwaitFinished(true, token);
        Hide();
    }
}