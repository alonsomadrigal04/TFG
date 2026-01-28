using System;
using Godot;
using Godot.Collections;

public partial class CameraStage : Node
{
    public static CameraStage Instance { get; private set; }
    [Export] Camera2D camera2D; // TODO: consider change this to GetViewport()

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
        Tween tween = CreateTween();

        Vector2 basePosition = camera2D.Position;
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
                camera2D,
                "position",
                basePosition + offset,
                duration/shakes
            );
        }

        tween.TweenProperty(camera2D, "position", basePosition, 0.05f);
    }

    public void CameraZoom(Vector2 zoomPosition, float seconds)
    {
        Tween tween = CreateTween();

        tween.SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);

        tween.TweenProperty(camera2D, "zoom", 1.5f, 0.2f);
        tween.SetParallel().TweenProperty(camera2D, "position", zoomPosition, seconds);

    }
}