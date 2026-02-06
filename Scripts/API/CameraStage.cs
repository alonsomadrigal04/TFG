using System;
using Godot;
using Godot.Collections;

public partial class CameraStage : Node
{
    public static CameraStage Instance { get; private set; }

    [Export] Control layerDialogBox;
    [Export] Control layerBackground;


    public override void _Ready()
    {
        if (Instance != null && Instance != this)
        {
            QueueFree();
            return;
        }
        Instance = this;
    }

    public void CameraShake(float duration = 0.01f, float intensity = 1f)
    {
        AnimateShake(duration, intensity, layerDialogBox);
        //AnimateShake(duration, intensity, layerBackground);
    }

    private void AnimateShake(float duration, float intensity, Control canvasToAnimate)
    {
        Tween tween = CreateTween();

        Vector2 basePosition = canvasToAnimate.Position;
        int shakes = 4;

        tween.SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);

        for (int i = 0; i < shakes; i++)
        {
            Vector2 offset = new Vector2(
                (float)GD.RandRange(-1f, 1f),
                (float)GD.RandRange(-1f, 1f)
            ) * (intensity * 0.1f) * 100f;

            tween.TweenProperty(
                canvasToAnimate,
                "position",
                basePosition + offset,
                duration / shakes
            );
        }

        tween.TweenProperty(canvasToAnimate, "position", basePosition, 0.05f);
    }

    public void CameraZoom(Vector2 zoomPosition, float seconds)
    {
        Tween tween = CreateTween();

        tween.SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);

        layerDialogBox.PivotOffset = zoomPosition;

        tween.TweenProperty(layerDialogBox, "scale", new Vector2(1.2f, 1.2f), seconds);
        //tween.SetParallel().TweenProperty(camera2D, "position", zoomPosition, seconds);

    }

    public void ResetCamera(Vector2 newPosition)
    {
        Tween tween = CreateTween();

        tween.SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);

        tween.TweenProperty(layerDialogBox, "pivot_offset", newPosition, 0.2f);
        tween.SetParallel().TweenProperty(layerDialogBox, "scale", new Vector2(1f, 1f), 0.2f);
    }
}