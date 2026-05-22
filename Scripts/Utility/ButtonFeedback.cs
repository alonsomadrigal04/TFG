// ButtonFeedback.cs
// Enchúfalo como nodo hijo de cualquier Button
using Godot;

public partial class ButtonFeedback : Button
{
    [ExportGroup("Sound")]
    [Export] public AudioStream HoverSound;
    [Export] public AudioStream ClickSound;
    [Export] float volume = 0f; 

    [ExportGroup("Hover Animation")]
    [Export] float hoverScale = 1.08f;
    [Export] float hoverDuration = 0.12f;
    [Export] Tween.EaseType hoverEase = Tween.EaseType.Out;

    [ExportGroup("Click Animation")]
    [Export] float clickScale = 0.93f;
    [Export] float clickDuration = 0.08f;

    AudioStreamPlayer audioPlayer;
    Tween tween;
    Vector2 originalScale;

    public override void _Ready()
    {
        originalScale = Scale;

        audioPlayer = new AudioStreamPlayer
        {
            VolumeDb = volume
        };
        
        AddChild(audioPlayer);

        MouseEntered += OnHoverEnter;
        MouseExited  += OnHoverExit;
        ButtonDown   += OnClick;
        ButtonUp     += OnRelease;

        Resized += () => PivotOffset = Size / 2f;
    }
    void OnHoverEnter()
    {
        PlaySound(HoverSound);
        AnimateTo(Vector2.One * hoverScale, hoverDuration, hoverEase);
    }

    void OnHoverExit()
    {
        AnimateTo(originalScale, hoverDuration, hoverEase);
    }

    void OnClick()
    {
        PlaySound(ClickSound);
        AnimateTo(Vector2.One * clickScale, clickDuration, Tween.EaseType.In);
    }

    void OnRelease()
    {
        bool stillHovered = GetRect().HasPoint(
            GetLocalMousePosition()
        );
        float targetScale = stillHovered ? hoverScale : 1f;
        AnimateTo(Vector2.One * targetScale, hoverDuration, Tween.EaseType.Out);
    }

    void AnimateTo(Vector2 targetScale, float duration, Tween.EaseType ease)
    {
        PivotOffset = Size / 2f;

        tween?.Kill();
        tween = CreateTween();
        tween.SetEase(ease);
        tween.SetTrans(Tween.TransitionType.Back);
        tween.TweenProperty(this, "scale", targetScale, duration);
    }

    void PlaySound(AudioStream stream)
    {
        if (stream == null) return;
        audioPlayer.Stream = stream;
        audioPlayer.Play();
    }
}