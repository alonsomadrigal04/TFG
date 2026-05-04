using Godot;
using System;
using System.Threading.Tasks;

public partial class CharacterActor : Control
{
    [Export]public AnimatedSprite2D animateSprites;

    [Export] float bobIntensity = 15f;
    bool isFlipped;
    Vector2 basePosition;
    bool isTalking;
    string currentEmotion;
    bool isTransitioning;

    /// <summary>
    /// Plays an emotion transition sequence:
    /// CurrentIdle -> CurrentOut -> NewIn -> NewIdle
    /// </summary>
    public async Task PlayEmotion(string emotion)
    {
        if (isTransitioning || string.IsNullOrWhiteSpace(emotion))
            return;

        emotion = NormalizeEmotion(emotion);

        if (!HasAnimation($"{emotion}Idle"))
        {
            GD.PrintErr($"[CharacterActor] Missing idle animation for emotion: {emotion}");
            return;
        }

        isTransitioning = true;

        if (!string.IsNullOrEmpty(currentEmotion))
            await PlayIfExists($"{currentEmotion}Out");

        await PlayIfExists($"{emotion}In");

        PlayLoop($"{emotion}Idle");

        currentEmotion = emotion;
        isTransitioning = false;
    }

    /// <summary>
    /// Instantly sets the current idle animation without transitions.
    /// </summary>
    public void SetEmotionImmediate(string emotion)
    {
        emotion = NormalizeEmotion(emotion);

        string idle = $"{emotion}Idle";

        if (!HasAnimation(idle))
        {
            GD.PrintErr($"[CharacterActor] Missing idle animation: {idle}");
            return;
        }

        animateSprites.Play(idle);
        currentEmotion = emotion;
    }

    async Task PlayIfExists(string animation)
    {
        if (!HasAnimation(animation))
            return;

        animateSprites.Play(animation);

        await ToSignal(animateSprites, AnimatedSprite2D.SignalName.AnimationFinished);
    }

    void PlayLoop(string animation)
    {
        if (!HasAnimation(animation))
        {
            GD.PrintErr($"[CharacterActor] Missing loop animation: {animation}");
            return;
        }

        animateSprites.Play(animation);
    }

    bool HasAnimation(string animation) => animateSprites.SpriteFrames.HasAnimation(animation);

    static string NormalizeEmotion(string emotion)
    {
        if (string.IsNullOrEmpty(emotion))
            return string.Empty;

        return char.ToUpper(emotion[0]) + emotion[1..];
    }


    public override void _Ready()
    {
        basePosition = Position;
    }

    public void PlayTalk()
    {
        if (isTalking)
            return;

        isTalking = true;
        DoTalkTween();
    }

    void DoTalkTween()
    {
        ActionBus.ActionStarted();

        Tween tween = CreateTween();

        tween.SetLoops(2);

        tween.TweenProperty(
            this,
            "position",
            basePosition + new Vector2(0, -bobIntensity),
            0.1f
        );

        tween.TweenProperty(
            this,
            "position",
            basePosition,
            0.1f
        );

        tween.Finished += () =>
        {
            isTalking = false;
            ActionBus.ActionFinished();
        };
    }

    public void FlipHorizontal()
    {
        ActionBus.ActionStarted();

        float duration = 0.5f;
        float half = duration * 0.5f;

        Tween tween = CreateTween();

        tween.SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
        Vector2 spriteScale = animateSprites.Scale;

        tween.TweenProperty(animateSprites, "scale", new Vector2(0f, spriteScale.Y), half);

        tween.TweenCallback(Callable.From(() =>
        {
            isFlipped = !isFlipped;
            animateSprites.FlipH = isFlipped;
        }));

        tween.TweenProperty(animateSprites, "scale", spriteScale, half);

        tween.Finished += ActionBus.ActionFinished;
    }

    public void SetFacing(ScreenPosition position)
    {
        bool shouldFlip =
            position == ScreenPosition.Right ||
            position == ScreenPosition.FarRight;

        if(shouldFlip)
            FlipHorizontal();

    }
}