using Godot;

public partial class CodexToast : Control
{
    [Export] Sprite2D toastIcon;
    [Export] AudioStream notificationSound;
    [Export] AudioStreamPlayer playerSound;
    Vector2 originalToasPosition;
    public override void _Ready()
    {
        toastIcon.Hide();
        CodexLibrary.codexActualized += throwToast;

        originalToasPosition = toastIcon.Position;
    }
    public override void _ExitTree()
    {
        CodexLibrary.codexActualized -= throwToast;
    }

    void throwToast()
    {
        Show();
        toastIcon.Show();
        playerSound.Stream = notificationSound;
        playerSound.Play();

        toastIcon.Modulate = new Color(1, 1, 1, 0);
        toastIcon.Position = originalToasPosition - new Vector2(0, 40);

        var tween = CreateTween();
        tween.SetTrans(Tween.TransitionType.Quint).SetEase(Tween.EaseType.Out);
        tween.SetParallel();
        tween.TweenProperty(toastIcon, "modulate:a", 1f, 2f);
        tween.TweenProperty(toastIcon, "position", originalToasPosition, 2f);

        tween.SetParallel(false);
        tween.TweenInterval(1f);

        tween.SetParallel(true);
        tween.TweenProperty(toastIcon, "modulate:a", 0f, 0.7f);
        tween.TweenProperty(toastIcon, "position", originalToasPosition - new Vector2(0, 40), 0.7f);

        tween.Finished += toastIcon.Hide;

    }

}
