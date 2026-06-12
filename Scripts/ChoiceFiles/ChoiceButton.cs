using Components;
using Game.Common.Modules;
using Godot;
using System;

[GlobalClass]
public partial class ChoiceButton : Button
{
    [Signal] public delegate void SelectedSignalEventHandler(int uid);

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

    [ExportGroup("BUTTONS ELEMENTS")]
    [Export] public Label choiceLabel;
    [Export] public TextureRect choiceTexture;


    AudioStreamPlayer audioPlayer;
    Tween tween;
    Vector2 originalScale;
    
    float rotationSpeed = 4f;

    Vector2 baseScale;
    Vector2 basePosition;
    float baseRotation;

    public int Uid;

    Tween hoverTween;
    Tween clickTween;
    float hoverVelocity = 16f;
    float idleVelocity = 4f;


    public override void _Process(double delta)
    {
        RotationDegrees += rotationSpeed * (float)delta;
        choiceLabel.RotationDegrees -= rotationSpeed * (float)delta;
    }


    public override void _Ready()
    {
        baseScale = Scale;
        basePosition = Position;
        baseRotation = RotationDegrees;
        ConnectSignals();

        originalScale = Scale;

        audioPlayer = new AudioStreamPlayer
        {
            VolumeDb = volume
        };
        
        AddChild(audioPlayer);

        

        Resized += OnResized;
        choiceTexture.Resized += OnResized;

    }

    void OnResized()
    {
        PivotOffset = Size / 2f;
        choiceTexture.PivotOffset = choiceTexture.Size / 2f;
        choiceTexture.Position = Vector2.Zero;
    }

    void OnRelease()
    {
        bool stillHovered = GetRect().HasPoint(
            GetLocalMousePosition()
        );

        float targetScale = stillHovered ? hoverScale : 1f;
        AnimateTo(Vector2.One * targetScale, hoverDuration, Tween.EaseType.Out);
    }


    void OnHoverExit()
    {
        AnimateTo(originalScale, hoverDuration, hoverEase);
        rotationSpeed = idleVelocity;
    }

    void OnClick()
    {
        PlaySound(ClickSound);
        AnimateTo(Vector2.One * clickScale, clickDuration, Tween.EaseType.In);
        EmitSignal(nameof(SelectedSignal), Uid);
    }

    void OnHoverEnter()
    {
        PlaySound(HoverSound);
        AnimateTo(Vector2.One * hoverScale, hoverDuration, hoverEase);
        rotationSpeed = hoverVelocity;
    }

    void ConnectSignals()
    {
        MouseEntered += OnHoverEnter;
        MouseExited  += OnHoverExit;
        ButtonDown   += OnClick;
        ButtonUp     += OnRelease;
    }

    void StopHoverShake()
    {
        hoverTween?.Kill();

        Position = basePosition;
        RotationDegrees = baseRotation;
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

    public void SetProperties(float width, int uid)
    {
        CustomMinimumSize = new Vector2(width, width);
        choiceTexture.CallDeferred(Control.MethodName.SetSize, new Vector2(width, width));
        Size = new(width, width);

        SizeFlagsHorizontal = SizeFlags.ShrinkCenter | SizeFlags.Expand;
        SizeFlagsVertical = SizeFlags.ShrinkCenter;
        Uid = uid;
        VisibilityLayer = 10;

    }
}
